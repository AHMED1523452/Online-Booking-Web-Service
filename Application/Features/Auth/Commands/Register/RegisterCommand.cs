using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Net.Mail;
using System.Security.Cryptography;

namespace Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    string? Phone = null,
    int RoleId = 1
) : IRequest<GenericResult<ForgotPasswordResponseDTO>>;

public sealed class RegisterCommandValidator 
                        : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.RoleId)
    .GreaterThanOrEqualTo(0).WithMessage("RoleId must be a valid role.");
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, GenericResult<ForgotPasswordResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService emailService;
    private readonly ILogger<RegisterCommandHandler> logger;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        this.emailService = emailService;
        this.logger = logger;
    }

    public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _context.passengers
            .AnyAsync(p => p.email == request.Email, cancellationToken)) return await Result.FailureAsync<ForgotPasswordResponseDTO>($"A user with email '{request.Email}' already exists.");


        if (!await _context.roles
            .AnyAsync(r => r.id == request.RoleId, cancellationToken)) return await Result.FailureAsync<ForgotPasswordResponseDTO>($"Role with ID {request.RoleId} does not exist.");

        var hashedPassword = await _passwordHasher.HashPassword(request.Password, cancellationToken);
        var user = new passenger
        {
            name = request.Name,
            email = request.Email,
            password_hash = hashedPassword,
            phone = request.Phone,
            role_id = request.RoleId,
            status = "unverified",
            created_at = DateTime.UtcNow
        };
        try
        {
            string emailConfirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            user.EmailConfirmationTokenHash = await _passwordHasher
                                           .HashPassword(emailConfirmationToken, cancellationToken);
            user.EmailConfirmationTokenExpiry = DateTime.Now.AddMinutes(5);

            await _context.passengers.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            string htmlBody = await MailBody.ConfirmEamilMailBody(user, emailConfirmationToken, cancellationToken);
            if (htmlBody is null) throw new Exception("Something invalid occurred. ");

            await emailService.SendEmail(user.email, "Confirm Your Email Address", htmlBody);


            return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
            {
                Message = "Registration completed successfully. Please check your email to confirm your account."
            });

        }catch(SmtpException ex)
        {
            logger.LogError("Something invalid occurred, the exception occurred in the part of the SMTP exception. ");
            throw new SmtpException(ex.Message);
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}

using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using Domain.Entities;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Security.Cryptography;

namespace Application.Features.Users.Handlers
{
    public sealed class ChangeEmailHandler : IRequestHandler<ChangeUserEmailCommand, GenericResult<ChangeEmailResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPasswordHasher passwordHasher;
        private readonly IEmailService emailService;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<ChangeEmailHandler> logger;

        public ChangeEmailHandler(IUnitOfWork unitOfWork, 
                                  IPasswordHasher passwordHasher,
                                  IEmailService emailService,
                                  ICurrentIUserService currentIUser,
                                  ILogger<ChangeEmailHandler> logger)
        {
            this.unitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.emailService = emailService;
            this.currentIUser = currentIUser;
            this.logger = logger;
        }
        public async Task<GenericResult<ChangeEmailResponseDTO>> Handle(ChangeUserEmailCommand request, CancellationToken cancellationToken)
        {
            var user_instance = unitOfWork.Repository<passenger>();
            if (user_instance is null) throw new ArgumentNullException(nameof(user_instance));

            try
            {
                if (await user_instance.AnyAsync(predicate: op => op.email == request.requestDTO.NewEmail, cancellationToken))
                    return await Result.FailureAsync<ChangeEmailResponseDTO>("Invalid data. ");
                var existing_user = await user_instance.GetByIdAsync(predicate: op =>
                                                                               op.email ==currentIUser.Email  && 
                                                                                op.IsDeleted == false &&
                                                                                op.status == "verified" &&
                                                                                op.is_email_verified == true,
                                                                                cancellationToken);
                if (existing_user is null) return await Result.FailureAsync<ChangeEmailResponseDTO>("User not found. ");

                if (!await passwordHasher.VerifyPassword(request.requestDTO.CurrentPassword, existing_user.password_hash, cancellationToken))
                    return await Result.FailureAsync<ChangeEmailResponseDTO>("Invalid data. ");

                var bytes = RandomNumberGenerator.GetBytes(64);
                string EmailConfirmationToken = Convert.ToBase64String(bytes);

                existing_user.email = request.requestDTO.NewEmail;
                existing_user.updated_at = DateTime.UtcNow;
                existing_user.UpdatedBy = currentIUser.UserId;
                existing_user.EmailConfirmationTokenHash = await passwordHasher.HashPassword(EmailConfirmationToken, cancellationToken);
                existing_user.EmailConfirmationTokenExpiry = DateTime.Now.AddDays(1);
                existing_user.status = "unverified";
                existing_user.EmailConfirmedAt = null;
                existing_user.is_email_verified = false;

                await unitOfWork.SaveChangesAsync(cancellationToken);

                string htmlBody = await MailBody.ChangeEmailHtmlBody(existing_user, EmailConfirmationToken, cancellationToken);
                if (htmlBody is null) throw new Exception("Something invalid occurred. ");

                await emailService.SendEmail(request.requestDTO.NewEmail, "TravelBooking - Confirm Your New Email Address", htmlBody);

                return await Result.SuccessAsync<ChangeEmailResponseDTO>(new ChangeEmailResponseDTO
                {
                    Message = "A confirmation email has been sent to your new email address."
                });
            }
            catch(SmtpException ex)
            {
                logger.LogError("Something invalid occurred, the exception occurred in the part of the SMTP exception. ");
                throw new SmtpException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

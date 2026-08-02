using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using FluentValidation.Results;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Issuing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Auth.Handler
{
    public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailService emailService;
        private readonly ILogger<ForgotPasswordHandler> logger;
        private readonly IPasswordHasher passwordHasher;
        private readonly ICurrentIUserService currentIUserService;

        public ForgotPasswordHandler(IUnitOfWork unitOfWork, 
                                     IEmailService emailService,
                                     ILogger<ForgotPasswordHandler> logger,
                                     IPasswordHasher passwordHasher,
                                     ICurrentIUserService currentIUserService)
        { 
            this.unitOfWork = unitOfWork;
            this.emailService = emailService;
            this.logger = logger;
            this.passwordHasher = passwordHasher;
            this.currentIUserService = currentIUserService;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var passenger_instance = unitOfWork.Repository<passenger>();
            if (passenger_instance is null) throw new ArgumentNullException(nameof(passenger_instance));

            var existing_passenger = await passenger_instance.GetByIdAsync(predicate: op => op.email == request.requestDTO.Email && 
                                                                                             op.IsDeleted == false && 
                                                                                             op.status == "verified", cancellationToken);
            if (existing_passenger is null)
                return await Result.FailureAsync<ForgotPasswordResponseDTO>("User not found. ");
            try
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                var generatedForgotPasswordToken = Convert.ToBase64String(bytes);

                string htmlBody = await MailBody.mailBody(existing_passenger, generatedForgotPasswordToken, cancellationToken);

                logger.LogInformation(
                                    "Attempting to send password reset email to {Email}",
                                    existing_passenger.email);

                existing_passenger.resetPasswordToken = await passwordHasher
                                                    .HashPassword(generatedForgotPasswordToken, cancellationToken);
                existing_passenger.resetPasswordTokenExpired = DateTime.Now.AddMinutes(5);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                await emailService.SendEmail(existing_passenger.email, "Password Reset Request", htmlBody);

                logger.LogInformation(
                    "User {UserName} forgot password has been sent to him an email at {SentAt}",
                    existing_passenger.name,
                    DateTime.Now);

                return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
                {
                    Message = "If email is valid, there is token had been sent to you"
                });
            }
            catch (SmtpException ex)
            {
                logger.LogError("Something fault occured at forgot password handler");
                Console.WriteLine(ex.Message);
                throw new SmtpException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Data.Values.ToString());
            }
        }
    }
}

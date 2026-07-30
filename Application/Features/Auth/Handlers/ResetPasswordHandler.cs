using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCoammand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPasswordHasher passwordHasher;
        private readonly ILogger<ResetPasswordHandler> logger;

        public ResetPasswordHandler(IUnitOfWork unitOfWork,
                                   IPasswordHasher passwordHasher,
                                   ILogger<ResetPasswordHandler> logger)
        {
            this.unitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.logger = logger;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(ResetPasswordCoammand request, CancellationToken cancellationToken)
        {
            var passenger_instance = unitOfWork.Repository<passenger>();
            if (passenger_instance is null) throw new ArgumentNullException(nameof(passenger_instance));

            try
            {
                var existing_passenger = await passenger_instance
                        .GetByIdAsync(predicate: op => op.email == request.requestDTO.Email &&
                                                       op.IsDeleted == false &&
                                                       op.is_revoked == false && 
                                                       op.is_email_verified == true  && 
                                                       op.status == "verified",
                                                       cancellationToken);

                if (!await passwordHasher.VerifyPassword(request.requestDTO.Token, existing_passenger.resetPasswordToken, cancellationToken))
                    return await Result.FailureAsync<ForgotPasswordResponseDTO>("Invalid data. ");

                if (existing_passenger is null)
                    return await Result.FailureAsync<ForgotPasswordResponseDTO>("Passenger not found. ");

                if (existing_passenger.resetPasswordTokenExpired < DateTime.UtcNow)
                    return await Result.FailureAsync<ForgotPasswordResponseDTO>("Token revoked.");

                existing_passenger.password_hash = await passwordHasher.HashPassword(request.requestDTO.NewPassword, cancellationToken);
                existing_passenger.resetPasswordTokenExpired = null;

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
                {
                    Message = "Password has been reseted successfully."
                });
            }catch(Exception ex)
            {
                logger.LogError("Something invalid occurred in Reset Password handler. ");
                throw new Exception(ex.Message);
            }
        }
    }
}

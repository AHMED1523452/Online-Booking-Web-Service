using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.ChangePassword;
using Application.Features.Auth.DTOs;
using Application.Features.Hotels.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe.Terminal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public sealed class ChangePasswordHandler : IRequestHandler<ChangePassengerPasswordCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPasswordHasher passwordHasher;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<ChangePasswordHandler> logger;

        public ChangePasswordHandler(IUnitOfWork unitOfWork,
                                     IPasswordHasher passwordHasher,
                                     ICurrentIUserService currentIUser,
                                     ILogger<ChangePasswordHandler> logger)
        {
            this.unitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.currentIUser = currentIUser;
            this.logger = logger;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(ChangePassengerPasswordCommand request, CancellationToken cancellationToken)
        {
            var passenger_instance = unitOfWork.Repository<passenger>();
            if (passenger_instance is null) throw new ArgumentNullException(nameof(passenger_instance));
            try
            {
                var existing_passenger = await passenger_instance
                                .GetByIdAsync(predicate: op => op.IsDeleted == false &&
                                                               op.is_revoked == false && 
                                                               op.is_email_verified == true && 
                                                               op.status == "verified"
                                                               , cancellationToken);

                if (existing_passenger is null) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Passenger not found .");

                if (!await passwordHasher.VerifyPassword(request.requestDTO.OldPassword, existing_passenger.password_hash, cancellationToken))
                    return await Result.FailureAsync<ForgotPasswordResponseDTO>("Invalid data.");

                bool valid_old_password = await passwordHasher.VerifyPassword(request.requestDTO.OldPassword, existing_passenger.password_hash, cancellationToken);
                if (valid_old_password is false) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Invalid data sent. ");

                existing_passenger.password_hash = await passwordHasher.HashPassword(request.requestDTO.NewPassword, cancellationToken);
                existing_passenger.updated_at = DateTime.UtcNow;
                existing_passenger.UpdatedBy = currentIUser.UserId;

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
                {
                    Message = "Password changed successfully. "
                });
            } catch (Exception ex)
            {
                logger.LogError("Something invalid occurred in changing password handler. ");
                throw new Exception(ex.Message);
            }
        }
    }
}

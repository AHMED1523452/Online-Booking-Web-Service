using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.ConfirmEmail;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using MediatR.Pipeline;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPasswordHasher passwordHasher;
        private readonly ICurrentIUserService currentIUser;

        public ConfirmEmailHandler(IUnitOfWork unitOfWork,
                                   IPasswordHasher passwordHasher,
                                   ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var passenger_instance = unitOfWork.Repository<passenger>();
            if (passenger_instance is null) throw new ArgumentNullException(nameof(passenger_instance));

            var existing_passenger = await passenger_instance.GetByIdAsync(op => op.email == request.requestDTO.Email &&
                                                                                op.IsDeleted == false &&
                                                                                op.is_revoked == false ,
                                                                                cancellationToken);
            if (existing_passenger is null) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Passenger not found. ");

            if (existing_passenger.is_email_verified == true || existing_passenger.status == "verified")
                return await Result.FailureAsync<ForgotPasswordResponseDTO>("Email is already verified. ");

            if (await passwordHasher.VerifyPassword(request.requestDTO.Token, existing_passenger.EmailConfirmationTokenHash, cancellationToken) is false)
                return await Result.FailureAsync<ForgotPasswordResponseDTO>("Invalid data. ");

            if (existing_passenger.EmailConfirmationTokenExpiry < DateTime.UtcNow)
                return await Result.FailureAsync<ForgotPasswordResponseDTO>("Email confirmation link has expired. Please request a new confirmation email.");

            existing_passenger.is_email_verified = true;
            existing_passenger.EmailConfirmationTokenExpiry = null;
            existing_passenger.EmailConfirmedAt = DateTime.UtcNow;
            existing_passenger.EmailConfirmationTokenHash = null;
            existing_passenger.updated_at = DateTime.UtcNow;
            existing_passenger.UpdatedBy = existing_passenger.id;
            existing_passenger.status = "verified";

            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
            {
                Message = "Email confirmed successfully. Please login."
            });
        }
    }
}

using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.RevokePassenger;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe.Terminal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public sealed class RevokePassengerHandler : IRequestHandler<RevokeRefreshTokenCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<RevokePassengerHandler> logger;

        public RevokePassengerHandler(IUnitOfWork unitOfWork, 
                                      ICurrentIUserService currentIUser,
                                      ILogger<RevokePassengerHandler> logger)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.logger = logger;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var passenger_instance = unitOfWork.Repository<passenger>();
            if (passenger_instance is null) throw new ArgumentNullException(nameof(passenger_instance));

            var existing_passenger = await passenger_instance
                                .GetByIdAsync(predicate: op => op.refreshToken == request.requestDTO.RefreshToken &&
                                                          op.IsDeleted == false &&
                                                          op.is_revoked == false,
                                                          cancellationToken);
            if (existing_passenger is null) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Passenger not found. ");

            try
            {
                existing_passenger.is_revoked = true;
                existing_passenger.IsDeleted = true;
                existing_passenger.updated_at = DateTime.UtcNow;
                existing_passenger.UpdatedBy = currentIUser.UserId;
                existing_passenger.DeletedAt = DateTime.UtcNow;

                await unitOfWork.SaveChangesAsync(cancellationToken);
                return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
                {
                    Message = "Passenger revoked successfully. "
                });
            }catch (Exception ex)
            {
                logger.LogError("Something invalid occurred in revoke refresh passenger handler. ");
                throw new ArgumentNullException(ex.Message);
            }
        }
    }
}
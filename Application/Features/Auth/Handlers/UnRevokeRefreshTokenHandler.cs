using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.RevokeTokenPassenger.UnRevokePassengerToken;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
namespace Application.Features.Auth.Handlers
{
    public  class UnRevokeRefreshTokenHandler : IRequestHandler<UnRevokePassengerCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<UnRevokeRefreshTokenHandler> logger;

        public UnRevokeRefreshTokenHandler(IUnitOfWork unitOfWork,
                                             ICurrentIUserService currentIUser, ILogger<UnRevokeRefreshTokenHandler> logger
                                             )
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(UnRevokePassengerCommand request, CancellationToken cancellationToken)
        {
            var passenger_instance = unitOfWork.Repository<passenger>();
            if (passenger_instance is null) throw new ArgumentNullException(nameof(passenger_instance));

            var existing_passenger = await passenger_instance
                                .GetByIdAsync(predicate: op => op.refreshToken == request.requestDTO.RefreshToken &&
                                                          op.IsDeleted == true &&
                                                          op.is_revoked == true,
                                                          cancellationToken);
            if (existing_passenger is null) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Passenger not found. ");

            try
            {
                existing_passenger.is_revoked = false;
                existing_passenger.IsDeleted = false;
                existing_passenger.updated_at = DateTime.UtcNow;
                existing_passenger.UpdatedBy = currentIUser.UserId;
                existing_passenger.DeletedAt = DateTime.UtcNow;

                await unitOfWork.SaveChangesAsync(cancellationToken);
                return await Result.SuccessAsync<ForgotPasswordResponseDTO>(new ForgotPasswordResponseDTO
                {
                    Message = "Passenger became unrevoked successfully. "
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Something invalid occurred in revoke refresh passenger handler. ");
                throw new ArgumentNullException(ex.Message);
            }
        }
    }
}

using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.UnRevokePassengerToken;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
namespace Application.Features.Auth.Handlers
{
    public  class UnRevokeRefreshTokenHandler : IRequestHandler<UnRevokePassengerCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IApplicationDbContext dbContext;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<UnRevokeRefreshTokenHandler> logger;

        public UnRevokeRefreshTokenHandler(IApplicationDbContext dbContext,
                                           ICurrentIUserService currentIUser,
                                           ILogger<UnRevokeRefreshTokenHandler> logger)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(UnRevokePassengerCommand request, CancellationToken cancellationToken)
        {
            //. un revoke the last refresh token for the user and also un delete the user.
            var existing_token = await dbContext.refreshTokens
                                                    .Include(op => op.User)
                                                    .Where(op => op.UserId == request.requestDTO.UserId
                                                              && op.IsRevoked == true)
                                                    .OrderByDescending(op => op.CreatedAt)
                                                    .FirstOrDefaultAsync();

            if (existing_token is null) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Invalid User Id. ");

            try
            {
                existing_token.IsRevoked = false;
                existing_token.RevokedAt = DateTime.UtcNow; 
                
                existing_token.User.IsDeleted = false;
                existing_token.User.updated_at = DateTime.UtcNow;
                existing_token.User.UpdatedBy = currentIUser.UserId;
                existing_token.User.DeletedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
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

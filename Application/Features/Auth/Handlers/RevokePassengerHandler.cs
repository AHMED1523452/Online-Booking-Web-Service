using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.RevokeTokenPassenger;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe.Terminal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public sealed class RevokePassengerHandler : IRequestHandler<RevokeRefreshTokenCommand, GenericResult<ForgotPasswordResponseDTO>>
    {
        private readonly IApplicationDbContext dbContext;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<RevokePassengerHandler> logger;

        public RevokePassengerHandler(IApplicationDbContext dbContext,
                                      ICurrentIUserService currentIUser,
                                      ILogger<RevokePassengerHandler> logger)
        {
            this.dbContext = dbContext;
            this.currentIUser = currentIUser;
            this.logger = logger;
        }
        public async Task<GenericResult<ForgotPasswordResponseDTO>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {

            var existing_token = await dbContext.refreshTokens
                                                    .Include(op => op.User)
                                                    .Where(op => op.UserId == request.requestDTO.UserId
                                                              && op.IsRevoked == null)
                                                    .OrderByDescending(op => op.CreatedAt)
                                                    .FirstOrDefaultAsync();

            if (existing_token is null) return await Result.FailureAsync<ForgotPasswordResponseDTO>("Invalid User Id. ");

            try
            {
                
                existing_token.IsRevoked = true;
                existing_token.RevokedAt = DateTime.UtcNow;

                existing_token.User.IsDeleted = true;
                existing_token.User.updated_at = DateTime.UtcNow;
                existing_token.User.UpdatedBy = currentIUser.UserId;
                existing_token.User.DeletedAt = DateTime.UtcNow;

                //. Modifying the existing token and user entity to reflect the revocation and deletion status.
                await dbContext.SaveChangesAsync(cancellationToken);
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
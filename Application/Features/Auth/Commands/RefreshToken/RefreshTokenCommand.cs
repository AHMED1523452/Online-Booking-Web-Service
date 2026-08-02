using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<ApiResponse<RefreshTokenResponseDTO>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponseDTO>>
{
    private readonly IApplicationDbContext dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly HybridCache _cache;
    private readonly ICurrentIUserService currentIUser;
    private readonly IPasswordHasher passwordHasher;

    public RefreshTokenCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator,
        HybridCache cache,
        ICurrentIUserService currentIUser,
        IPasswordHasher passwordHasher)
    {
        this.dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _cache = cache;
        this.currentIUser = currentIUser;
        this.passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<RefreshTokenResponseDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {

        var existing_Token = await dbContext.refreshTokens
                                    .Include(op => op.User)
                                    .ThenInclude(op => op.role)
                                    .Where(op => op.UserId == currentIUser.UserId &&
                                                 op.IsRevoked == false)
                                    .OrderByDescending(op => op.CreatedAt)
                                    .FirstOrDefaultAsync(cancellationToken);

        if(!await passwordHasher.VerifyPassword(request.RefreshToken, existing_Token.TokenHash, cancellationToken))
            return await Task.FromResult(ApiResponse<RefreshTokenResponseDTO>.Fail("Invalid refresh token.", statusCode: 400));

        // 2. Check if token is expired
        if (existing_Token.ExpiresAt <= DateTime.UtcNow)
        {
            // Clear expired token details
            existing_Token.IsRevoked = true;
            existing_Token.RevokedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return ApiResponse<RefreshTokenResponseDTO>.Fail("Refresh token has expired. Please login again.", statusCode: 400);
        }

        // 3. Generate new JWT token and rotate refresh token
        var newRefreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(existing_Token.User, cancellationToken);


        //. Creating a new refresh token and updating the existing one to be revoked and replaced by the new token
        var token = new RefreshTokens
        {
            TokenHash = await passwordHasher.HashPassword(newRefreshToken, cancellationToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Set the expiry for the new refresh token
            UserId = existing_Token.UserId,
        };

        existing_Token.ExpiresAt = DateTime.UtcNow;
        existing_Token.IsRevoked = true;
        existing_Token.RevokedAt = DateTime.UtcNow;
        existing_Token.ReplacedByTokenId = token.Id;

        //. here you update the values for the refresh token and refresh token expiry not adding don't forget  
        await dbContext.refreshTokens.AddAsync(token, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 4. Evict the cached profile — the passenger record has changed (new refresh token).
        //    The next Login call will re-populate the cache from the DB.
        await _cache.RemoveAsync($"passenger-email:{currentIUser.Email}", cancellationToken);

        return ApiResponse<RefreshTokenResponseDTO>.Ok(
            new RefreshTokenResponseDTO(newRefreshToken,existing_Token.User.name, currentIUser.Email, existing_Token.User.role.name),
            "Token refreshed successfully."
        );
    }
}

using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
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
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly HybridCache _cache;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        HybridCache cache)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _cache = cache;
    }

    public async Task<ApiResponse<RefreshTokenResponseDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // NOTE: refreshToken is a random GUID — not suitable as a cache key.
        // We look it up from the DB, rotate it, and then evict the email-keyed profile.

        // 1. Find passenger with matching refresh token
        var user = await _context.passengers
            .Include(p => p.role)
            .FirstOrDefaultAsync(p => p.refreshToken == request.RefreshToken && 
                                      p.is_revoked == false && 
                                      p.IsDeleted == false && 
                                      p.is_email_verified == true &&
                                      p.status == "verified", cancellationToken);

        if (user is null)
        {
            return ApiResponse<RefreshTokenResponseDTO>.Fail("Invalid refresh token.", statusCode: 400);
        }

        // 2. Check if token is expired
        if (user.refresh_token_expiry < DateTime.Now )
        {
            // Clear expired token details
            user.refreshToken = null;
            user.refresh_token_expiry = default ;
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<RefreshTokenResponseDTO>.Fail("Refresh token has expired. Please login again.", statusCode: 400);
        }

        // 3. Generate new JWT token and rotate refresh token
        var newRefreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user, cancellationToken);

        user.refreshToken = newRefreshToken;
        user.refresh_token_expiry = DateTime.UtcNow.AddDays(7);

        //. here you update the values for the refresh token and refresh token expiry not adding don't forget  
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Evict the cached profile — the passenger record has changed (new refresh token).
        //    The next Login call will re-populate the cache from the DB.
        await _cache.RemoveAsync($"passenger-email:{user.email}", cancellationToken);

        return ApiResponse<RefreshTokenResponseDTO>.Ok(
            new RefreshTokenResponseDTO(newRefreshToken,user.name, user.email, user.role?.name ?? "Passenger"),
            "Token refreshed successfully."
        );
    }
}

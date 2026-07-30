using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateAccessTokenAsync(passenger user, CancellationToken cancellationToken);
    Task<string> GenerateRefreshTokenAsync(passenger user, CancellationToken cancellationToken);
}

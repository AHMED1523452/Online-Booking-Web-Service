namespace Application.Common.Interfaces;

public interface IPasswordHasher
{
    Task<string> HashPassword(string password, CancellationToken cancellationToken);
    Task<bool> VerifyPassword(string password, string hashedPassword, CancellationToken cancellationToken);
}

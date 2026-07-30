using Application.Common.Interfaces;

namespace Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public async Task< string> HashPassword(string password, CancellationToken cancellationToken)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    //
    public async Task< bool> VerifyPassword(string password, string hashedPassword, CancellationToken cancellationToken)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

using Microsoft.AspNetCore.Identity;

namespace Duotify.Membership.Web.Infrastructure.Security;

public sealed class PasswordHasherAdapter
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private static readonly object Marker = new();

    public string Hash(string password) => _passwordHasher.HashPassword(Marker, password);

    public bool Verify(string hashedPassword, string providedPassword)
        => _passwordHasher.VerifyHashedPassword(Marker, hashedPassword, providedPassword) is not PasswordVerificationResult.Failed;
}
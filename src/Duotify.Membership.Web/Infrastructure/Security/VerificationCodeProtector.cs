using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Duotify.Membership.Web.Infrastructure.Security;

public sealed class VerificationCodeProtector(IOptions<VerificationCodeOptions> options)
{
    private readonly VerificationCodeOptions _options = options.Value;

    public (string Code, string CodeHash) GenerateCode(Guid challengeId)
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        var code = value.ToString("D6");
        return (code, ComputeHash(challengeId, code));
    }

    public string ComputeHash(Guid challengeId, string code)
    {
        var secretBytes = Encoding.UTF8.GetBytes(_options.VerificationCodeKey);
        using var hmac = new HMACSHA256(secretBytes);
        var payload = Encoding.UTF8.GetBytes($"{_options.VerificationCodeKeyVersion}:{challengeId:N}:{code}");
        return Convert.ToHexString(hmac.ComputeHash(payload));
    }

    public bool Verify(Guid challengeId, string providedCode, string storedHash)
        => string.Equals(ComputeHash(challengeId, providedCode), storedHash, StringComparison.Ordinal);

    public string CurrentKeyVersion => _options.VerificationCodeKeyVersion;
}
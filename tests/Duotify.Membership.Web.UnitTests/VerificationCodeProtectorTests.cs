using Duotify.Membership.Web.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Duotify.Membership.Web.UnitTests;

public sealed class VerificationCodeProtectorTests
{
    private static readonly VerificationCodeOptions TestOptions = new()
    {
        VerificationCodeKey = "test-secret-key-1234567890",
        VerificationCodeKeyVersion = "v1"
    };

    [Fact]
    public void Verify_ReturnsTrue_ForMatchingCodeAndChallenge()
    {
        var protector = new VerificationCodeProtector(Microsoft.Extensions.Options.Options.Create(TestOptions));
        var challengeId = Guid.NewGuid();
        var generated = protector.GenerateCode(challengeId);

        var verified = protector.Verify(challengeId, generated.Code, generated.CodeHash);

        Assert.True(verified);
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenChallengeIsDifferent()
    {
        var protector = new VerificationCodeProtector(Microsoft.Extensions.Options.Options.Create(TestOptions));
        var challengeId = Guid.NewGuid();
        var generated = protector.GenerateCode(challengeId);

        var verified = protector.Verify(Guid.NewGuid(), generated.Code, generated.CodeHash);

        Assert.False(verified);
    }
}
namespace Duotify.Membership.Web.Infrastructure.Security;

public sealed class VerificationCodeOptions
{
    public string VerificationCodeKey { get; init; } = string.Empty;
    public string VerificationCodeKeyVersion { get; init; } = "v1";
}
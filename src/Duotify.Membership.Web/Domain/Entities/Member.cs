using Duotify.Membership.Web.Domain.ValueObjects;

namespace Duotify.Membership.Web.Domain.Entities;

public sealed class Member
{
    public Guid MemberId { get; init; }
    public string RegistrationReference { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public EmailVerificationStatus EmailVerificationStatus { get; init; }
    public CapabilityStatus CapabilityStatus { get; init; }
    public DateTimeOffset? EmailVerifiedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public bool IsVerified => EmailVerificationStatus == EmailVerificationStatus.Verified;
}
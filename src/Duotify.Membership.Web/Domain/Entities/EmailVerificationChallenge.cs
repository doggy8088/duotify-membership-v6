using Duotify.Membership.Web.Domain.ValueObjects;

namespace Duotify.Membership.Web.Domain.Entities;

public sealed class EmailVerificationChallenge
{
    public Guid ChallengeId { get; init; }
    public Guid MemberId { get; init; }
    public string CodeHash { get; init; } = string.Empty;
    public string KeyVersion { get; init; } = string.Empty;
    public DateTimeOffset SentAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public int FailedAttemptCount { get; init; }
    public ChallengeStatus Status { get; init; }
    public DateTimeOffset? InvalidatedAt { get; init; }
    public DateTimeOffset? VerifiedAt { get; init; }
    public DateTimeOffset? LockedAt { get; init; }
    public Guid? ReplacedByChallengeId { get; init; }
}
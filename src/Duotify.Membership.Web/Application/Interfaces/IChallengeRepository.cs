using System.Data.Common;
using Duotify.Membership.Web.Domain.Entities;

namespace Duotify.Membership.Web.Application.Interfaces;

public interface IChallengeRepository
{
    Task<EmailVerificationChallenge?> GetCurrentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);
    Task<EmailVerificationChallenge?> GetCurrentByRegistrationReferenceAsync(string registrationReference, CancellationToken cancellationToken);
    Task CreateAsync(EmailVerificationChallenge challenge, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
    Task MarkVerifiedAsync(Guid challengeId, DateTimeOffset verifiedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
    Task MarkExpiredIfNeededAsync(Guid challengeId, DateTimeOffset expiredAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
    Task MarkLockedAsync(Guid challengeId, int failedAttemptCount, DateTimeOffset lockedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
    Task IncrementFailedAttemptAsync(Guid challengeId, int failedAttemptCount, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
    Task InvalidateAsync(Guid challengeId, Guid replacedByChallengeId, DateTimeOffset invalidatedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
}
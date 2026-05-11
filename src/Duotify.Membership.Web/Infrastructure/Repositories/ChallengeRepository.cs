using System.Data.Common;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;
using Duotify.Membership.Web.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Duotify.Membership.Web.Infrastructure.Repositories;

public sealed class ChallengeRepository(SqlConnectionFactory connectionFactory, IMemberRepository memberRepository) : IChallengeRepository
{
    public async Task<EmailVerificationChallenge?> GetCurrentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
        => await QuerySingleAsync(@"SELECT TOP 1 * FROM dbo.EmailVerificationChallenges WHERE MemberId = @MemberId AND Status IN (0, 1) ORDER BY SentAt DESC", new SqlParameter("@MemberId", memberId), cancellationToken);

    public async Task<EmailVerificationChallenge?> GetCurrentByRegistrationReferenceAsync(string registrationReference, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByRegistrationReferenceAsync(registrationReference, cancellationToken);
        return member is null ? null : await GetCurrentByMemberIdAsync(member.MemberId, cancellationToken);
    }

    public async Task CreateAsync(EmailVerificationChallenge challenge, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
INSERT INTO dbo.EmailVerificationChallenges
(ChallengeId, MemberId, CodeHash, KeyVersion, SentAt, ExpiresAt, FailedAttemptCount, Status, InvalidatedAt, VerifiedAt, LockedAt, ReplacedByChallengeId)
VALUES
(@ChallengeId, @MemberId, @CodeHash, @KeyVersion, @SentAt, @ExpiresAt, @FailedAttemptCount, @Status, @InvalidatedAt, @VerifiedAt, @LockedAt, @ReplacedByChallengeId);";
        command.Parameters.AddRange([
            new SqlParameter("@ChallengeId", challenge.ChallengeId),
            new SqlParameter("@MemberId", challenge.MemberId),
            new SqlParameter("@CodeHash", challenge.CodeHash),
            new SqlParameter("@KeyVersion", challenge.KeyVersion),
            new SqlParameter("@SentAt", challenge.SentAt),
            new SqlParameter("@ExpiresAt", challenge.ExpiresAt),
            new SqlParameter("@FailedAttemptCount", challenge.FailedAttemptCount),
            new SqlParameter("@Status", (int)challenge.Status),
            new SqlParameter("@InvalidatedAt", DBNull.Value),
            new SqlParameter("@VerifiedAt", DBNull.Value),
            new SqlParameter("@LockedAt", DBNull.Value),
            new SqlParameter("@ReplacedByChallengeId", DBNull.Value)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task MarkVerifiedAsync(Guid challengeId, DateTimeOffset verifiedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
        => ExecuteStatusCommandAsync(challengeId, ChallengeStatus.Verified, verifiedAt, "VerifiedAt", null, connection, transaction, cancellationToken);

    public Task MarkExpiredIfNeededAsync(Guid challengeId, DateTimeOffset expiredAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
        => ExecuteStatusCommandAsync(challengeId, ChallengeStatus.Expired, expiredAt, "InvalidatedAt", "AND Status IN (0, 1)", connection, transaction, cancellationToken);

    public async Task MarkLockedAsync(Guid challengeId, int failedAttemptCount, DateTimeOffset lockedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
UPDATE dbo.EmailVerificationChallenges
SET FailedAttemptCount = @FailedAttemptCount,
    Status = @Status,
    LockedAt = @LockedAt
WHERE ChallengeId = @ChallengeId;";
        command.Parameters.AddRange([
            new SqlParameter("@FailedAttemptCount", failedAttemptCount),
            new SqlParameter("@Status", (int)ChallengeStatus.Locked),
            new SqlParameter("@LockedAt", lockedAt),
            new SqlParameter("@ChallengeId", challengeId)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task IncrementFailedAttemptAsync(Guid challengeId, int failedAttemptCount, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
UPDATE dbo.EmailVerificationChallenges
SET FailedAttemptCount = @FailedAttemptCount
WHERE ChallengeId = @ChallengeId;";
        command.Parameters.AddRange([
            new SqlParameter("@FailedAttemptCount", failedAttemptCount),
            new SqlParameter("@ChallengeId", challengeId)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InvalidateAsync(Guid challengeId, Guid replacedByChallengeId, DateTimeOffset invalidatedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
UPDATE dbo.EmailVerificationChallenges
SET Status = @Status,
    InvalidatedAt = @InvalidatedAt,
    ReplacedByChallengeId = @ReplacedByChallengeId
WHERE ChallengeId = @ChallengeId;";
        command.Parameters.AddRange([
            new SqlParameter("@Status", (int)ChallengeStatus.Invalidated),
            new SqlParameter("@InvalidatedAt", invalidatedAt),
            new SqlParameter("@ReplacedByChallengeId", replacedByChallengeId),
            new SqlParameter("@ChallengeId", challengeId)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<EmailVerificationChallenge?> QuerySingleAsync(string sql, SqlParameter parameter, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(parameter);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return SqlMappingHelpers.MapChallenge(reader);
    }

    private static async Task ExecuteStatusCommandAsync(Guid challengeId, ChallengeStatus status, DateTimeOffset timestamp, string timestampColumn, string? extraPredicate, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = $@"
UPDATE dbo.EmailVerificationChallenges
SET Status = @Status,
    {timestampColumn} = @Timestamp
WHERE ChallengeId = @ChallengeId {extraPredicate};";
        command.Parameters.AddRange([
            new SqlParameter("@Status", (int)status),
            new SqlParameter("@Timestamp", timestamp),
            new SqlParameter("@ChallengeId", challengeId)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
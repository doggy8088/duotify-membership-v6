using System.Text.Json;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;
using Duotify.Membership.Web.Infrastructure.Data;
using Duotify.Membership.Web.Infrastructure.Security;

namespace Duotify.Membership.Web.Application.Services;

public sealed class VerifyEmailCodeService(
    IMemberRepository memberRepository,
    IChallengeRepository challengeRepository,
    ISecurityAuditLogRepository auditLogRepository,
    SqlConnectionFactory connectionFactory,
    VerificationCodeProtector verificationCodeProtector)
{
    public async Task<ServiceResult> VerifyAsync(string registrationReference, string verificationCode, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByRegistrationReferenceAsync(registrationReference, cancellationToken);
        if (member is null)
        {
            return ServiceResult.Failure("registration_not_found", "找不到對應的註冊流程。");
        }

        var challenge = await challengeRepository.GetCurrentByMemberIdAsync(member.MemberId, cancellationToken);
        if (challenge is null)
        {
            return ServiceResult.Failure("challenge_not_found", "目前沒有可用的驗證碼，請重新寄送。");
        }

        var now = DateTimeOffset.UtcNow;
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            if (challenge.Status == ChallengeStatus.Locked)
            {
                await auditLogRepository.CreateAsync(CreateAudit("VerificationLocked", member.MemberId, challenge.ChallengeId, now, ipAddress, userAgent, null), connection, transaction, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ServiceResult.Failure("challenge_locked", "驗證流程已鎖定，請重新寄送新的驗證碼。");
            }

            if (challenge.ExpiresAt < now)
            {
                await challengeRepository.MarkExpiredIfNeededAsync(challenge.ChallengeId, now, connection, transaction, cancellationToken);
                await auditLogRepository.CreateAsync(CreateAudit("VerificationExpired", member.MemberId, challenge.ChallengeId, now, ipAddress, userAgent, null), connection, transaction, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ServiceResult.Failure("challenge_expired", "驗證碼已失效，請重新寄送新的驗證碼。");
            }

            if (!verificationCodeProtector.Verify(challenge.ChallengeId, verificationCode, challenge.CodeHash))
            {
                var failedAttemptCount = challenge.FailedAttemptCount + 1;
                if (failedAttemptCount >= 3)
                {
                    await challengeRepository.MarkLockedAsync(challenge.ChallengeId, failedAttemptCount, now, connection, transaction, cancellationToken);
                    await auditLogRepository.CreateAsync(CreateAudit("VerificationLocked", member.MemberId, challenge.ChallengeId, now, ipAddress, userAgent, new { failedAttemptCount }), connection, transaction, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return ServiceResult.Failure("challenge_locked", "驗證碼輸入錯誤次數過多，請重新寄送新的驗證碼。");
                }

                await challengeRepository.IncrementFailedAttemptAsync(challenge.ChallengeId, failedAttemptCount, connection, transaction, cancellationToken);
                await auditLogRepository.CreateAsync(CreateAudit("VerificationFailed", member.MemberId, challenge.ChallengeId, now, ipAddress, userAgent, new { failedAttemptCount }), connection, transaction, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ServiceResult.Failure("invalid_code", "驗證碼不正確。");
            }

            await challengeRepository.MarkVerifiedAsync(challenge.ChallengeId, now, connection, transaction, cancellationToken);
            await memberRepository.MarkVerifiedAsync(member.MemberId, now, connection, transaction, cancellationToken);
            await auditLogRepository.CreateAsync(CreateAudit("VerificationSucceeded", member.MemberId, challenge.ChallengeId, now, ipAddress, userAgent, null), connection, transaction, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResult.Success();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static SecurityAuditLog CreateAudit(string eventType, Guid memberId, Guid challengeId, DateTimeOffset occurredAt, string? ipAddress, string? userAgent, object? metadata)
        => new()
        {
            AuditLogId = Guid.NewGuid(),
            MemberId = memberId,
            ChallengeId = challengeId,
            EventType = eventType,
            OccurredAt = occurredAt,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata)
        };
}
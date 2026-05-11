using System.Text.Json;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;
using Duotify.Membership.Web.Infrastructure.Data;
using Duotify.Membership.Web.Infrastructure.Security;

namespace Duotify.Membership.Web.Application.Services;

public sealed class ResendEmailVerificationService(
    IMemberRepository memberRepository,
    IChallengeRepository challengeRepository,
    ISecurityAuditLogRepository auditLogRepository,
    IEmailSender emailSender,
    SqlConnectionFactory connectionFactory,
    VerificationCodeProtector verificationCodeProtector)
{
    public async Task<ServiceResult> ResendAsync(string registrationReference, Func<string, string> verificationLinkFactory, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByRegistrationReferenceAsync(registrationReference, cancellationToken);
        if (member is null)
        {
            return ServiceResult.Failure("registration_not_found", "找不到對應的註冊流程。");
        }

        if (member.IsVerified)
        {
            return ServiceResult.Failure("already_verified", "帳號已完成驗證，請直接登入。");
        }

        var currentChallenge = await challengeRepository.GetCurrentByMemberIdAsync(member.MemberId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var nextChallengeId = Guid.NewGuid();
        var verificationCode = verificationCodeProtector.GenerateCode(nextChallengeId);
        var nextChallenge = new EmailVerificationChallenge
        {
            ChallengeId = nextChallengeId,
            MemberId = member.MemberId,
            CodeHash = verificationCode.CodeHash,
            KeyVersion = verificationCodeProtector.CurrentKeyVersion,
            SentAt = now,
            ExpiresAt = now.AddMinutes(5),
            FailedAttemptCount = 0,
            Status = ChallengeStatus.Active
        };

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            if (currentChallenge is not null)
            {
                await challengeRepository.InvalidateAsync(currentChallenge.ChallengeId, nextChallengeId, now, connection, transaction, cancellationToken);
            }

            await challengeRepository.CreateAsync(nextChallenge, connection, transaction, cancellationToken);
            await auditLogRepository.CreateAsync(new SecurityAuditLog
            {
                AuditLogId = Guid.NewGuid(),
                MemberId = member.MemberId,
                ChallengeId = nextChallenge.ChallengeId,
                EventType = "VerificationResent",
                OccurredAt = now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                MetadataJson = JsonSerializer.Serialize(new { registrationReference, previousChallengeId = currentChallenge?.ChallengeId })
            }, connection, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await emailSender.SendVerificationCodeAsync(
            registrationReference,
            member.Email,
            member.FullName,
            verificationCode.Code,
            verificationLinkFactory(registrationReference),
            cancellationToken);

        return ServiceResult.Success();
    }
}
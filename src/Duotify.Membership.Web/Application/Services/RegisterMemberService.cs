using System.Text.Json;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;
using Duotify.Membership.Web.Infrastructure.Data;
using Duotify.Membership.Web.Infrastructure.Security;

namespace Duotify.Membership.Web.Application.Services;

public sealed class RegisterMemberService(
    IMemberRepository memberRepository,
    IChallengeRepository challengeRepository,
    ISecurityAuditLogRepository auditLogRepository,
    IEmailSender emailSender,
    SqlConnectionFactory connectionFactory,
    PasswordHasherAdapter passwordHasher,
    VerificationCodeProtector verificationCodeProtector)
{
    public async Task<ServiceResult<RegisterMemberResult>> RegisterAsync(string nationalId, string fullName, string email, string password, Func<string, string> verificationLinkFactory, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        if (!PasswordRules.IsValid(password))
        {
            return ServiceResult<RegisterMemberResult>.Failure("invalid_password", "密碼必須為 8 到 20 碼，且包含英文大小寫與數字。");
        }

        if (await memberRepository.GetByNationalIdAsync(nationalId, cancellationToken) is not null)
        {
            return ServiceResult<RegisterMemberResult>.Failure("duplicate_national_id", "此身分證字號已完成註冊。");
        }

        if (await memberRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return ServiceResult<RegisterMemberResult>.Failure("duplicate_email", "此 E-Mail 已綁定其他帳號。");
        }

        var now = DateTimeOffset.UtcNow;
        var memberId = Guid.NewGuid();
        var registrationReference = Guid.NewGuid().ToString("N");
        var challengeId = Guid.NewGuid();
        var verificationCode = verificationCodeProtector.GenerateCode(challengeId);

        var member = new Member
        {
            MemberId = memberId,
            RegistrationReference = registrationReference,
            NationalId = nationalId.Trim(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(password),
            EmailVerificationStatus = EmailVerificationStatus.Pending,
            CapabilityStatus = CapabilityStatus.Restricted,
            CreatedAt = now,
            UpdatedAt = now
        };

        var challenge = new EmailVerificationChallenge
        {
            ChallengeId = challengeId,
            MemberId = memberId,
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
            await memberRepository.CreateAsync(member, connection, transaction, cancellationToken);
            await challengeRepository.CreateAsync(challenge, connection, transaction, cancellationToken);
            await auditLogRepository.CreateAsync(new SecurityAuditLog
            {
                AuditLogId = Guid.NewGuid(),
                MemberId = memberId,
                ChallengeId = challengeId,
                EventType = "RegistrationCreated",
                OccurredAt = now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                MetadataJson = JsonSerializer.Serialize(new { registrationReference })
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

        return ServiceResult<RegisterMemberResult>.Success(new RegisterMemberResult
        {
            RegistrationReference = registrationReference,
            Email = member.Email
        });
    }
}
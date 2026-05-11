using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Application.Services;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Infrastructure.Data;
using Duotify.Membership.Web.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Duotify.Membership.Web.UnitTests;

public sealed class RegisterMemberServiceTests
{
    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenPasswordDoesNotMeetPolicy()
    {
        var service = CreateService();

        var result = await service.RegisterAsync("A123456789", "王小明", "member@example.com", "weakpass", null, null, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("invalid_password", result.ErrorCode);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenEmailAlreadyExists()
    {
        var memberRepository = new FakeMemberRepository
        {
            ExistingEmailMember = new Member { MemberId = Guid.NewGuid(), Email = "member@example.com" }
        };
        var service = CreateService(memberRepository: memberRepository);

        var result = await service.RegisterAsync("A123456789", "王小明", "member@example.com", "Password123", null, null, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("duplicate_email", result.ErrorCode);
    }

    private static RegisterMemberService CreateService(FakeMemberRepository? memberRepository = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=master;Integrated Security=true;TrustServerCertificate=true"
            })
            .Build();

        var protector = new VerificationCodeProtector(Microsoft.Extensions.Options.Options.Create(new VerificationCodeOptions
        {
            VerificationCodeKey = "unit-test-secret-key-1234567890",
            VerificationCodeKeyVersion = "v1"
        }));

        return new RegisterMemberService(
            memberRepository ?? new FakeMemberRepository(),
            new FakeChallengeRepository(),
            new FakeAuditRepository(),
            new FakeEmailSender(),
            new SqlConnectionFactory(configuration),
            new PasswordHasherAdapter(),
            protector);
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public Member? ExistingEmailMember { get; init; }
        public Member? ExistingNationalIdMember { get; init; }

        public Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult(ExistingEmailMember);
        public Task<Member?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) => Task.FromResult(ExistingNationalIdMember);
        public Task<Member?> GetByRegistrationReferenceAsync(string registrationReference, CancellationToken cancellationToken) => Task.FromResult<Member?>(null);
        public Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken) => Task.FromResult<Member?>(null);
        public Task CreateAsync(Member member, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task MarkVerifiedAsync(Guid memberId, DateTimeOffset verifiedAt, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeChallengeRepository : IChallengeRepository
    {
        public Task<EmailVerificationChallenge?> GetCurrentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken) => Task.FromResult<EmailVerificationChallenge?>(null);
        public Task<EmailVerificationChallenge?> GetCurrentByRegistrationReferenceAsync(string registrationReference, CancellationToken cancellationToken) => Task.FromResult<EmailVerificationChallenge?>(null);
        public Task CreateAsync(EmailVerificationChallenge challenge, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task MarkVerifiedAsync(Guid challengeId, DateTimeOffset verifiedAt, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task MarkExpiredIfNeededAsync(Guid challengeId, DateTimeOffset expiredAt, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task MarkLockedAsync(Guid challengeId, int failedAttemptCount, DateTimeOffset lockedAt, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task IncrementFailedAttemptAsync(Guid challengeId, int failedAttemptCount, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task InvalidateAsync(Guid challengeId, Guid replacedByChallengeId, DateTimeOffset invalidatedAt, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeAuditRepository : ISecurityAuditLogRepository
    {
        public Task CreateAsync(SecurityAuditLog auditLog, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public Task SendVerificationCodeAsync(string recipientEmail, string recipientName, string verificationCode, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
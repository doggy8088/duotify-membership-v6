using Duotify.Membership.Web.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Duotify.Membership.Web.UnitTests;

public sealed class SmtpEmailSenderTests
{
    [Fact]
    public async Task SendVerificationCodeAsync_DoesNotThrow_WhenSmtpIsNotConfigured()
    {
        var previewStore = new VerificationEmailPreviewStore();
        var sender = new SmtpEmailSender(
            Options.Create(new EmailOptions
            {
                Host = string.Empty,
                Port = 0,
                SenderAddress = string.Empty,
                SenderName = "Duotify Membership"
            }),
            previewStore,
            NullLogger<SmtpEmailSender>.Instance);

        await sender.SendVerificationCodeAsync(
            "ref-001",
            "member@example.com",
            "王小明",
            "123456",
            "https://example.test/register/verify?registrationRef=ref-001",
            CancellationToken.None);

        var preview = previewStore.Get("ref-001");

        Assert.NotNull(preview);
        Assert.Equal("123456", preview!.VerificationCode);
        Assert.Equal("https://example.test/register/verify?registrationRef=ref-001", preview.VerificationLink);
    }
}
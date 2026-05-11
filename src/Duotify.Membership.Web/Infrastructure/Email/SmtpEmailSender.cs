using System.Net.Mail;
using Duotify.Membership.Web.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Duotify.Membership.Web.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendVerificationCodeAsync(string recipientEmail, string recipientName, string verificationCode, CancellationToken cancellationToken)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_options.SenderAddress, _options.SenderName),
            Subject = "Duotify 驗證碼",
            Body = $"Hi {recipientName},\n\n您的驗證碼是 {verificationCode}。此驗證碼 5 分鐘內有效。\n\n若非本人操作，請忽略這封信。"
        };
        message.To.Add(recipientEmail);

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port);
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to send verification email to {RecipientEmail}", recipientEmail);
            throw;
        }
    }
}
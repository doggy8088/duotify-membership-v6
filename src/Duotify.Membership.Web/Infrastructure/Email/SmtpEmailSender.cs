using System.Net.Mail;
using Duotify.Membership.Web.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Duotify.Membership.Web.Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    VerificationEmailPreviewStore previewStore,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendVerificationCodeAsync(
        string registrationReference,
        string recipientEmail,
        string recipientName,
        string verificationCode,
        string verificationLink,
        CancellationToken cancellationToken)
    {
        if (!HasRequiredConfiguration())
        {
            logger.LogWarning(
                "SMTP is not configured. Verification email for {RegistrationReference} to {RecipientEmail} was not sent. Test preview will be exposed in non-Release builds.",
                registrationReference,
                recipientEmail);

            SavePreview(registrationReference, recipientEmail, verificationCode, verificationLink, "目前尚未設定 SMTP，系統未實際寄出驗證信。請使用下方測試資料完成流程。");
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.SenderAddress, _options.SenderName),
                Subject = "Duotify 驗證碼",
                Body = $"Hi {recipientName},\n\n您的驗證碼是 {verificationCode}。此驗證碼 5 分鐘內有效。\n\n驗證頁面：{verificationLink}\n\n若非本人操作，請忽略這封信。"
            };
            message.To.Add(recipientEmail);

            using var client = new SmtpClient(_options.Host, _options.Port);
            await client.SendMailAsync(message, cancellationToken);
            previewStore.Clear(registrationReference);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to send verification email for {RegistrationReference} to {RecipientEmail}. Test preview will be exposed in non-Release builds.",
                registrationReference,
                recipientEmail);

            SavePreview(registrationReference, recipientEmail, verificationCode, verificationLink, "SMTP 傳送失敗，系統未實際寄出驗證信。請使用下方測試資料完成流程。");
        }
    }

    private bool HasRequiredConfiguration()
        => !string.IsNullOrWhiteSpace(_options.Host)
            && _options.Port > 0
            && !string.IsNullOrWhiteSpace(_options.SenderAddress);

    private void SavePreview(string registrationReference, string recipientEmail, string verificationCode, string verificationLink, string notice)
    {
        previewStore.Save(registrationReference, new VerificationEmailPreview
        {
            RecipientEmail = recipientEmail,
            VerificationCode = verificationCode,
            VerificationLink = verificationLink,
            Notice = notice,
            CapturedAt = DateTimeOffset.UtcNow
        });
    }
}
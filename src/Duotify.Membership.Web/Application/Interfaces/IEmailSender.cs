namespace Duotify.Membership.Web.Application.Interfaces;

public interface IEmailSender
{
    Task SendVerificationCodeAsync(string recipientEmail, string recipientName, string verificationCode, CancellationToken cancellationToken);
}
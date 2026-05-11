namespace Duotify.Membership.Web.Application.Interfaces;

public interface IEmailSender
{
    Task SendVerificationCodeAsync(
        string registrationReference,
        string recipientEmail,
        string recipientName,
        string verificationCode,
        string verificationLink,
        CancellationToken cancellationToken);
}
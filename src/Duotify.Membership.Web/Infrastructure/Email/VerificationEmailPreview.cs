namespace Duotify.Membership.Web.Infrastructure.Email;

public sealed class VerificationEmailPreview
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public string VerificationLink { get; init; } = string.Empty;
    public string Notice { get; init; } = string.Empty;
    public DateTimeOffset CapturedAt { get; init; }
}
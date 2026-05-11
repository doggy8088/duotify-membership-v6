namespace Duotify.Membership.Web.Application.Services;

public sealed class RegisterMemberResult
{
    public string RegistrationReference { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
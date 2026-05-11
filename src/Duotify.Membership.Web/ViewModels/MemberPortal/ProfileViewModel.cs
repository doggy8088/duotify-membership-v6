namespace Duotify.Membership.Web.ViewModels.MemberPortal;

public sealed class ProfileViewModel
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string NationalIdMasked { get; init; } = string.Empty;
    public string RegistrationReference { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public bool CanPerformInteractiveAction { get; init; }
    public string? RestrictionMessage { get; init; }
    public string? ActionMessage { get; init; }
}
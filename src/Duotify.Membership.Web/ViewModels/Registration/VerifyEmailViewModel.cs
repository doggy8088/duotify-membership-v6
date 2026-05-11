using System.ComponentModel.DataAnnotations;
using Duotify.Membership.Web.Infrastructure.Email;

namespace Duotify.Membership.Web.ViewModels.Registration;

public sealed class VerifyEmailViewModel
{
    [Required]
    public string RegistrationReference { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入 6 位數驗證碼")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "驗證碼必須為 6 位數")]
    [Display(Name = "驗證碼")]
    public string VerificationCode { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public bool VerificationSucceeded { get; set; }
    public bool IsLocked { get; set; }
    public string? TestEmailNotice { get; set; }
    public string? TestVerificationCode { get; set; }
    public string? TestVerificationLink { get; set; }

    public VerifyEmailViewModel ApplyPreview(VerificationEmailPreview? preview)
    {
        TestEmailNotice = preview?.Notice;
        TestVerificationCode = preview?.VerificationCode;
        TestVerificationLink = preview?.VerificationLink;
        return this;
    }
}
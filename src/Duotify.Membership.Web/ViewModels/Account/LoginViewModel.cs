using System.ComponentModel.DataAnnotations;

namespace Duotify.Membership.Web.ViewModels.Account;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "請輸入 E-Mail")]
    [EmailAddress(ErrorMessage = "請輸入有效的 E-Mail")]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
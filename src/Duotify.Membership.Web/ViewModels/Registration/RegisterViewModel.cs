using System.ComponentModel.DataAnnotations;

namespace Duotify.Membership.Web.ViewModels.Registration;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "請輸入身分證字號")]
    [Display(Name = "身分證字號")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入姓名")]
    [Display(Name = "姓名")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入 E-Mail")]
    [EmailAddress(ErrorMessage = "請輸入有效的 E-Mail")]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;
}
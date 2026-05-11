using System.Text.RegularExpressions;

namespace Duotify.Membership.Web.Application.Services;

public static partial class PasswordRules
{
    public static bool IsValid(string password)
        => !string.IsNullOrWhiteSpace(password)
        && password.Length is >= 8 and <= 20
        && UpperCase().IsMatch(password)
        && LowerCase().IsMatch(password)
        && Digit().IsMatch(password);

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCase();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowerCase();

    [GeneratedRegex("[0-9]")]
    private static partial Regex Digit();
}
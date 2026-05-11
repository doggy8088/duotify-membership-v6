using Duotify.Membership.Web.Application.Services;

namespace Duotify.Membership.Web.UnitTests;

public sealed class PasswordRulesTests
{
    [Theory]
    [InlineData("Password123")]
    [InlineData("Qwerty2024")]
    public void IsValid_ReturnsTrue_ForPasswordMatchingPolicy(string password)
    {
        Assert.True(PasswordRules.IsValid(password));
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase123")]
    [InlineData("ALLUPPERCASE123")]
    [InlineData("NoDigitsHere")]
    [InlineData("1234567890")]
    public void IsValid_ReturnsFalse_ForPasswordViolatingPolicy(string password)
    {
        Assert.False(PasswordRules.IsValid(password));
    }
}
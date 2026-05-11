using Microsoft.AspNetCore.Mvc.Testing;

namespace Duotify.Membership.Web.WebTests;

public sealed class RegistrationPagesTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task GetRegister_ReturnsPageContent()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/register");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("建立你的會員帳號", content);
    }

    [Fact]
    public async Task GetLogin_ReturnsPageContent()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/account/login");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("登入你的帳號", content);
    }

    [Fact]
    public async Task MemberProfile_RedirectsAnonymousUserToLogin()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/member/profile");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith("http://localhost/account/login", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }
}
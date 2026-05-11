namespace Duotify.Membership.Web.WebTests;

public sealed class RestrictedAccessTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task MemberProfile_RedirectsAnonymousUserToLogin()
    {
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/member/profile");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
    }
}
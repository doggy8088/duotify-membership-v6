namespace Duotify.Membership.Web.WebTests;

public sealed class VerifyRouteContractTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task PostVerify_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["RegistrationReference"] = "ref-001",
            ["VerificationCode"] = "123456"
        });

        var response = await client.PostAsync("/register/verify", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostResend_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["RegistrationReference"] = "ref-001"
        });

        var response = await client.PostAsync("/register/verify/resend", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
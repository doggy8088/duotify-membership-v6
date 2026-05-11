namespace Duotify.Membership.Web.WebTests;

public sealed class RegisterRouteContractTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task GetRegister_ReturnsOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/register");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostRegister_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["NationalId"] = "A123456789",
            ["FullName"] = "王小明",
            ["Email"] = "member@example.com",
            ["Password"] = "Password123"
        });

        var response = await client.PostAsync("/register", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
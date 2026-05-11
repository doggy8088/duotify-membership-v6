using System.Security.Claims;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Application.Services;

namespace Duotify.Membership.Web.UnitTests;

public sealed class RestrictedAccessServiceTests
{
    [Fact]
    public void CanPerformInteractiveAction_ReturnsFalse_ForUnverifiedMember()
    {
        var service = new RestrictedAccessService(new FakeCurrentMemberAccessor(isVerified: false));
        var user = CreateAuthenticatedUser();

        var allowed = service.CanPerformInteractiveAction(user);

        Assert.False(allowed);
    }

    [Fact]
    public void CanPerformInteractiveAction_ReturnsTrue_ForVerifiedMember()
    {
        var service = new RestrictedAccessService(new FakeCurrentMemberAccessor(isVerified: true));
        var user = CreateAuthenticatedUser();

        var allowed = service.CanPerformInteractiveAction(user);

        Assert.True(allowed);
    }

    private static ClaimsPrincipal CreateAuthenticatedUser()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "Test"));

    private sealed class FakeCurrentMemberAccessor(bool isVerified) : ICurrentMemberAccessor
    {
        public Guid? GetMemberId(ClaimsPrincipal user) => Guid.NewGuid();
        public bool IsVerified(ClaimsPrincipal user) => isVerified;
        public string? GetEmail(ClaimsPrincipal user) => "member@example.com";
    }
}
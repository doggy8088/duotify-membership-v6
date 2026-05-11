using System.Security.Claims;
using Duotify.Membership.Web.Application.Interfaces;

namespace Duotify.Membership.Web.Application.Services;

public sealed class RestrictedAccessService(ICurrentMemberAccessor currentMemberAccessor)
{
    public bool CanViewBasicProfile(ClaimsPrincipal user) => user.Identity?.IsAuthenticated ?? false;

    public bool CanPerformInteractiveAction(ClaimsPrincipal user) => currentMemberAccessor.IsVerified(user);
}
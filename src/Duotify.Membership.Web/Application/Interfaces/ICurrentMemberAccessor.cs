using System.Security.Claims;

namespace Duotify.Membership.Web.Application.Interfaces;

public interface ICurrentMemberAccessor
{
    Guid? GetMemberId(ClaimsPrincipal user);
    bool IsVerified(ClaimsPrincipal user);
    string? GetEmail(ClaimsPrincipal user);
}
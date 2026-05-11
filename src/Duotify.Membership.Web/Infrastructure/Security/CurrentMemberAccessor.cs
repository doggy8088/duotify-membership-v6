using System.Security.Claims;
using Duotify.Membership.Web.Application.Interfaces;

namespace Duotify.Membership.Web.Infrastructure.Security;

public sealed class CurrentMemberAccessor : ICurrentMemberAccessor
{
    public Guid? GetMemberId(ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var memberId) ? memberId : null;

    public bool IsVerified(ClaimsPrincipal user)
        => bool.TryParse(user.FindFirstValue("is_verified"), out var isVerified) && isVerified;

    public string? GetEmail(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email);
}
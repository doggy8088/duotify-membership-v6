using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Application.Services;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Infrastructure.Security;
using Duotify.Membership.Web.ViewModels.MemberPortal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Duotify.Membership.Web.Controllers;

[Authorize]
public sealed class MemberPortalController(
    ICurrentMemberAccessor currentMemberAccessor,
    IMemberRepository memberRepository,
    RestrictedAccessService restrictedAccessService) : Controller
{
    [HttpGet("/member/profile")]
    public async Task<IActionResult> Profile([FromQuery] string? message = null, CancellationToken cancellationToken = default)
    {
        var memberId = currentMemberAccessor.GetMemberId(User);
        if (memberId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var member = await memberRepository.GetByIdAsync(memberId.Value, cancellationToken);
        if (member is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(ToProfileViewModel(member, message));
    }

    [HttpPost("/member/demo-action")]
    [ServiceFilter(typeof(RequireVerifiedMemberFilter))]
    [ValidateAntiForgeryToken]
    public IActionResult DemoAction()
    {
        return RedirectToAction(nameof(Profile), new { message = "互動功能執行成功。" });
    }

    private ProfileViewModel ToProfileViewModel(Member member, string? actionMessage)
        => new()
        {
            FullName = member.FullName,
            Email = member.Email,
            NationalIdMasked = member.NationalId.Length > 4 ? $"{member.NationalId[..2]}******{member.NationalId[^2..]}" : member.NationalId,
            RegistrationReference = member.RegistrationReference,
            IsVerified = member.IsVerified,
            CanPerformInteractiveAction = restrictedAccessService.CanPerformInteractiveAction(User),
            RestrictionMessage = member.IsVerified ? null : "帳號尚未完成 E-Mail 驗證，目前僅可查看基本資料。",
            ActionMessage = actionMessage
        };
}
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Application.Services;
using Duotify.Membership.Web.Infrastructure.Security;
using Duotify.Membership.Web.ViewModels.Registration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Duotify.Membership.Web.Controllers;

public sealed class RegistrationController(
    RegisterMemberService registerMemberService,
    VerifyEmailCodeService verifyEmailCodeService,
    ResendEmailVerificationService resendEmailVerificationService,
    IMemberRepository memberRepository) : Controller
{
    [HttpGet("/")]
    public IActionResult Root() => RedirectToAction(nameof(Register));

    [HttpGet("/register")]
    public async Task<IActionResult> Register([FromQuery] string? registrationRef, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(registrationRef))
        {
            var member = await memberRepository.GetByRegistrationReferenceAsync(registrationRef, cancellationToken);
            if (member is not null && !member.IsVerified)
            {
                return RedirectToAction(nameof(Verify), new { registrationRef });
            }
        }

        return View(new RegisterViewModel());
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.Register)]
    public async Task<IActionResult> Register(RegisterViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await registerMemberService.RegisterAsync(
            viewModel.NationalId,
            viewModel.FullName,
            viewModel.Email,
            viewModel.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "註冊失敗");
            return View(viewModel);
        }

        return RedirectToAction(nameof(Verify), new { registrationRef = result.Value.RegistrationReference, registered = true });
    }

    [HttpGet("/register/verify")]
    public async Task<IActionResult> Verify([FromQuery] string registrationRef, [FromQuery] bool registered = false, [FromQuery] string? message = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(registrationRef))
        {
            return RedirectToAction(nameof(Register));
        }

        var member = await memberRepository.GetByRegistrationReferenceAsync(registrationRef, cancellationToken);
        if (member is null)
        {
            return RedirectToAction(nameof(Register));
        }

        return View(new VerifyEmailViewModel
        {
            RegistrationReference = registrationRef,
            Email = member.Email,
            StatusMessage = registered ? "註冊完成，驗證碼已寄出。請手動輸入驗證碼完成 E-Mail 驗證。" : message
        });
    }

    [HttpPost("/register/verify")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.Verify)]
    public async Task<IActionResult> Verify(VerifyEmailViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await PopulateVerifyViewAsync(viewModel, cancellationToken);
        }

        var result = await verifyEmailCodeService.VerifyAsync(
            viewModel.RegistrationReference,
            viewModel.VerificationCode,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.Succeeded)
        {
            viewModel.StatusMessage = result.Message;
            viewModel.IsLocked = result.ErrorCode == "challenge_locked";
            return await PopulateVerifyViewAsync(viewModel, cancellationToken);
        }

        viewModel.StatusMessage = "E-Mail 驗證完成。請使用帳號密碼登入。";
        viewModel.VerificationSucceeded = true;
        viewModel.VerificationCode = string.Empty;
        return await PopulateVerifyViewAsync(viewModel, cancellationToken);
    }

    [HttpPost("/register/verify/resend")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.Resend)]
    public async Task<IActionResult> Resend(ResendVerificationViewModel viewModel, CancellationToken cancellationToken)
    {
        var result = await resendEmailVerificationService.ResendAsync(
            viewModel.RegistrationReference,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        var message = result.Succeeded ? "新的驗證碼已寄出，舊驗證碼已失效。" : result.Message;
        return RedirectToAction(nameof(Verify), new { registrationRef = viewModel.RegistrationReference, message });
    }

    private async Task<ViewResult> PopulateVerifyViewAsync(VerifyEmailViewModel viewModel, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByRegistrationReferenceAsync(viewModel.RegistrationReference, cancellationToken);
        viewModel.Email = member?.Email ?? string.Empty;
        return View(viewModel);
    }
}
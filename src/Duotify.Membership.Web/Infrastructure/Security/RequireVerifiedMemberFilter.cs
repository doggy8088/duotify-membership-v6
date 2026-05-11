using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Duotify.Membership.Web.Infrastructure.Security;

public sealed class RequireVerifiedMemberFilter(
    ICurrentMemberAccessor currentMemberAccessor,
    ISecurityAuditLogRepository auditLogRepository,
    SqlConnectionFactory connectionFactory,
    ILogger<RequireVerifiedMemberFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!currentMemberAccessor.IsVerified(context.HttpContext.User))
        {
            var memberId = currentMemberAccessor.GetMemberId(context.HttpContext.User);
            if (memberId is not null)
            {
                try
                {
                    await using var connection = connectionFactory.CreateConnection();
                    await connection.OpenAsync(context.HttpContext.RequestAborted);
                    await using var transaction = await connection.BeginTransactionAsync(context.HttpContext.RequestAborted);
                    await auditLogRepository.CreateAsync(new SecurityAuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        MemberId = memberId,
                        EventType = "RestrictedFeatureDenied",
                        OccurredAt = DateTimeOffset.UtcNow,
                        IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = context.HttpContext.Request.Headers.UserAgent.ToString()
                    }, connection, transaction, context.HttpContext.RequestAborted);
                    await transaction.CommitAsync(context.HttpContext.RequestAborted);
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Failed to write restricted access audit log for member {MemberId}", memberId);
                }
            }

            context.Result = new RedirectToActionResult("Profile", "MemberPortal", new { message = "請先完成 E-Mail 驗證，才能使用這項功能。" });
            return;
        }

        await next();
    }
}
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Duotify.Membership.Web.Infrastructure.Security;

public static class RateLimitPolicies
{
    public const string Register = "register";
    public const string Verify = "verify";
    public const string Resend = "resend";

    public static void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(Register, CreateFixedWindowPolicy(5, TimeSpan.FromMinutes(1)));
        options.AddPolicy(Verify, CreateFixedWindowPolicy(10, TimeSpan.FromMinutes(1)));
        options.AddPolicy(Resend, CreateFixedWindowPolicy(3, TimeSpan.FromMinutes(1)));
    }

    private static Func<HttpContext, RateLimitPartition<string>> CreateFixedWindowPolicy(int permitLimit, TimeSpan window)
        => context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true
            });
}
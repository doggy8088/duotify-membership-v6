using System.Security.Claims;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Application.Services;
using Duotify.Membership.Web.Infrastructure.Data;
using Duotify.Membership.Web.Infrastructure.Email;
using Duotify.Membership.Web.Infrastructure.Repositories;
using Duotify.Membership.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<VerificationCodeOptions>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/login";
        options.Cookie.Name = "duotify-membership";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddAntiforgery();
builder.Services.AddRateLimiter(RateLimitPolicies.Configure);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddSingleton<PasswordHasherAdapter>();
builder.Services.AddSingleton<VerificationCodeProtector>();
builder.Services.AddSingleton<ICurrentMemberAccessor, CurrentMemberAccessor>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<ISecurityAuditLogRepository, SecurityAuditLogRepository>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<RegisterMemberService>();
builder.Services.AddScoped<VerifyEmailCodeService>();
builder.Services.AddScoped<ResendEmailVerificationService>();
builder.Services.AddScoped<LoginMemberService>();
builder.Services.AddScoped<RestrictedAccessService>();
builder.Services.AddScoped<RequireVerifiedMemberFilter>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

await DatabaseInitializer.EnsureSchemaAsync(app.Services, app.Lifetime.ApplicationStopping);

app.UseMembershipSecurityHeaders();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Registration}/{action=Register}/{id?}");

app.Run();

public partial class Program;

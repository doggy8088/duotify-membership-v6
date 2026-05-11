using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Infrastructure.Security;

namespace Duotify.Membership.Web.Application.Services;

public sealed class LoginMemberService(IMemberRepository memberRepository, PasswordHasherAdapter passwordHasher)
{
    public async Task<ServiceResult<Member>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (member is null || !passwordHasher.Verify(member.PasswordHash, password))
        {
            return ServiceResult<Member>.Failure("invalid_credentials", "帳號或密碼不正確。");
        }

        return ServiceResult<Member>.Success(member);
    }
}
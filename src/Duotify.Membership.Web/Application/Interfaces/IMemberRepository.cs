using System.Data.Common;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;

namespace Duotify.Membership.Web.Application.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Member?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
    Task<Member?> GetByRegistrationReferenceAsync(string registrationReference, CancellationToken cancellationToken);
    Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken);
    Task CreateAsync(Member member, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
    Task MarkVerifiedAsync(Guid memberId, DateTimeOffset verifiedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
}
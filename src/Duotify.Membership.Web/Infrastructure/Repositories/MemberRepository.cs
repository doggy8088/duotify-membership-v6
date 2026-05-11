using System.Data.Common;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;
using Duotify.Membership.Web.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Duotify.Membership.Web.Infrastructure.Repositories;

public sealed class MemberRepository(SqlConnectionFactory connectionFactory) : IMemberRepository
{
    public async Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => await QuerySingleAsync("SELECT TOP 1 * FROM dbo.Members WHERE Email = @Email", new SqlParameter("@Email", email), cancellationToken);

    public async Task<Member?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken)
        => await QuerySingleAsync("SELECT TOP 1 * FROM dbo.Members WHERE NationalId = @NationalId", new SqlParameter("@NationalId", nationalId), cancellationToken);

    public async Task<Member?> GetByRegistrationReferenceAsync(string registrationReference, CancellationToken cancellationToken)
        => await QuerySingleAsync("SELECT TOP 1 * FROM dbo.Members WHERE RegistrationReference = @RegistrationReference", new SqlParameter("@RegistrationReference", registrationReference), cancellationToken);

    public async Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken)
        => await QuerySingleAsync("SELECT TOP 1 * FROM dbo.Members WHERE MemberId = @MemberId", new SqlParameter("@MemberId", memberId), cancellationToken);

    public async Task CreateAsync(Member member, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
INSERT INTO dbo.Members
(MemberId, RegistrationReference, NationalId, FullName, Email, PasswordHash, EmailVerificationStatus, CapabilityStatus, EmailVerifiedAt, CreatedAt, UpdatedAt)
VALUES
(@MemberId, @RegistrationReference, @NationalId, @FullName, @Email, @PasswordHash, @EmailVerificationStatus, @CapabilityStatus, @EmailVerifiedAt, @CreatedAt, @UpdatedAt);";
        command.Parameters.AddRange([
            new SqlParameter("@MemberId", member.MemberId),
            new SqlParameter("@RegistrationReference", member.RegistrationReference),
            new SqlParameter("@NationalId", member.NationalId),
            new SqlParameter("@FullName", member.FullName),
            new SqlParameter("@Email", member.Email),
            new SqlParameter("@PasswordHash", member.PasswordHash),
            new SqlParameter("@EmailVerificationStatus", (int)member.EmailVerificationStatus),
            new SqlParameter("@CapabilityStatus", (int)member.CapabilityStatus),
            new SqlParameter("@EmailVerifiedAt", member.EmailVerifiedAt is null ? DBNull.Value : member.EmailVerifiedAt),
            new SqlParameter("@CreatedAt", member.CreatedAt),
            new SqlParameter("@UpdatedAt", member.UpdatedAt)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkVerifiedAsync(Guid memberId, DateTimeOffset verifiedAt, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
UPDATE dbo.Members
SET EmailVerificationStatus = @EmailVerificationStatus,
    CapabilityStatus = @CapabilityStatus,
    EmailVerifiedAt = @VerifiedAt,
    UpdatedAt = @VerifiedAt
WHERE MemberId = @MemberId;";
        command.Parameters.AddRange([
            new SqlParameter("@EmailVerificationStatus", (int)EmailVerificationStatus.Verified),
            new SqlParameter("@CapabilityStatus", (int)CapabilityStatus.FullAccess),
            new SqlParameter("@VerifiedAt", verifiedAt),
            new SqlParameter("@MemberId", memberId)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<Member?> QuerySingleAsync(string sql, SqlParameter parameter, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(parameter);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return SqlMappingHelpers.MapMember(reader);
    }
}
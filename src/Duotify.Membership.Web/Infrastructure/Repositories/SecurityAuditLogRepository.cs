using System.Data.Common;
using Duotify.Membership.Web.Application.Interfaces;
using Duotify.Membership.Web.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Duotify.Membership.Web.Infrastructure.Repositories;

public sealed class SecurityAuditLogRepository : ISecurityAuditLogRepository
{
    public async Task CreateAsync(SecurityAuditLog auditLog, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = (SqlCommand)connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction;
        command.CommandText = @"
INSERT INTO dbo.SecurityAuditLogs
(AuditLogId, MemberId, ChallengeId, EventType, OccurredAt, IpAddress, UserAgent, MetadataJson)
VALUES
(@AuditLogId, @MemberId, @ChallengeId, @EventType, @OccurredAt, @IpAddress, @UserAgent, @MetadataJson);";
        command.Parameters.AddRange([
            new SqlParameter("@AuditLogId", auditLog.AuditLogId),
            new SqlParameter("@MemberId", auditLog.MemberId is null ? DBNull.Value : auditLog.MemberId),
            new SqlParameter("@ChallengeId", auditLog.ChallengeId is null ? DBNull.Value : auditLog.ChallengeId),
            new SqlParameter("@EventType", auditLog.EventType),
            new SqlParameter("@OccurredAt", auditLog.OccurredAt),
            new SqlParameter("@IpAddress", (object?)auditLog.IpAddress ?? DBNull.Value),
            new SqlParameter("@UserAgent", (object?)auditLog.UserAgent ?? DBNull.Value),
            new SqlParameter("@MetadataJson", (object?)auditLog.MetadataJson ?? DBNull.Value)
        ]);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
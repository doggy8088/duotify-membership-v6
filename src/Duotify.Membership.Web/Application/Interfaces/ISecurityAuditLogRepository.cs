using System.Data.Common;
using Duotify.Membership.Web.Domain.Entities;

namespace Duotify.Membership.Web.Application.Interfaces;

public interface ISecurityAuditLogRepository
{
    Task CreateAsync(SecurityAuditLog auditLog, DbConnection connection, DbTransaction transaction, CancellationToken cancellationToken);
}
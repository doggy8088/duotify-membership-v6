namespace Duotify.Membership.Web.Domain.Entities;

public sealed class SecurityAuditLog
{
    public Guid AuditLogId { get; init; }
    public Guid? MemberId { get; init; }
    public Guid? ChallengeId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? MetadataJson { get; init; }
}
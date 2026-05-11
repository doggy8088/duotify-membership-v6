using System.Data.Common;
using Duotify.Membership.Web.Domain.Entities;
using Duotify.Membership.Web.Domain.ValueObjects;

namespace Duotify.Membership.Web.Infrastructure.Repositories;

internal static class SqlMappingHelpers
{
    public static Member MapMember(DbDataReader record) => new()
    {
        MemberId = record.GetGuid(record.GetOrdinal("MemberId")),
        RegistrationReference = record.GetString(record.GetOrdinal("RegistrationReference")),
        NationalId = record.GetString(record.GetOrdinal("NationalId")),
        FullName = record.GetString(record.GetOrdinal("FullName")),
        Email = record.GetString(record.GetOrdinal("Email")),
        PasswordHash = record.GetString(record.GetOrdinal("PasswordHash")),
        EmailVerificationStatus = (EmailVerificationStatus)record.GetInt32(record.GetOrdinal("EmailVerificationStatus")),
        CapabilityStatus = (CapabilityStatus)record.GetInt32(record.GetOrdinal("CapabilityStatus")),
        EmailVerifiedAt = record.IsDBNull(record.GetOrdinal("EmailVerifiedAt")) ? null : record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("EmailVerifiedAt")),
        CreatedAt = record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("CreatedAt")),
        UpdatedAt = record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("UpdatedAt"))
    };

    public static EmailVerificationChallenge MapChallenge(DbDataReader record) => new()
    {
        ChallengeId = record.GetGuid(record.GetOrdinal("ChallengeId")),
        MemberId = record.GetGuid(record.GetOrdinal("MemberId")),
        CodeHash = record.GetString(record.GetOrdinal("CodeHash")),
        KeyVersion = record.GetString(record.GetOrdinal("KeyVersion")),
        SentAt = record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("SentAt")),
        ExpiresAt = record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("ExpiresAt")),
        FailedAttemptCount = record.GetInt32(record.GetOrdinal("FailedAttemptCount")),
        Status = (ChallengeStatus)record.GetInt32(record.GetOrdinal("Status")),
        InvalidatedAt = record.IsDBNull(record.GetOrdinal("InvalidatedAt")) ? null : record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("InvalidatedAt")),
        VerifiedAt = record.IsDBNull(record.GetOrdinal("VerifiedAt")) ? null : record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("VerifiedAt")),
        LockedAt = record.IsDBNull(record.GetOrdinal("LockedAt")) ? null : record.GetFieldValue<DateTimeOffset>(record.GetOrdinal("LockedAt")),
        ReplacedByChallengeId = record.IsDBNull(record.GetOrdinal("ReplacedByChallengeId")) ? null : record.GetGuid(record.GetOrdinal("ReplacedByChallengeId"))
    };
}
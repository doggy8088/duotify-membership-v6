IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        MemberId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        RegistrationReference NVARCHAR(64) NOT NULL,
        NationalId NVARCHAR(32) NOT NULL,
        FullName NVARCHAR(128) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        EmailVerificationStatus INT NOT NULL,
        CapabilityStatus INT NOT NULL,
        EmailVerifiedAt DATETIMEOFFSET NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NOT NULL
    );

    CREATE UNIQUE INDEX UX_Members_RegistrationReference ON dbo.Members (RegistrationReference);
    CREATE UNIQUE INDEX UX_Members_NationalId ON dbo.Members (NationalId);
    CREATE UNIQUE INDEX UX_Members_Email ON dbo.Members (Email);
END
GO

IF OBJECT_ID(N'dbo.EmailVerificationChallenges', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmailVerificationChallenges
    (
        ChallengeId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        MemberId UNIQUEIDENTIFIER NOT NULL,
        CodeHash NVARCHAR(256) NOT NULL,
        KeyVersion NVARCHAR(16) NOT NULL,
        SentAt DATETIMEOFFSET NOT NULL,
        ExpiresAt DATETIMEOFFSET NOT NULL,
        FailedAttemptCount INT NOT NULL,
        Status INT NOT NULL,
        InvalidatedAt DATETIMEOFFSET NULL,
        VerifiedAt DATETIMEOFFSET NULL,
        LockedAt DATETIMEOFFSET NULL,
        ReplacedByChallengeId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_EmailVerificationChallenges_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );

    CREATE UNIQUE INDEX UX_EmailVerificationChallenges_CurrentActive
        ON dbo.EmailVerificationChallenges (MemberId)
        WHERE Status IN (0, 1);

    CREATE INDEX IX_EmailVerificationChallenges_MemberId_Status
        ON dbo.EmailVerificationChallenges (MemberId, Status);
END
GO

IF OBJECT_ID(N'dbo.SecurityAuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SecurityAuditLogs
    (
        AuditLogId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        MemberId UNIQUEIDENTIFIER NULL,
        ChallengeId UNIQUEIDENTIFIER NULL,
        EventType NVARCHAR(64) NOT NULL,
        OccurredAt DATETIMEOFFSET NOT NULL,
        IpAddress NVARCHAR(64) NULL,
        UserAgent NVARCHAR(512) NULL,
        MetadataJson NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SecurityAuditLogs_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );

    CREATE INDEX IX_SecurityAuditLogs_MemberId_OccurredAt ON dbo.SecurityAuditLogs (MemberId, OccurredAt DESC);
END
GO
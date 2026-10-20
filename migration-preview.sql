IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917132920_InitialCreate'
)
BEGIN
    CREATE TABLE [Candidates] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [CvUrl] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Candidates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917132920_InitialCreate'
)
BEGIN
    CREATE TABLE [Jobs] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Jobs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917132920_InitialCreate'
)
BEGIN
    CREATE TABLE [JobApplications] (
        [Id] int NOT NULL IDENTITY,
        [CandidateId] int NOT NULL,
        [JobId] int NOT NULL,
        [JobApplicationStatus] nvarchar(max) NOT NULL,
        [AppliedAt] datetime2 NOT NULL,
        [StatusUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_JobApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JobApplications_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JobApplications_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917132920_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JobApplications_CandidateId] ON [JobApplications] ([CandidateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917132920_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JobApplications_JobId] ON [JobApplications] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917132920_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917132920_InitialCreate', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    ALTER TABLE [Jobs] ADD [ClosedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    ALTER TABLE [Jobs] ADD [ClosedByUserId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    ALTER TABLE [Jobs] ADD [CreatedByUserId] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    ALTER TABLE [JobApplications] ADD [CancelledAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Name');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Candidates] ALTER COLUMN [Name] nvarchar(200) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'CvUrl');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Candidates] ALTER COLUMN [CvUrl] nvarchar(2000) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    ALTER TABLE [Candidates] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [TokenHash] nvarchar(256) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Candidates_UserId] ON [Candidates] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260917182321_AddIdentityAndFeatures'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917182321_AddIdentityAndFeatures', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Jobs]') AND [c].[name] = N'ClosedByUserId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Jobs] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Jobs] DROP COLUMN [ClosedByUserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Jobs]') AND [c].[name] = N'CreatedByUserId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Jobs] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Jobs] DROP COLUMN [CreatedByUserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    ALTER TABLE [Jobs] ADD [ClosedById] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    ALTER TABLE [Jobs] ADD [RecruiterId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    CREATE TABLE [Recruiters] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_Recruiters] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    CREATE INDEX [IX_Jobs_ClosedById] ON [Jobs] ([ClosedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    CREATE INDEX [IX_Jobs_RecruiterId] ON [Jobs] ([RecruiterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Recruiters_UserId] ON [Recruiters] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    ALTER TABLE [Jobs] ADD CONSTRAINT [FK_Jobs_Recruiters_ClosedById] FOREIGN KEY ([ClosedById]) REFERENCES [Recruiters] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    ALTER TABLE [Jobs] ADD CONSTRAINT [FK_Jobs_Recruiters_RecruiterId] FOREIGN KEY ([RecruiterId]) REFERENCES [Recruiters] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918145329_AddRecruiterEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260918145329_AddRecruiterEntity', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920201644_AddUniqueConstraintToJobApplication'
)
BEGIN
    DROP INDEX [IX_JobApplications_CandidateId] ON [JobApplications];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920201644_AddUniqueConstraintToJobApplication'
)
BEGIN
    CREATE UNIQUE INDEX [IX_JobApplications_CandidateId_JobId] ON [JobApplications] ([CandidateId], [JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920201644_AddUniqueConstraintToJobApplication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920201644_AddUniqueConstraintToJobApplication', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210201_AddSavedJobs'
)
BEGIN
    CREATE TABLE [SavedJobs] (
        [Id] int NOT NULL IDENTITY,
        [CandidateId] int NOT NULL,
        [JobId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SavedJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SavedJobs_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SavedJobs_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210201_AddSavedJobs'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SavedJobs_CandidateId_JobId] ON [SavedJobs] ([CandidateId], [JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210201_AddSavedJobs'
)
BEGIN
    CREATE INDEX [IX_SavedJobs_JobId] ON [SavedJobs] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210201_AddSavedJobs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924210201_AddSavedJobs', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    DROP INDEX [IX_Jobs_RecruiterId] ON [Jobs];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Recruiters] ADD [CompanyId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [ApplicationDeadline] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Benefits] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Category] nvarchar(150) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [City] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [CompanyId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Country] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Currency] nchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Department] nvarchar(150) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [EducationRequirements] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [EmploymentType] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [ExperienceLevel] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [IsSalaryVisible] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Location] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [MaximumSalary] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [MaximumYearsExperience] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [MinimumSalary] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [MinimumYearsExperience] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [NumberOfVacancies] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [PostedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [PreferredQualifications] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Requirements] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Responsibilities] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [SalaryPeriod] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [Status] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD [WorkplaceType] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [JobApplications] ADD [SubmittedCvId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Candidates] ADD [ProfileImageUrl] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE TABLE [CandidateCvs] (
        [Id] int NOT NULL IDENTITY,
        [CandidateId] int NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [OriginalFileName] nvarchar(260) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [FileSize] bigint NOT NULL,
        [Content] varbinary(max) NOT NULL,
        [IsDefault] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CandidateCvs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CandidateCvs_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [LogoUrl] nvarchar(2000) NULL,
        [Description] nvarchar(max) NULL,
        [Website] nvarchar(2000) NULL,
        [Industry] nvarchar(150) NULL,
        [CompanySize] nvarchar(100) NULL,
        [Headquarters] nvarchar(300) NULL,
        [Country] nvarchar(100) NULL,
        [City] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE TABLE [Skills] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [NormalizedName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Skills] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    DECLARE @RecruiterId int;
    DECLARE @RecruiterName nvarchar(200);
    DECLARE @CompanyId int;
    DECLARE recruiter_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT [Id], [Name] FROM [Recruiters] WHERE [CompanyId] IS NULL;

    OPEN recruiter_cursor;
    FETCH NEXT FROM recruiter_cursor INTO @RecruiterId, @RecruiterName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO [Companies]
            ([Name], [CreatedAt], [UpdatedAt])
        VALUES
            (CONCAT(@RecruiterName, ' Company'), SYSUTCDATETIME(), SYSUTCDATETIME());

        SET @CompanyId = CAST(SCOPE_IDENTITY() AS int);
        UPDATE [Recruiters] SET [CompanyId] = @CompanyId WHERE [Id] = @RecruiterId;

        FETCH NEXT FROM recruiter_cursor INTO @RecruiterId, @RecruiterName;
    END
    CLOSE recruiter_cursor;
    DEALLOCATE recruiter_cursor;

    UPDATE jobs
    SET
        jobs.[CompanyId] = recruiters.[CompanyId],
        jobs.[Department] = 'General',
        jobs.[Category] = 'General',
        jobs.[Country] = 'Unknown',
        jobs.[City] = 'Unknown',
        jobs.[WorkplaceType] = 'OnSite',
        jobs.[EmploymentType] = 'FullTime',
        jobs.[ExperienceLevel] = 'EntryLevel',
        jobs.[Responsibilities] = LEFT(jobs.[Description], 10000),
        jobs.[Requirements] = LEFT(jobs.[Description], 10000),
        jobs.[NumberOfVacancies] = 1,
        jobs.[CreatedAt] = COALESCE(jobs.[ClosedAt], SYSUTCDATETIME()),
        jobs.[UpdatedAt] = COALESCE(jobs.[ClosedAt], SYSUTCDATETIME()),
        jobs.[PostedAt] = CASE WHEN jobs.[IsActive] = 1 THEN SYSUTCDATETIME() ELSE NULL END,
        jobs.[Status] = CASE WHEN jobs.[IsActive] = 1 THEN 'Published' ELSE 'Closed' END
    FROM [Jobs] AS jobs
    INNER JOIN [Recruiters] AS recruiters ON recruiters.[Id] = jobs.[RecruiterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Jobs]') AND [c].[name] = N'IsActive');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Jobs] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Jobs] DROP COLUMN [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE TABLE [JobSkills] (
        [JobId] int NOT NULL,
        [SkillId] int NOT NULL,
        [IsRequired] bit NOT NULL,
        CONSTRAINT [PK_JobSkills] PRIMARY KEY ([JobId], [SkillId]),
        CONSTRAINT [FK_JobSkills_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_JobSkills_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Recruiters_CompanyId] ON [Recruiters] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Jobs_ApplicationDeadline] ON [Jobs] ([ApplicationDeadline]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Jobs_CompanyId_Status] ON [Jobs] ([CompanyId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Jobs_Country_City] ON [Jobs] ([Country], [City]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Jobs_PostedAt] ON [Jobs] ([PostedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Jobs_RecruiterId_Status] ON [Jobs] ([RecruiterId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Jobs_Status] ON [Jobs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_JobApplications_SubmittedCvId] ON [JobApplications] ([SubmittedCvId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CandidateCvs_CandidateId_IsDefault] ON [CandidateCvs] ([CandidateId], [IsDefault]) WHERE [IsDefault] = 1 AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_CandidateCvs_CandidateId_IsDeleted] ON [CandidateCvs] ([CandidateId], [IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_Companies_Name] ON [Companies] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE INDEX [IX_JobSkills_SkillId_IsRequired] ON [JobSkills] ([SkillId], [IsRequired]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Skills_NormalizedName] ON [Skills] ([NormalizedName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [JobApplications] ADD CONSTRAINT [FK_JobApplications_CandidateCvs_SubmittedCvId] FOREIGN KEY ([SubmittedCvId]) REFERENCES [CandidateCvs] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Jobs] ADD CONSTRAINT [FK_Jobs_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    ALTER TABLE [Recruiters] ADD CONSTRAINT [FK_Recruiters_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011221936_SyncJobsAndApplications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011221936_SyncJobsAndApplications', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    DROP INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [JobApplications] ADD [CoverLetter] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [LogoPublicId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Candidates] ADD [City] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Candidates] ADD [Country] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Candidates] ADD [ProfessionalTitle] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Candidates] ADD [ProfileImagePublicId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Candidates] ADD [Summary] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [Candidates] ADD [YearsOfExperience] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CandidateCvs]') AND [c].[name] = N'Content');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [CandidateCvs] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [CandidateCvs] ALTER COLUMN [Content] varbinary(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [CandidateCvs] ADD [ChecksumSha256] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [CandidateCvs] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [CandidateCvs] ADD [DeliveryType] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [CandidateCvs] ADD [ResourceType] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [CandidateCvs] ADD [StorageKey] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    ALTER TABLE [CandidateCvs] ADD [StorageProvider] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [AccountOneTimeCodes] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Purpose] nvarchar(50) NOT NULL,
        [CodeHash] nvarchar(88) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [ConsumedAt] datetime2 NULL,
        [FailedAttempts] int NOT NULL,
        CONSTRAINT [PK_AccountOneTimeCodes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [ApplicationStatusHistory] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationId] int NOT NULL,
        [OldStatus] nvarchar(40) NULL,
        [NewStatus] nvarchar(40) NOT NULL,
        [ChangedByUserId] nvarchar(450) NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        [Note] nvarchar(2000) NULL,
        CONSTRAINT [PK_ApplicationStatusHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApplicationStatusHistory_JobApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [Interviews] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationId] int NOT NULL,
        [ScheduledAt] datetime2 NOT NULL,
        [DurationMinutes] int NOT NULL,
        [InterviewType] nvarchar(30) NOT NULL,
        [MeetingLink] nvarchar(1000) NULL,
        [Location] nvarchar(500) NULL,
        [Notes] nvarchar(2000) NULL,
        [Status] nvarchar(30) NOT NULL,
        [ScheduledByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Interviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Interviews_JobApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [JobAlerts] (
        [Id] int NOT NULL IDENTITY,
        [CandidateId] int NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Keyword] nvarchar(200) NULL,
        [Location] nvarchar(200) NULL,
        [EmploymentType] nvarchar(40) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_JobAlerts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JobAlerts_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [NotificationPreferences] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [InAppEnabled] bit NOT NULL,
        [EmailEnabled] bit NOT NULL,
        [JobAlertsEnabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Type] nvarchar(60) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [RelatedEntityId] int NULL,
        [RelatedEntityType] nvarchar(80) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ReadAt] datetime2 NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE TABLE [OutboxMessages] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(100) NOT NULL,
        [ProtectedPayload] nvarchar(max) NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [NextAttemptAt] datetime2 NULL,
        [AttemptCount] int NOT NULL,
        [LastError] nvarchar(2000) NULL,
        CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_CandidateCvs_StorageKey] ON [CandidateCvs] ([StorageKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_AccountOneTimeCodes_UserId_Purpose_ExpiresAt] ON [AccountOneTimeCodes] ([UserId], [Purpose], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_ApplicationStatusHistory_ApplicationId_ChangedAt] ON [ApplicationStatusHistory] ([ApplicationId], [ChangedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_Interviews_ApplicationId_ScheduledAt] ON [Interviews] ([ApplicationId], [ScheduledAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_JobAlerts_CandidateId_IsActive] ON [JobAlerts] ([CandidateId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NotificationPreferences_UserId] ON [NotificationPreferences] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead_CreatedAt] ON [Notifications] ([UserId], [IsRead], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_ProcessedAt_NextAttemptAt_OccurredAt] ON [OutboxMessages] ([ProcessedAt], [NextAttemptAt], [OccurredAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    UPDATE [CandidateCvs]
    SET [StorageProvider] = 'Database', [ResourceType] = 'raw', [DeliveryType] = 'database'
    WHERE [Content] IS NOT NULL AND [StorageProvider] = '';

    UPDATE [JobApplications]
    SET [JobApplicationStatus] = 'Interview'
    WHERE [JobApplicationStatus] = 'InterView';

    INSERT INTO [ApplicationStatusHistory]
        ([ApplicationId], [OldStatus], [NewStatus], [ChangedByUserId], [ChangedAt], [Note])
    SELECT [Id], NULL, [JobApplicationStatus], 'migration', [AppliedAt], 'Imported from the existing application record.'
    FROM [JobApplications] AS [a]
    WHERE NOT EXISTS (
        SELECT 1 FROM [ApplicationStatusHistory] AS [h]
        WHERE [h].[ApplicationId] = [a].[Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020023415_AddMissingEntitiesAndCandidateCvCloudinaryFields', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020025943_AddRefreshTokenAuditFields'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [CreatedByIp] nvarchar(45) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020025943_AddRefreshTokenAuditFields'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [ReplacedByTokenHash] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020025943_AddRefreshTokenAuditFields'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [RevokedByIp] nvarchar(45) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020025943_AddRefreshTokenAuditFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020025943_AddRefreshTokenAuditFields', N'8.0.32');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020145341_AddAdminAuditAndNormalizeApplicationStatus'
)
BEGIN
    CREATE TABLE [AdminAuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [AdminUserId] nvarchar(450) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [TargetType] nvarchar(100) NOT NULL,
        [TargetId] nvarchar(450) NOT NULL,
        [Details] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AdminAuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020145341_AddAdminAuditAndNormalizeApplicationStatus'
)
BEGIN
    CREATE INDEX [IX_AdminAuditLogs_CreatedAt_AdminUserId] ON [AdminAuditLogs] ([CreatedAt], [AdminUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020145341_AddAdminAuditAndNormalizeApplicationStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020145341_AddAdminAuditAndNormalizeApplicationStatus', N'8.0.32');
END;
GO

COMMIT;
GO


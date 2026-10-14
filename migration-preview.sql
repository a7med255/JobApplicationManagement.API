BEGIN TRANSACTION;
GO

DROP INDEX [IX_Jobs_RecruiterId] ON [Jobs];
GO

ALTER TABLE [Recruiters] ADD [CompanyId] int NULL;
GO

ALTER TABLE [Jobs] ADD [ApplicationDeadline] datetime2 NULL;
GO

ALTER TABLE [Jobs] ADD [Benefits] nvarchar(max) NULL;
GO

ALTER TABLE [Jobs] ADD [Category] nvarchar(150) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [City] nvarchar(100) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [CompanyId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Jobs] ADD [Country] nvarchar(100) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [Jobs] ADD [Currency] nchar(3) NULL;
GO

ALTER TABLE [Jobs] ADD [Department] nvarchar(150) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [EducationRequirements] nvarchar(max) NULL;
GO

ALTER TABLE [Jobs] ADD [EmploymentType] nvarchar(30) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [ExperienceLevel] nvarchar(30) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [IsSalaryVisible] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Jobs] ADD [Location] nvarchar(300) NULL;
GO

ALTER TABLE [Jobs] ADD [MaximumSalary] decimal(18,2) NULL;
GO

ALTER TABLE [Jobs] ADD [MaximumYearsExperience] int NULL;
GO

ALTER TABLE [Jobs] ADD [MinimumSalary] decimal(18,2) NULL;
GO

ALTER TABLE [Jobs] ADD [MinimumYearsExperience] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Jobs] ADD [NumberOfVacancies] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Jobs] ADD [PostedAt] datetime2 NULL;
GO

ALTER TABLE [Jobs] ADD [PreferredQualifications] nvarchar(max) NULL;
GO

ALTER TABLE [Jobs] ADD [Requirements] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [Responsibilities] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [SalaryPeriod] nvarchar(30) NULL;
GO

ALTER TABLE [Jobs] ADD [Status] nvarchar(30) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Jobs] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [Jobs] ADD [WorkplaceType] nvarchar(30) NOT NULL DEFAULT N'';
GO

ALTER TABLE [JobApplications] ADD [SubmittedCvId] int NULL;
GO

ALTER TABLE [Candidates] ADD [ProfileImageUrl] nvarchar(2000) NULL;
GO

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
GO

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
GO

CREATE TABLE [Skills] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [NormalizedName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Skills] PRIMARY KEY ([Id])
);
GO

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
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Jobs]') AND [c].[name] = N'IsActive');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Jobs] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Jobs] DROP COLUMN [IsActive];
GO

CREATE TABLE [JobSkills] (
    [JobId] int NOT NULL,
    [SkillId] int NOT NULL,
    [IsRequired] bit NOT NULL,
    CONSTRAINT [PK_JobSkills] PRIMARY KEY ([JobId], [SkillId]),
    CONSTRAINT [FK_JobSkills_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JobSkills_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Recruiters_CompanyId] ON [Recruiters] ([CompanyId]);
GO

CREATE INDEX [IX_Jobs_ApplicationDeadline] ON [Jobs] ([ApplicationDeadline]);
GO

CREATE INDEX [IX_Jobs_CompanyId_Status] ON [Jobs] ([CompanyId], [Status]);
GO

CREATE INDEX [IX_Jobs_Country_City] ON [Jobs] ([Country], [City]);
GO

CREATE INDEX [IX_Jobs_PostedAt] ON [Jobs] ([PostedAt]);
GO

CREATE INDEX [IX_Jobs_RecruiterId_Status] ON [Jobs] ([RecruiterId], [Status]);
GO

CREATE INDEX [IX_Jobs_Status] ON [Jobs] ([Status]);
GO

CREATE INDEX [IX_JobApplications_SubmittedCvId] ON [JobApplications] ([SubmittedCvId]);
GO

CREATE UNIQUE INDEX [IX_CandidateCvs_CandidateId_IsDefault] ON [CandidateCvs] ([CandidateId], [IsDefault]) WHERE [IsDefault] = 1 AND [IsDeleted] = 0;
GO

CREATE INDEX [IX_CandidateCvs_CandidateId_IsDeleted] ON [CandidateCvs] ([CandidateId], [IsDeleted]);
GO

CREATE INDEX [IX_Companies_Name] ON [Companies] ([Name]);
GO

CREATE INDEX [IX_JobSkills_SkillId_IsRequired] ON [JobSkills] ([SkillId], [IsRequired]);
GO

CREATE UNIQUE INDEX [IX_Skills_NormalizedName] ON [Skills] ([NormalizedName]);
GO

ALTER TABLE [JobApplications] ADD CONSTRAINT [FK_JobApplications_CandidateCvs_SubmittedCvId] FOREIGN KEY ([SubmittedCvId]) REFERENCES [CandidateCvs] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Jobs] ADD CONSTRAINT [FK_Jobs_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Recruiters] ADD CONSTRAINT [FK_Recruiters_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260929132950_AddCompaniesSkillsEnhancedJobsAndCandidateCvs', N'8.0.10');
GO

COMMIT;
GO


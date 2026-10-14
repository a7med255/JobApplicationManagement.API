using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationManagement.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class AddCompaniesSkillsEnhancedJobsAndCandidateCvs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_RecruiterId",
                table: "Jobs");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Recruiters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDeadline",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Jobs",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Jobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Jobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Jobs",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EducationRequirements",
                table: "Jobs",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLevel",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSalaryVisible",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Jobs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumSalary",
                table: "Jobs",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumYearsExperience",
                table: "Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumSalary",
                table: "Jobs",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumYearsExperience",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfVacancies",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredQualifications",
                table: "Jobs",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Jobs",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Responsibilities",
                table: "Jobs",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalaryPeriod",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "WorkplaceType",
                table: "Jobs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubmittedCvId",
                table: "JobApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Candidates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CandidateCvs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCvs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateCvs_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CompanySize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Headquarters = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.Sql(
                """
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
                """);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Jobs");

            migrationBuilder.CreateTable(
                name: "JobSkills",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkills", x => new { x.JobId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_JobSkills_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_CompanyId",
                table: "Recruiters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ApplicationDeadline",
                table: "Jobs",
                column: "ApplicationDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyId_Status",
                table: "Jobs",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Country_City",
                table: "Jobs",
                columns: new[] { "Country", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PostedAt",
                table: "Jobs",
                column: "PostedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RecruiterId_Status",
                table: "Jobs",
                columns: new[] { "RecruiterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_SubmittedCvId",
                table: "JobApplications",
                column: "SubmittedCvId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvs_CandidateId_IsDefault",
                table: "CandidateCvs",
                columns: new[] { "CandidateId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvs_CandidateId_IsDeleted",
                table: "CandidateCvs",
                columns: new[] { "CandidateId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkills_SkillId_IsRequired",
                table: "JobSkills",
                columns: new[] { "SkillId", "IsRequired" });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_NormalizedName",
                table: "Skills",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_CandidateCvs_SubmittedCvId",
                table: "JobApplications",
                column: "SubmittedCvId",
                principalTable: "CandidateCvs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                table: "Jobs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recruiters_Companies_CompanyId",
                table: "Recruiters",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_CandidateCvs_SubmittedCvId",
                table: "JobApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Recruiters_Companies_CompanyId",
                table: "Recruiters");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE [Jobs] SET [IsActive] = CASE WHEN [Status] = 'Published' THEN 1 ELSE 0 END;");

            migrationBuilder.DropTable(
                name: "CandidateCvs");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "JobSkills");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Recruiters_CompanyId",
                table: "Recruiters");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ApplicationDeadline",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CompanyId_Status",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Country_City",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PostedAt",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_RecruiterId_Status",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Status",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_SubmittedCvId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Recruiters");

            migrationBuilder.DropColumn(
                name: "ApplicationDeadline",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EducationRequirements",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsSalaryVisible",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MaximumSalary",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MaximumYearsExperience",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MinimumSalary",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MinimumYearsExperience",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "NumberOfVacancies",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PostedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PreferredQualifications",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Responsibilities",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SalaryPeriod",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "WorkplaceType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SubmittedCvId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Candidates");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RecruiterId",
                table: "Jobs",
                column: "RecruiterId");
        }
    }
}

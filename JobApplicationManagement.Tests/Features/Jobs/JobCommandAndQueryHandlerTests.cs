using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.Commands;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Queries;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using JobApplicationManagement.Tests.Features.SavedJobs;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobCommandAndQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public JobCommandAndQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Skills.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Skill>(Enumerable.Empty<Skill>()));
        _unitOfWorkMock.Setup(u => u.Skills.AddAsync(It.IsAny<Skill>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.JobSkills.AddAsync(It.IsAny<JobSkill>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.JobSkills.Delete(It.IsAny<JobSkill>()));
        _unitOfWorkMock.Setup(u => u.Jobs.AddAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.Jobs.Update(It.IsAny<Job>()));
        _unitOfWorkMock.Setup(u => u.Jobs.Delete(It.IsAny<Job>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }

    private void SetupRecruiters(IEnumerable<Recruiter> recruiters) =>
        _unitOfWorkMock.Setup(u => u.Recruiters.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Recruiter>(recruiters));

    private void SetupJobs(IEnumerable<Job> jobs) =>
        _unitOfWorkMock.Setup(u => u.Jobs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Job>(jobs));

    private void SetupApplications(IEnumerable<JobApplication> applications) =>
        _unitOfWorkMock.Setup(u => u.JobApplications.Query())
            .Returns(new SavedJobTestAsyncEnumerable<JobApplication>(applications));

    private void SetupSavedJobs(IEnumerable<SavedJob> savedJobs) =>
        _unitOfWorkMock.Setup(u => u.SavedJobs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<SavedJob>(savedJobs));

    private void SetupCandidates(IEnumerable<Candidate> candidates) =>
        _unitOfWorkMock.Setup(u => u.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>(candidates));

    // ── CreateJobCommandHandler Tests ──────────────────────────────────────────

    [Fact]
    public async Task CreateJob_Throws_UnauthorizedException_When_Not_Authenticated()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(c => c.UserId).Returns((string?)null);

        var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var command = new CreateJobCommand(SampleCreateDto());

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateJob_Throws_ForbiddenException_When_User_Not_Recruiter()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("not-a-recruiter");
        SetupRecruiters(Enumerable.Empty<Recruiter>());

        var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var command = new CreateJobCommand(SampleCreateDto());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateJob_Throws_ConflictException_When_Recruiter_Has_No_Company()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-user");

        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-user", Company = null };
        SetupRecruiters(new[] { recruiter });

        var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var command = new CreateJobCommand(SampleCreateDto());

        var ex = await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Complete your company profile before creating a job.", ex.Message);
    }

    [Fact]
    public async Task CreateJob_Creates_Draft_Job_When_PublishImmediately_Is_False()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-user");

        var company = new Company { Id = 5, Name = "Careerly Corp" };
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-user", Company = company, CompanyId = company.Id };
        SetupRecruiters(new[] { recruiter });

        var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var dto = SampleCreateDto();
        dto.PublishImmediately = false;

        var result = await handler.Handle(new CreateJobCommand(dto), CancellationToken.None);

        Assert.Equal(JobStatus.Draft, result.Status);
        Assert.False(result.IsActive);
        _unitOfWorkMock.Verify(u => u.Jobs.AddAsync(It.Is<Job>(j => j.Status == JobStatus.Draft)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateJob_Creates_Published_Job_When_PublishImmediately_Is_True()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-user");

        var company = new Company { Id = 5, Name = "Careerly Corp" };
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-user", Company = company, CompanyId = company.Id };
        SetupRecruiters(new[] { recruiter });

        var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var dto = SampleCreateDto();
        dto.PublishImmediately = true;
        dto.ApplicationDeadline = DateTime.UtcNow.AddDays(14);

        var result = await handler.Handle(new CreateJobCommand(dto), CancellationToken.None);

        Assert.Equal(JobStatus.Published, result.Status);
        Assert.True(result.IsActive);
        _unitOfWorkMock.Verify(u => u.Jobs.AddAsync(It.Is<Job>(j => j.Status == JobStatus.Published)), Times.Once);
    }

    // ── UpdateJobCommandHandler Tests ──────────────────────────────────────────

    [Fact]
    public async Task UpdateJob_Throws_NotFoundException_When_Job_Not_Found()
    {
        SetupJobs(Enumerable.Empty<Job>());
        var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateJobCommand(99, SampleUpdateDto()), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateJob_Throws_ForbiddenException_When_Stranger_Recruiter()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-stranger");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var strangerRecruiter = new Recruiter { Id = 99, UserId = "recruiter-stranger" };
        SetupRecruiters(new[] { strangerRecruiter });

        var job = new Job { Id = 1, RecruiterId = 10, Status = JobStatus.Draft };
        SetupJobs(new[] { job });

        var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateJobCommand(1, SampleUpdateDto()), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateJob_Throws_ConflictException_When_Job_Is_Archived()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var job = new Job { Id = 1, RecruiterId = 10, Status = JobStatus.Archived };
        SetupJobs(new[] { job });

        var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateJobCommand(1, SampleUpdateDto()), CancellationToken.None));
        Assert.Equal("An archived job cannot be updated.", ex.Message);
    }

    [Fact]
    public async Task UpdateJob_Updates_Job_Details_When_Authorized()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var company = new Company { Id = 2, Name = "Tech Corp" };
        var job = new Job
        {
            Id = 1,
            RecruiterId = 10,
            Company = company,
            CompanyId = company.Id,
            Status = JobStatus.Published,
            Title = "Old Title",
            Description = "Old Description",
            Department = "Old Dept",
            Category = "Old Cat",
            Country = "Egypt",
            City = "Cairo",
            Responsibilities = "Old Resp",
            Requirements = "Old Req",
            NumberOfVacancies = 1
        };
        SetupJobs(new[] { job });

        var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var updateDto = SampleUpdateDto();
        updateDto.Title = "Updated Principal Engineer";

        var result = await handler.Handle(new UpdateJobCommand(1, updateDto), CancellationToken.None);

        Assert.Equal("Updated Principal Engineer", result.Title);
        _unitOfWorkMock.Verify(u => u.Jobs.Update(job), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── Close, Reopen, Archive & Delete Tests ──────────────────────────────────

    [Fact]
    public async Task CloseJob_Throws_Conflict_When_Not_Published()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var draftJob = new Job { Id = 1, RecruiterId = 10, Status = JobStatus.Draft };
        SetupJobs(new[] { draftJob });

        var handler = new CloseJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CloseJobCommand(1), CancellationToken.None));
    }

    [Fact]
    public async Task CloseJob_Closes_Published_Job()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var publishedJob = new Job
        {
            Id = 1,
            RecruiterId = 10,
            Status = JobStatus.Published,
            Company = new Company { Id = 1, Name = "Co" },
            Title = "Job",
            Description = "Desc",
            Department = "Dept",
            Category = "Cat",
            Country = "EG",
            City = "CAI",
            Responsibilities = "Resp",
            Requirements = "Req"
        };
        SetupJobs(new[] { publishedJob });

        var handler = new CloseJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new CloseJobCommand(1), CancellationToken.None);

        Assert.Equal(JobStatus.Closed, result.Status);
        Assert.False(result.IsActive);
        _unitOfWorkMock.Verify(u => u.Jobs.Update(publishedJob), Times.Once);
    }

    [Fact]
    public async Task ReopenJob_Reopens_Closed_Job()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var closedJob = new Job
        {
            Id = 1,
            RecruiterId = 10,
            Status = JobStatus.Closed,
            ApplicationDeadline = DateTime.UtcNow.AddDays(7),
            Company = new Company { Id = 1, Name = "Co" },
            Title = "Job",
            Description = "Desc",
            Department = "Dept",
            Category = "Cat",
            Country = "EG",
            City = "CAI",
            Responsibilities = "Resp",
            Requirements = "Req"
        };
        SetupJobs(new[] { closedJob });

        var handler = new ReopenJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new ReopenJobCommand(1), CancellationToken.None);

        Assert.Equal(JobStatus.Published, result.Status);
        Assert.True(result.IsActive);
        _unitOfWorkMock.Verify(u => u.Jobs.Update(closedJob), Times.Once);
    }

    [Fact]
    public async Task ArchiveJob_Throws_Conflict_When_Job_Is_Published()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var publishedJob = new Job { Id = 1, RecruiterId = 10, Status = JobStatus.Published };
        SetupJobs(new[] { publishedJob });

        var handler = new ArchiveJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new ArchiveJobCommand(1), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteJob_Throws_Conflict_When_Job_Has_Applications()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var draftJob = new Job { Id = 1, RecruiterId = 10, Status = JobStatus.Draft };
        _unitOfWorkMock.Setup(u => u.Jobs.GetByIdAsync(1)).ReturnsAsync(draftJob);
        SetupApplications(new[] { new JobApplication { Id = 100, JobId = 1 } });
        SetupSavedJobs(Enumerable.Empty<SavedJob>());

        var handler = new DeleteJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new DeleteJobCommand(1), CancellationToken.None));
        Assert.Contains("applications or saved-job references", ex.Message);
    }

    [Fact]
    public async Task DeleteJob_Successfully_Deletes_Draft_With_No_Dependencies()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var draftJob = new Job { Id = 1, RecruiterId = 10, Status = JobStatus.Draft };
        _unitOfWorkMock.Setup(u => u.Jobs.GetByIdAsync(1)).ReturnsAsync(draftJob);
        SetupApplications(Enumerable.Empty<JobApplication>());
        SetupSavedJobs(Enumerable.Empty<SavedJob>());

        var handler = new DeleteJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        await handler.Handle(new DeleteJobCommand(1), CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.Jobs.Delete(draftJob), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GetJobByIdQueryHandler Tests ───────────────────────────────────────────

    [Fact]
    public async Task GetJobById_Returns_Published_Job_For_Anonymous_User()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(false);

        var publishedJob = new Job
        {
            Id = 1,
            Title = "Public Software Engineer",
            Status = JobStatus.Published,
            ApplicationDeadline = DateTime.UtcNow.AddDays(10),
            Company = new Company { Id = 1, Name = "Careerly" },
            Description = "Desc",
            Department = "Dept",
            Category = "Cat",
            Country = "EG",
            City = "CAI",
            Responsibilities = "Resp",
            Requirements = "Req"
        };
        SetupJobs(new[] { publishedJob });

        var handler = new GetJobByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetJobByIdQuery(1), CancellationToken.None);

        Assert.Equal("Public Software Engineer", result.Title);
        Assert.False(result.IsSaved);
        Assert.Null(result.ApplicationId);
    }

    [Fact]
    public async Task GetJobById_Throws_NotFound_For_Draft_Job_When_Anonymous()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(false);

        var draftJob = new Job
        {
            Id = 1,
            Title = "Secret Draft Job",
            Status = JobStatus.Draft
        };
        SetupJobs(new[] { draftJob });

        var handler = new GetJobByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetJobByIdQuery(1), CancellationToken.None));
    }

    [Fact]
    public async Task GetJobById_Returns_Draft_Job_For_Owning_Recruiter()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var owner = new Recruiter { Id = 10, UserId = "recruiter-owner" };
        SetupRecruiters(new[] { owner });

        var draftJob = new Job
        {
            Id = 1,
            RecruiterId = 10,
            Title = "My Draft Job",
            Status = JobStatus.Draft,
            Company = new Company { Id = 1, Name = "Careerly" },
            Description = "Desc",
            Department = "Dept",
            Category = "Cat",
            Country = "EG",
            City = "CAI",
            Responsibilities = "Resp",
            Requirements = "Req"
        };
        SetupJobs(new[] { draftJob });

        var handler = new GetJobByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetJobByIdQuery(1), CancellationToken.None);

        Assert.Equal("My Draft Job", result.Title);
    }

    private static CreateJobDto SampleCreateDto() => new()
    {
        Title = "Full Stack .NET Developer",
        Description = "Develop cloud applications with ASP.NET Core.",
        Department = "Software",
        Category = "Development",
        Country = "Egypt",
        City = "Cairo",
        WorkplaceType = WorkplaceType.Hybrid,
        EmploymentType = EmploymentType.FullTime,
        ExperienceLevel = ExperienceLevel.MidLevel,
        MinimumYearsExperience = 3,
        MaximumYearsExperience = 6,
        Responsibilities = "Design and build scalable APIs.",
        Requirements = "Proficient with C#, SQL Server, Angular.",
        NumberOfVacancies = 2,
        RequiredSkills = [".NET", "C#", "SQL"]
    };

    private static UpdateJobDto SampleUpdateDto() => new()
    {
        Title = "Senior Full Stack .NET Developer",
        Description = "Lead backend API design.",
        Department = "Software",
        Category = "Development",
        Country = "Egypt",
        City = "Cairo",
        WorkplaceType = WorkplaceType.Remote,
        EmploymentType = EmploymentType.FullTime,
        ExperienceLevel = ExperienceLevel.Senior,
        MinimumYearsExperience = 5,
        MaximumYearsExperience = 8,
        Responsibilities = "Lead feature development and mentorship.",
        Requirements = "5+ years .NET and Angular experience.",
        NumberOfVacancies = 1,
        RequiredSkills = [".NET", "C#", "EF Core"]
    };
}

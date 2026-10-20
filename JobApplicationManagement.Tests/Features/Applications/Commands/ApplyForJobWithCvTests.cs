using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.Commands;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using JobApplicationManagement.Tests.Features.SavedJobs;
using Moq;

namespace JobApplicationManagement.Tests.Features.Applications.Commands;

public class ApplyForJobWithCvTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<IEmailQueue> _emailQueue = new();

    [Fact]
    public async Task Candidate_Can_Apply_Using_Own_Cv()
    {
        var candidate = new Candidate { Id = 1, UserId = "candidate-1", Name = "Candidate" };
        var job = PublishedJob();
        var cv = new CandidateCv
        {
            Id = 7,
            CandidateId = candidate.Id,
            DisplayName = "Backend CV",
            OriginalFileName = "backend.pdf",
            ContentType = "application/pdf",
            Content = [1]
        };
        Setup(candidate, job, [cv], []);
        JobApplication? savedApplication = null;
        _unitOfWork.Setup(unit => unit.JobApplications.AddAsync(It.IsAny<JobApplication>()))
            .Callback<JobApplication>(application => savedApplication = application)
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(unit => unit.SaveChangesAsync()).ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new ApplyForJobCommand(new CreateJobApplicationDto { JobId = job.Id, CandidateCvId = cv.Id }),
            CancellationToken.None);

        Assert.Equal(cv.Id, result.SubmittedCvId);
        Assert.Equal(cv.Id, savedApplication!.SubmittedCvId);
        Assert.Equal(JobApplicationStatus.Applied, savedApplication.JobApplicationStatus);
    }

    [Fact]
    public async Task Candidate_Cannot_Apply_Using_Another_Candidates_Cv()
    {
        var candidate = new Candidate { Id = 1, UserId = "candidate-1" };
        var foreignCv = new CandidateCv { Id = 9, CandidateId = 2, DisplayName = "Foreign" };
        var job = PublishedJob();
        Setup(candidate, job, [foreignCv], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new ApplyForJobCommand(new CreateJobApplicationDto { JobId = job.Id, CandidateCvId = foreignCv.Id }),
            CancellationToken.None));

        _unitOfWork.Verify(unit => unit.JobApplications.AddAsync(It.IsAny<JobApplication>()), Times.Never);
    }

    [Theory]
    [InlineData(JobStatus.Draft, false)]
    [InlineData(JobStatus.Closed, false)]
    [InlineData(JobStatus.Expired, false)]
    [InlineData(JobStatus.Published, true)]
    public async Task Candidate_Cannot_Apply_To_Unavailable_Job(JobStatus status, bool pastDeadline)
    {
        var candidate = new Candidate { Id = 1, UserId = "candidate-1" };
        var job = PublishedJob();
        job.Status = status;
        job.ApplicationDeadline = pastDeadline ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1);
        Setup(candidate, job, [], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new ApplyForJobCommand(new CreateJobApplicationDto { JobId = job.Id }),
            CancellationToken.None));
    }

    private void Setup(
        Candidate candidate,
        Job job,
        IEnumerable<CandidateCv> cvs,
        IEnumerable<JobApplication> applications)
    {
        _currentUser.Setup(service => service.UserId).Returns(candidate.UserId);
        _unitOfWork.Setup(unit => unit.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>([candidate]));
        _unitOfWork.Setup(unit => unit.Jobs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Job>([job]));
        _unitOfWork.Setup(unit => unit.CandidateCvs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<CandidateCv>(cvs));
        _unitOfWork.Setup(unit => unit.JobApplications.Query())
            .Returns(new SavedJobTestAsyncEnumerable<JobApplication>(applications));
        _unitOfWork.Setup(unit => unit.NotificationPreferences.Query())
            .Returns(new SavedJobTestAsyncEnumerable<NotificationPreference>(Array.Empty<NotificationPreference>()));
        _identity.Setup(x => x.GetUserByIdAsync(candidate.UserId))
            .ReturnsAsync((candidate.UserId, "candidate@example.com", (IEnumerable<string>)["Candidate"]));
        if (job.Recruiter is not null)
        {
            _identity.Setup(x => x.GetUserByIdAsync(job.Recruiter.UserId))
                .ReturnsAsync((job.Recruiter.UserId, "recruiter@example.com", (IEnumerable<string>)["Recruiter"]));
        }
        _emailQueue.Setup(x => x.QueueAsync(It.IsAny<JobApplicationManagement.Application.Common.Models.EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.ApplicationStatusHistory.AddAsync(It.IsAny<ApplicationStatusHistory>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.Notifications.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((operation, token) => operation(token));
    }

    private ApplyForJobCommandHandler CreateHandler() =>
        new(_unitOfWork.Object, _currentUser.Object, _identity.Object, _emailQueue.Object);

    private static Job PublishedJob() => new()
    {
        Id = 10,
        Title = "Developer",
        Status = JobStatus.Published,
        ApplicationDeadline = DateTime.UtcNow.AddDays(7)
        ,Company = new Company { Id = 3, Name = "Careerly" }
        ,Recruiter = new Recruiter { Id = 4, UserId = "recruiter-1", Name = "Recruiter" }
    };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.Services;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using JobApplicationManagement.Tests.Features.SavedJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobExpirationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<JobExpirationService>> _loggerMock = new();
    private readonly JobExpirationService _service;

    public JobExpirationServiceTests()
    {
        _service = new JobExpirationService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    private void SetupJobs(IEnumerable<Job> jobs)
    {
        var asyncEnumerable = new SavedJobTestAsyncEnumerable<Job>(jobs);
        _unitOfWorkMock.Setup(u => u.Jobs.Query()).Returns(asyncEnumerable);
    }

    [Fact]
    public async Task ExpireJobsAsync_Expires_Past_Deadline_Published_Jobs_And_Saves()
    {
        var now = DateTime.UtcNow;
        var expiredJob1 = new Job
        {
            Id = 1,
            Title = "Job 1",
            Status = JobStatus.Published,
            ApplicationDeadline = now.AddHours(-2),
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now.AddDays(-10)
        };
        var expiredJob2 = new Job
        {
            Id = 2,
            Title = "Job 2",
            Status = JobStatus.Published,
            ApplicationDeadline = now.AddMinutes(-5),
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-5)
        };
        var futureJob = new Job
        {
            Id = 3,
            Title = "Job 3",
            Status = JobStatus.Published,
            ApplicationDeadline = now.AddDays(5),
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddDays(-1)
        };

        SetupJobs(new[] { expiredJob1, expiredJob2, futureJob });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(2);

        await _service.ExpireJobsAsync(CancellationToken.None);

        Assert.Equal(JobStatus.Expired, expiredJob1.Status);
        Assert.NotNull(expiredJob1.ClosedAt);
        Assert.False(expiredJob1.IsActive);

        Assert.Equal(JobStatus.Expired, expiredJob2.Status);
        Assert.NotNull(expiredJob2.ClosedAt);
        Assert.False(expiredJob2.IsActive);

        Assert.Equal(JobStatus.Published, futureJob.Status);
        Assert.True(futureJob.IsActive);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExpireJobsAsync_Does_Not_Save_When_No_Jobs_Expired()
    {
        var now = DateTime.UtcNow;
        var futureJob = new Job
        {
            Id = 1,
            Title = "Active Job",
            Status = JobStatus.Published,
            ApplicationDeadline = now.AddDays(10),
            CreatedAt = now,
            UpdatedAt = now
        };
        var jobWithoutDeadline = new Job
        {
            Id = 2,
            Title = "Open Job",
            Status = JobStatus.Published,
            ApplicationDeadline = null,
            CreatedAt = now,
            UpdatedAt = now
        };
        var draftJobWithPastDeadline = new Job
        {
            Id = 3,
            Title = "Draft Job",
            Status = JobStatus.Draft,
            ApplicationDeadline = now.AddDays(-1),
            CreatedAt = now,
            UpdatedAt = now
        };

        SetupJobs(new[] { futureJob, jobWithoutDeadline, draftJobWithPastDeadline });

        await _service.ExpireJobsAsync(CancellationToken.None);

        Assert.Equal(JobStatus.Published, futureJob.Status);
        Assert.Equal(JobStatus.Published, jobWithoutDeadline.Status);
        Assert.Equal(JobStatus.Draft, draftJobWithPastDeadline.Status);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}

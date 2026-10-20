using System;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Features.Jobs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobLifecycleTests
{
    private static DateTime Now => DateTime.UtcNow;

    [Fact]
    public void Publish_Draft_Marks_Job_Published()
    {
        var now = Now;
        var job = NewJob(JobStatus.Draft, now.AddDays(7), now);

        JobLifecycle.Publish(job, now);

        Assert.Equal(JobStatus.Published, job.Status);
        Assert.Equal(now, job.PostedAt);
        Assert.True(job.IsActive);
    }

    [Fact]
    public void Publish_With_Past_Deadline_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Draft, now.AddMinutes(-1), now);

        Assert.Throws<ConflictException>(() => JobLifecycle.Publish(job, now));
    }

    [Fact]
    public void Publish_Already_Published_Job_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Published, now.AddDays(7), now);

        var ex = Assert.Throws<ConflictException>(() => JobLifecycle.Publish(job, now));
        Assert.Equal("The job is already published.", ex.Message);
    }

    [Fact]
    public void Archived_Job_Cannot_Be_Published()
    {
        var now = Now;
        var job = NewJob(JobStatus.Archived, now.AddDays(1), now);

        var ex = Assert.Throws<ConflictException>(() => JobLifecycle.Publish(job, now));
        Assert.Equal("An archived job cannot be published.", ex.Message);
    }

    [Fact]
    public void Close_Published_Job_Stops_Applications()
    {
        var now = Now;
        var job = NewJob(JobStatus.Published, now.AddDays(7), now);

        JobLifecycle.Close(job, now, 12);

        Assert.Equal(JobStatus.Closed, job.Status);
        Assert.Equal(12, job.ClosedById);
        Assert.Equal(now, job.ClosedAt);
        Assert.False(JobLifecycle.CanAcceptApplications(job, now));
    }

    [Fact]
    public void Close_Non_Published_Job_Fails()
    {
        var now = Now;
        var draftJob = NewJob(JobStatus.Draft, now.AddDays(7), now);
        Assert.Throws<ConflictException>(() => JobLifecycle.Close(draftJob, now, 1));

        var archivedJob = NewJob(JobStatus.Archived, now.AddDays(7), now);
        Assert.Throws<ConflictException>(() => JobLifecycle.Close(archivedJob, now, 1));

        var expiredJob = NewJob(JobStatus.Expired, now.AddDays(-1), now);
        Assert.Throws<ConflictException>(() => JobLifecycle.Close(expiredJob, now, 1));
    }

    [Fact]
    public void Archive_Published_Job_Without_Closing_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Published, now.AddDays(7), now);

        var ex = Assert.Throws<ConflictException>(() => JobLifecycle.Archive(job, now));
        Assert.Equal("Close the job before archiving it.", ex.Message);
    }

    [Fact]
    public void Archive_Already_Archived_Job_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Archived, now.AddDays(7), now);

        var ex = Assert.Throws<ConflictException>(() => JobLifecycle.Archive(job, now));
        Assert.Equal("The job is already archived.", ex.Message);
    }

    [Fact]
    public void Archive_Draft_Or_Closed_Job_Succeeds()
    {
        var now = Now;
        var draftJob = NewJob(JobStatus.Draft, now.AddDays(7), now);
        JobLifecycle.Archive(draftJob, now);
        Assert.Equal(JobStatus.Archived, draftJob.Status);

        var closedJob = NewJob(JobStatus.Closed, now.AddDays(7), now);
        JobLifecycle.Archive(closedJob, now);
        Assert.Equal(JobStatus.Archived, closedJob.Status);
    }

    [Fact]
    public void Expire_Published_Job_After_Deadline()
    {
        var now = Now;
        var job = NewJob(JobStatus.Published, now.AddSeconds(-1), now);

        JobLifecycle.Expire(job, now);

        Assert.Equal(JobStatus.Expired, job.Status);
        Assert.Equal(now, job.ClosedAt);
        Assert.False(job.IsActive);
    }

    [Fact]
    public void Expire_Before_Deadline_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Published, now.AddDays(1), now);

        Assert.Throws<ConflictException>(() => JobLifecycle.Expire(job, now));
    }

    [Fact]
    public void Expire_Without_Deadline_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Published, null, now);

        Assert.Throws<ConflictException>(() => JobLifecycle.Expire(job, now));
    }

    [Fact]
    public void Expire_Draft_Job_Fails()
    {
        var now = Now;
        var job = NewJob(JobStatus.Draft, now.AddDays(-1), now);

        Assert.Throws<ConflictException>(() => JobLifecycle.Expire(job, now));
    }

    [Theory]
    [InlineData(JobStatus.Published, 5, true)]
    [InlineData(JobStatus.Published, null, true)]
    [InlineData(JobStatus.Published, -5, false)]
    [InlineData(JobStatus.Draft, 5, false)]
    [InlineData(JobStatus.Closed, 5, false)]
    [InlineData(JobStatus.Expired, -5, false)]
    [InlineData(JobStatus.Archived, 5, false)]
    public void CanAcceptApplications_Matrix_Check(JobStatus status, int? deadlineDaysOffset, bool expectedCanAccept)
    {
        var now = Now;
        DateTime? deadline = deadlineDaysOffset.HasValue ? now.AddDays(deadlineDaysOffset.Value) : null;
        var job = NewJob(status, deadline, now);

        var canAccept = JobLifecycle.CanAcceptApplications(job, now);
        Assert.Equal(expectedCanAccept, canAccept);
    }

    private static Job NewJob(JobStatus status, DateTime? deadline, DateTime now) => new()
    {
        Status = status,
        ApplicationDeadline = deadline,
        CreatedAt = now,
        UpdatedAt = now
    };
}

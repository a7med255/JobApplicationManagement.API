using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Features.Jobs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobLifecycleTests
{
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Publish_Draft_Marks_Job_Published()
    {
        var job = NewJob(JobStatus.Draft, Now.AddDays(7));

        JobLifecycle.Publish(job, Now);

        Assert.Equal(JobStatus.Published, job.Status);
        Assert.Equal(Now, job.PostedAt);
        Assert.True(job.IsActive);
    }

    [Fact]
    public void Publish_With_Past_Deadline_Fails()
    {
        var job = NewJob(JobStatus.Draft, Now.AddMinutes(-1));

        Assert.Throws<ConflictException>(() => JobLifecycle.Publish(job, Now));
    }

    [Fact]
    public void Close_Published_Job_Stops_Applications()
    {
        var job = NewJob(JobStatus.Published, Now.AddDays(7));

        JobLifecycle.Close(job, Now, 12);

        Assert.Equal(JobStatus.Closed, job.Status);
        Assert.Equal(12, job.ClosedById);
        Assert.False(JobLifecycle.CanAcceptApplications(job, Now));
    }

    [Fact]
    public void Expire_Published_Job_After_Deadline()
    {
        var job = NewJob(JobStatus.Published, Now.AddSeconds(-1));

        JobLifecycle.Expire(job, Now);

        Assert.Equal(JobStatus.Expired, job.Status);
        Assert.Equal(Now, job.ClosedAt);
        Assert.False(job.IsActive);
    }

    [Fact]
    public void Expire_Before_Deadline_Fails()
    {
        var job = NewJob(JobStatus.Published, Now.AddDays(1));

        Assert.Throws<ConflictException>(() => JobLifecycle.Expire(job, Now));
    }

    [Fact]
    public void Archived_Job_Cannot_Be_Published()
    {
        var job = NewJob(JobStatus.Archived, Now.AddDays(1));

        Assert.Throws<ConflictException>(() => JobLifecycle.Publish(job, Now));
    }

    private static Job NewJob(JobStatus status, DateTime? deadline) => new()
    {
        Status = status,
        ApplicationDeadline = deadline,
        CreatedAt = Now,
        UpdatedAt = Now
    };
}

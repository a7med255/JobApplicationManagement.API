using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Jobs;

public static class JobLifecycle
{
    public static void Publish(Job job, DateTime utcNow)
    {
        if (job.Status == JobStatus.Archived)
            throw new ConflictException("An archived job cannot be published.");

        if (job.Status == JobStatus.Published)
            throw new ConflictException("The job is already published.");

        if (job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value <= utcNow)
            throw new ConflictException("The application deadline must be in the future before publishing.");

        job.Status = JobStatus.Published;
        job.PostedAt ??= utcNow;
        job.ClosedAt = null;
        job.ClosedById = null;
        job.UpdatedAt = utcNow;
    }

    public static void Close(Job job, DateTime utcNow, int? closedByRecruiterId)
    {
        if (job.Status != JobStatus.Published)
            throw new ConflictException("Only a published job can be closed.");

        job.Status = JobStatus.Closed;
        job.ClosedAt = utcNow;
        job.ClosedById = closedByRecruiterId;
        job.UpdatedAt = utcNow;
    }

    public static void Archive(Job job, DateTime utcNow)
    {
        if (job.Status == JobStatus.Published)
            throw new ConflictException("Close the job before archiving it.");

        if (job.Status == JobStatus.Archived)
            throw new ConflictException("The job is already archived.");

        job.Status = JobStatus.Archived;
        job.UpdatedAt = utcNow;
    }

    public static void Expire(Job job, DateTime utcNow)
    {
        if (job.Status != JobStatus.Published ||
            !job.ApplicationDeadline.HasValue ||
            job.ApplicationDeadline.Value > utcNow)
        {
            throw new ConflictException("Only a published job whose deadline has passed can expire.");
        }

        job.Status = JobStatus.Expired;
        job.ClosedAt = utcNow;
        job.ClosedById = null;
        job.UpdatedAt = utcNow;
    }

    public static bool CanAcceptApplications(Job job, DateTime utcNow) =>
        job.Status == JobStatus.Published &&
        (!job.ApplicationDeadline.HasValue || job.ApplicationDeadline.Value > utcNow);
}

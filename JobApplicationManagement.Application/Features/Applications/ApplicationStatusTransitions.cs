using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Applications;

public static class ApplicationStatusTransitions
{
    private static readonly IReadOnlyDictionary<JobApplicationStatus, JobApplicationStatus[]> Allowed =
        new Dictionary<JobApplicationStatus, JobApplicationStatus[]>
        {
            [JobApplicationStatus.Applied] = [JobApplicationStatus.UnderReview, JobApplicationStatus.Shortlisted, JobApplicationStatus.Rejected, JobApplicationStatus.Cancelled],
            [JobApplicationStatus.UnderReview] = [JobApplicationStatus.Shortlisted, JobApplicationStatus.Interview, JobApplicationStatus.Accepted, JobApplicationStatus.Rejected, JobApplicationStatus.Cancelled],
            [JobApplicationStatus.Shortlisted] = [JobApplicationStatus.Interview, JobApplicationStatus.Accepted, JobApplicationStatus.Rejected],
            [JobApplicationStatus.Interview] = [JobApplicationStatus.Accepted, JobApplicationStatus.Rejected],
            [JobApplicationStatus.Accepted] = [],
            [JobApplicationStatus.Rejected] = [],
            [JobApplicationStatus.Cancelled] = []
        };

    public static bool CanTransition(JobApplicationStatus from, JobApplicationStatus to) =>
        from != to && Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}

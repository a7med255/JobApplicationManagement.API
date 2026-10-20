namespace JobApplicationManagement.Application.Features.Interviews;

public interface IInterviewService
{
    Task<IReadOnlyCollection<InterviewDto>> GetForApplicationAsync(int applicationId, CancellationToken cancellationToken);
    Task<InterviewDto> ScheduleAsync(ScheduleInterviewDto request, CancellationToken cancellationToken);
    Task<InterviewDto> RescheduleAsync(int id, RescheduleInterviewDto request, CancellationToken cancellationToken);
    Task CancelAsync(int id, CancellationToken cancellationToken);
}

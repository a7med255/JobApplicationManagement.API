using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Enums;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record CancelApplicationCommand(int ApplicationId) : IRequest;

public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    public CancelApplicationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(request.ApplicationId);

        if (application is null)
            throw new NotFoundException("JobApplication", request.ApplicationId);

        var userId = _currentUserService.UserId ?? throw new ForbiddenException("An authenticated user is required.");
        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
            throw new ForbiddenException("No candidate profile found for the authenticated user.");

        if (application.CandidateId != candidate.Id)
            throw new ForbiddenException("You do not have permission to cancel this application.");

        if (application.JobApplicationStatus != JobApplicationStatus.Applied &&
            application.JobApplicationStatus != JobApplicationStatus.UnderReview)
        {
            throw new ConflictException(
                $"Cannot cancel application with status '{application.JobApplicationStatus}'. " +
                "Only applications with status 'Applied' or 'UnderReview' can be cancelled.");
        }

        var oldStatus = application.JobApplicationStatus;
        var now = DateTime.UtcNow;
        application.JobApplicationStatus = JobApplicationStatus.Cancelled;
        application.CancelledAt = now;
        application.StatusUpdatedAt = now;

        await _unitOfWork.ApplicationStatusHistory.AddAsync(new ApplicationStatusHistory
        {
            ApplicationId = application.Id,
            OldStatus = oldStatus,
            NewStatus = JobApplicationStatus.Cancelled,
            ChangedByUserId = userId,
            ChangedAt = now,
            Note = "Cancelled by candidate"
        });

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

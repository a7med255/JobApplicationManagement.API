using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Queries;

public record GetSimilarJobsQuery(int JobId, int Limit = 4) : IRequest<IReadOnlyCollection<JobSummaryDto>>;
public record GetRecommendedJobsQuery(int Limit = 6) : IRequest<IReadOnlyCollection<JobSummaryDto>>;

public sealed class GetSimilarJobsQueryHandler : IRequestHandler<GetSimilarJobsQuery, IReadOnlyCollection<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetSimilarJobsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<IReadOnlyCollection<JobSummaryDto>> Handle(GetSimilarJobsQuery request, CancellationToken cancellationToken)
    {
        var source = await _unitOfWork.Jobs.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.JobId);
        var now = DateTime.UtcNow;
        var jobs = await _unitOfWork.Jobs.Query().AsNoTracking()
            .Where(x => x.Id != source.Id && x.Status == JobStatus.Published &&
                        (!x.ApplicationDeadline.HasValue || x.ApplicationDeadline > now) &&
                        (x.Category == source.Category || x.City == source.City || x.EmploymentType == source.EmploymentType))
            .OrderByDescending(x => x.Category == source.Category)
            .ThenByDescending(x => x.City == source.City)
            .ThenByDescending(x => x.PostedAt)
            .Include(x => x.Company).Include(x => x.JobSkills).ThenInclude(x => x.Skill)
            .Take(Math.Clamp(request.Limit, 1, 12)).ToListAsync(cancellationToken);
        return jobs.Select(JobDtoFactory.ToSummary).ToArray();
    }
}

public sealed class GetRecommendedJobsQueryHandler : IRequestHandler<GetRecommendedJobsQuery, IReadOnlyCollection<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public GetRecommendedJobsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);
    public async Task<IReadOnlyCollection<JobSummaryDto>> Handle(GetRecommendedJobsQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenException("A candidate profile is required.");
        var appliedIds = _unitOfWork.JobApplications.Query().Where(x => x.CandidateId == candidate.Id).Select(x => x.JobId);
        var now = DateTime.UtcNow;
        var jobs = await _unitOfWork.Jobs.Query().AsNoTracking()
            .Where(x => x.Status == JobStatus.Published && (!x.ApplicationDeadline.HasValue || x.ApplicationDeadline > now) && !appliedIds.Contains(x.Id))
            .OrderByDescending(x => candidate.City != null && x.City == candidate.City)
            .ThenByDescending(x => candidate.Country != null && x.Country == candidate.Country)
            .ThenByDescending(x => candidate.ProfessionalTitle != null && x.Title.Contains(candidate.ProfessionalTitle))
            .ThenByDescending(x => x.PostedAt)
            .Include(x => x.Company).Include(x => x.JobSkills).ThenInclude(x => x.Skill)
            .Take(Math.Clamp(request.Limit, 1, 12)).ToListAsync(cancellationToken);
        return jobs.Select(JobDtoFactory.ToSummary).ToArray();
    }
}

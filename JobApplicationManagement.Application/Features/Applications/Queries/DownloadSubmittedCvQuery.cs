using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Queries;

public record DownloadSubmittedCvQuery(int ApplicationId) : IRequest<FileDownloadDto>;

public class DownloadSubmittedCvQueryHandler : IRequestHandler<DownloadSubmittedCvQuery, FileDownloadDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DownloadSubmittedCvQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<FileDownloadDto> Handle(
        DownloadSubmittedCvQuery request,
        CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.JobApplications.Query().AsNoTracking()
            .Include(item => item.Job)
            .Include(item => item.SubmittedCv)
            .FirstOrDefaultAsync(item => item.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(JobApplication), request.ApplicationId);
        if (application.SubmittedCv is null)
            throw new NotFoundException(nameof(CandidateCv), "submitted CV");

        var authorized = _currentUserService.Roles.Contains("Admin");
        if (!authorized && _currentUserService.Roles.Contains("Candidate"))
        {
            authorized = await _unitOfWork.Candidates.Query().AsNoTracking()
                .AnyAsync(candidate => candidate.UserId == _currentUserService.UserId &&
                                       candidate.Id == application.CandidateId, cancellationToken);
        }
        if (!authorized && _currentUserService.Roles.Contains("Recruiter"))
        {
            authorized = await _unitOfWork.Recruiters.Query().AsNoTracking()
                .AnyAsync(recruiter => recruiter.UserId == _currentUserService.UserId &&
                                       recruiter.Id == application.Job.RecruiterId, cancellationToken);
        }
        if (!authorized)
            throw new ForbiddenException("You do not have permission to download this submitted CV.");

        return new FileDownloadDto(
            application.SubmittedCv.Content,
            application.SubmittedCv.ContentType,
            application.SubmittedCv.OriginalFileName);
    }
}

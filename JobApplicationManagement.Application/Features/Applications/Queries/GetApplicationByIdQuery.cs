using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Queries;

public record GetApplicationByIdQuery(int Id) : IRequest<JobApplicationResponseDto>;

public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, JobApplicationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetApplicationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<JobApplicationResponseDto> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.JobApplications.Query()
            .AsNoTracking()
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (application is null)
            throw new NotFoundException(nameof(JobApplication), request.Id);

        var rolesList = _currentUserService.Roles.ToList();
        var userId = _currentUserService.UserId;

        if (!rolesList.Contains("Admin"))
        {
            if (rolesList.Contains("Recruiter"))
            {
                var recruiter = await _unitOfWork.Recruiters.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

                if (recruiter == null || application.Job.RecruiterId != recruiter.Id)
                    throw new ForbiddenException("You do not have permission to access this application.");
            }
            else if (rolesList.Contains("Candidate"))
            {
                var candidate = await _unitOfWork.Candidates.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (candidate == null || application.CandidateId != candidate.Id)
                    throw new ForbiddenException("You do not have permission to access this application.");
            }
            else
            {
                throw new ForbiddenException("You do not have permission to access this application.");
            }
        }

        return _mapper.Map<JobApplicationResponseDto>(application);
    }
}

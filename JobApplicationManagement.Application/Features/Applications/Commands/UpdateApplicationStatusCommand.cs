using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record UpdateApplicationStatusCommand(int Id, UpdateJobApplicationDto Dto) : IRequest<JobApplicationResponseDto>;

public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, JobApplicationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateApplicationStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<JobApplicationResponseDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.JobApplications.Query()
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (application is null)
            throw new NotFoundException(nameof(JobApplication), request.Id);

        var roles = _currentUserService.Roles.ToList();
        if (!roles.Contains("Admin"))
        {
            var userId = _currentUserService.UserId;
            var recruiter = await _unitOfWork.Recruiters.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

            if (recruiter is null || application.Job.RecruiterId != recruiter.Id)
            {
                throw new ForbiddenException("You do not have permission to update this application.");
            }
        }

        application.JobApplicationStatus = request.Dto.JobApplicationStatus;
        application.StatusUpdatedAt = DateTime.UtcNow;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<JobApplicationResponseDto>(application);
    }
}

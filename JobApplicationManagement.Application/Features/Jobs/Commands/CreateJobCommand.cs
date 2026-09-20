using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record CreateJobCommand(CreateJobDto JobDto) : IRequest<JobResponseDto>;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var createdByUserId = _currentUserService.UserId;
        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == createdByUserId, cancellationToken);

        if (recruiter is null)
        {
            throw new ForbiddenException("Only registered recruiters can create jobs.");
        }

        var job = _mapper.Map<Job>(request.JobDto);
        job.IsActive = true;
        job.RecruiterId = recruiter.Id;

        await _unitOfWork.Jobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<JobResponseDto>(job);
    }
}

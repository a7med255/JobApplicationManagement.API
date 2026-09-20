using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record UpdateJobCommand(int Id, UpdateJobDto JobDto) : IRequest<JobResponseDto>;

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(request.Id);
        if (job is null)
            throw new NotFoundException(nameof(Job), request.Id);

        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(_unitOfWork, job, _currentUserService, "update");

        _mapper.Map(request.JobDto, job);
        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<JobResponseDto>(job);
    }
}

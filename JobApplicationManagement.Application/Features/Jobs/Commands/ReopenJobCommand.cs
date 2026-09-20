using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record ReopenJobCommand(int Id) : IRequest<JobResponseDto>;

public class ReopenJobCommandHandler : IRequestHandler<ReopenJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ReopenJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(ReopenJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(request.Id);
        if (job is null)
            throw new NotFoundException(nameof(Job), request.Id);

        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(_unitOfWork, job, _currentUserService, "reopen");

        job.IsActive = true;
        job.ClosedAt = null;
        job.ClosedById = null;

        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<JobResponseDto>(job);
    }
}

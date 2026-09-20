using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record CloseJobCommand(int Id) : IRequest<CloseJobResponseDto>;

public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand, CloseJobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CloseJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CloseJobResponseDto> Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(request.Id);
        if (job is null)
            throw new NotFoundException(nameof(Job), request.Id);

        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(_unitOfWork, job, _currentUserService, "close");

        var userId = _currentUserService.UserId;
        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

        job.IsActive = false;
        job.ClosedAt = DateTime.UtcNow;
        job.ClosedById = recruiter?.Id;

        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CloseJobResponseDto>(job);
    }
}

using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Recruiters.Queries;

public record GetMyRecruiterProfileQuery : IRequest<RecruiterResponseDto>;

public class GetMyRecruiterProfileQueryHandler : IRequestHandler<GetMyRecruiterProfileQuery, RecruiterResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyRecruiterProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<RecruiterResponseDto> Handle(GetMyRecruiterProfileQuery request, CancellationToken cancellationToken)
    {
        var recruiter = await _unitOfWork.Recruiters.Query().AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == _currentUserService.UserId, cancellationToken);

        return recruiter is null
            ? throw new NotFoundException(nameof(Recruiter), _currentUserService.UserId ?? "current user")
            : RecruiterDtoFactory.ToResponse(recruiter);
    }
}

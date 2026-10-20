using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Recruiters.Queries;

public record GetRecruiterByIdQuery(int Id) : IRequest<RecruiterResponseDto>;

public class GetRecruiterByIdQueryHandler : IRequestHandler<GetRecruiterByIdQuery, RecruiterResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetRecruiterByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<RecruiterResponseDto> Handle(GetRecruiterByIdQuery request, CancellationToken cancellationToken)
    {
        var recruiter = await _unitOfWork.Recruiters.Query().AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recruiter is null)
            throw new NotFoundException(nameof(Recruiter), request.Id);

        var isAdmin = _currentUserService.Roles.Contains("Admin");
        if (!isAdmin && recruiter.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to view this recruiter profile.");

        return RecruiterDtoFactory.ToResponse(recruiter);
    }
}

using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Companies.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Companies.Queries;

public record GetMyCompanyQuery : IRequest<CompanyResponseDto>;

public class GetMyCompanyQueryHandler : IRequestHandler<GetMyCompanyQuery, CompanyResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyCompanyQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CompanyResponseDto> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Recruiters.Query().AsNoTracking()
            .Where(recruiter => recruiter.UserId == _currentUserService.UserId)
            .Select(recruiter => recruiter.Company)
            .FirstOrDefaultAsync(cancellationToken);

        return company is null
            ? throw new NotFoundException(nameof(Company), "current recruiter")
            : CompanyDtoFactory.ToResponse(company);
    }
}

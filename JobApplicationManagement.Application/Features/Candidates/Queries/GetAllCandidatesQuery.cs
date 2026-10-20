using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Candidates.Queries;

public record GetAllCandidatesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<CandidateResponseDto>>;

public class GetAllCandidatesQueryHandler : IRequestHandler<GetAllCandidatesQuery, PaginatedResult<CandidateResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCandidatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<CandidateResponseDto>> Handle(GetAllCandidatesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.Candidates.Query().AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CandidateResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CvUrl = c.CvUrl,
                UserId = c.UserId,
                ProfileImageUrl = c.ProfileImageUrl,
                ProfessionalTitle = c.ProfessionalTitle,
                Summary = c.Summary,
                Country = c.Country,
                City = c.City,
                YearsOfExperience = c.YearsOfExperience
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<CandidateResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

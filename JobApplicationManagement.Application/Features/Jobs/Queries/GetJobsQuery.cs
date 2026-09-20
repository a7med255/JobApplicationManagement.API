using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Queries;

public record GetJobsQuery(int PageNumber = 1, int PageSize = 10, string? Search = null) : IRequest<PaginatedResult<JobResponseDto>>;

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, PaginatedResult<JobResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetJobsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<JobResponseDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Jobs.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(j => j.Title.Contains(request.Search) || j.Description.Contains(request.Search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(j => j.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                IsActive = j.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<JobResponseDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

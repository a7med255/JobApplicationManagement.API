using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Recruiters.Queries;

public record GetAllRecruitersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<RecruiterResponseDto>>;

public class GetAllRecruitersQueryHandler : IRequestHandler<GetAllRecruitersQuery, PaginatedResult<RecruiterResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRecruitersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<RecruiterResponseDto>> Handle(GetAllRecruitersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.Recruiters.Query().AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecruiterResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                UserId = r.UserId
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<RecruiterResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

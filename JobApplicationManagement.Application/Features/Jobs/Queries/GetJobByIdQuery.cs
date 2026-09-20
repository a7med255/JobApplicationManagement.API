using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Queries;

public record GetJobByIdQuery(int Id) : IRequest<JobResponseDto>;

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetJobByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<JobResponseDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (job is null)
            throw new NotFoundException(nameof(Job), request.Id);

        return _mapper.Map<JobResponseDto>(job);
    }
}

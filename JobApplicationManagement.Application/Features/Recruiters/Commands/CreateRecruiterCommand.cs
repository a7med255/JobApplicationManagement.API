using AutoMapper;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Recruiters.Commands;

public record CreateRecruiterCommand(CreateRecruiterDto Dto) : IRequest<RecruiterResponseDto>;

public class CreateRecruiterCommandHandler : IRequestHandler<CreateRecruiterCommand, RecruiterResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRecruiterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RecruiterResponseDto> Handle(CreateRecruiterCommand request, CancellationToken cancellationToken)
    {
        var recruiter = _mapper.Map<Recruiter>(request.Dto);

        await _unitOfWork.Recruiters.AddAsync(recruiter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecruiterDtoFactory.ToResponse(recruiter);
    }
}

using AutoMapper;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Candidates.Commands;

public record CreateCandidateCommand(CreateCandidateDto Dto) : IRequest<CandidateResponseDto>;

public class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, CandidateResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCandidateCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CandidateResponseDto> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = _mapper.Map<Candidate>(request.Dto);

        await _unitOfWork.Candidates.AddAsync(candidate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CandidateDtoFactory.ToResponse(candidate);
    }
}

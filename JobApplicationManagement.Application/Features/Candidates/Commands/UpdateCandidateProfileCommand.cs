using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Candidates.Commands;

public record UpdateCandidateProfileCommand(int Id, UpdateCandidateDto Dto) : IRequest<CandidateResponseDto>;

public class UpdateCandidateProfileCommandHandler : IRequestHandler<UpdateCandidateProfileCommand, CandidateResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateCandidateProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CandidateResponseDto> Handle(UpdateCandidateProfileCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(request.Id);
        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), request.Id);

        var isAdmin = _currentUserService.Roles.Contains("Admin");
        if (!isAdmin && candidate.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to update this candidate profile.");

        _mapper.Map(request.Dto, candidate);
        _unitOfWork.Candidates.Update(candidate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CandidateDtoFactory.ToResponse(candidate);
    }
}

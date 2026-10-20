using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Recruiters.Commands;

public record UpdateRecruiterProfileCommand(int Id, UpdateRecruiterDto Dto) : IRequest<RecruiterResponseDto>;

public class UpdateRecruiterProfileCommandHandler : IRequestHandler<UpdateRecruiterProfileCommand, RecruiterResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateRecruiterProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<RecruiterResponseDto> Handle(UpdateRecruiterProfileCommand request, CancellationToken cancellationToken)
    {
        var recruiter = await _unitOfWork.Recruiters.GetByIdAsync(request.Id);
        if (recruiter is null)
            throw new NotFoundException(nameof(Recruiter), request.Id);

        var isAdmin = _currentUserService.Roles.Contains("Admin");
        if (!isAdmin && recruiter.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to update this recruiter profile.");

        _mapper.Map(request.Dto, recruiter);
        _unitOfWork.Recruiters.Update(recruiter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecruiterDtoFactory.ToResponse(recruiter);
    }
}

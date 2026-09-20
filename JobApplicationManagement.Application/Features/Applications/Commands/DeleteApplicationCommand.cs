using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using MediatR;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record DeleteApplicationCommand(int Id) : IRequest;

public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteApplicationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(request.Id);
        if (application is null)
            throw new NotFoundException("JobApplication", request.Id);

        _unitOfWork.JobApplications.Delete(application);
        await _unitOfWork.SaveChangesAsync();
    }
}

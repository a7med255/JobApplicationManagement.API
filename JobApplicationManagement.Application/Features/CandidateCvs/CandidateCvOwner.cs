using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.CandidateCvs;

internal static class CandidateCvOwner
{
    public static async Task<Candidate> GetCandidateAsync(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken) =>
        await unitOfWork.Candidates.Query()
            .FirstOrDefaultAsync(candidate => candidate.UserId == currentUserService.UserId, cancellationToken)
        ?? throw new ForbiddenException("Only registered candidates can manage CVs.");
}

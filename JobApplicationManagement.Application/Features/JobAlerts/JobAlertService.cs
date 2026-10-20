using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.JobAlerts;

public interface IJobAlertService
{
    Task<IReadOnlyCollection<JobAlertDto>> GetMineAsync(CancellationToken cancellationToken);
    Task<JobAlertDto> CreateAsync(SaveJobAlertDto request, CancellationToken cancellationToken);
    Task<JobAlertDto> UpdateAsync(int id, SaveJobAlertDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class JobAlertService : IJobAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public JobAlertService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);

    public async Task<IReadOnlyCollection<JobAlertDto>> GetMineAsync(CancellationToken cancellationToken)
    {
        var candidate = await CandidateAsync(cancellationToken);
        var values = await _unitOfWork.JobAlerts.Query().AsNoTracking()
            .Where(x => x.CandidateId == candidate.Id).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return values.Select(ToDto).ToArray();
    }

    public async Task<JobAlertDto> CreateAsync(SaveJobAlertDto request, CancellationToken cancellationToken)
    {
        Validate(request);
        var candidate = await CandidateAsync(cancellationToken);
        var entity = new JobAlert { CandidateId = candidate.Id, CreatedAt = DateTime.UtcNow };
        Apply(entity, request);
        await _unitOfWork.JobAlerts.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<JobAlertDto> UpdateAsync(int id, SaveJobAlertDto request, CancellationToken cancellationToken)
    {
        Validate(request);
        var candidate = await CandidateAsync(cancellationToken);
        var entity = await _unitOfWork.JobAlerts.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateId == candidate.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobAlert), id);
        Apply(entity, request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var candidate = await CandidateAsync(cancellationToken);
        var entity = await _unitOfWork.JobAlerts.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.CandidateId == candidate.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobAlert), id);
        _unitOfWork.JobAlerts.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Candidate> CandidateAsync(CancellationToken cancellationToken) =>
        await _unitOfWork.Candidates.Query().FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken)
        ?? throw new ForbiddenException("A candidate profile is required.");

    private static void Validate(SaveJobAlertDto request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 150) errors["Name"] = ["Name is required and cannot exceed 150 characters."];
        if (request.Keyword?.Length > 200) errors["Keyword"] = ["Keyword cannot exceed 200 characters."];
        if (request.Location?.Length > 200) errors["Location"] = ["Location cannot exceed 200 characters."];
        if (errors.Count > 0) throw new AppValidationException(errors);
    }

    private static void Apply(JobAlert entity, SaveJobAlertDto request)
    {
        entity.Name = request.Name.Trim();
        entity.Keyword = Normalize(request.Keyword);
        entity.Location = Normalize(request.Location);
        entity.EmploymentType = request.EmploymentType;
        entity.IsActive = request.IsActive;
    }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static JobAlertDto ToDto(JobAlert x) => new()
    {
        Id = x.Id, Name = x.Name, Keyword = x.Keyword, Location = x.Location,
        EmploymentType = x.EmploymentType, IsActive = x.IsActive, CreatedAt = x.CreatedAt
    };
}

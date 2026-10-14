using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Infrastructure.Persistence;
using JobApplicationManagement.Infrastructure.Repositories;

namespace JobApplicationManagement.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work implementation.
/// Lazily creates repositories and provides a single SaveChangesAsync.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IGenericRepository<Job>? _jobs;
    private IGenericRepository<JobApplication>? _jobApplications;
    private IGenericRepository<Candidate>? _candidates;
    private IGenericRepository<Recruiter>? _recruiters;
    private IGenericRepository<SavedJob>? _savedJobs;
    private IGenericRepository<Company>? _companies;
    private IGenericRepository<Skill>? _skills;
    private IGenericRepository<JobSkill>? _jobSkills;
    private IGenericRepository<CandidateCv>? _candidateCvs;

    public INotificationService NotificationService { get; }

    public UnitOfWork(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        NotificationService = notificationService;
    }

    /// <inheritdoc/>
    public IGenericRepository<Job> Jobs
        => _jobs ??= new GenericRepository<Job>(_context);

    /// <inheritdoc/>
    public IGenericRepository<JobApplication> JobApplications
        => _jobApplications ??= new GenericRepository<JobApplication>(_context);

    /// <inheritdoc/>
    public IGenericRepository<Candidate> Candidates
        => _candidates ??= new GenericRepository<Candidate>(_context);

    /// <inheritdoc/>
    public IGenericRepository<Recruiter> Recruiters
        => _recruiters ??= new GenericRepository<Recruiter>(_context);

    /// <inheritdoc/>
    public IGenericRepository<SavedJob> SavedJobs
        => _savedJobs ??= new GenericRepository<SavedJob>(_context);

    public IGenericRepository<Company> Companies
        => _companies ??= new GenericRepository<Company>(_context);

    public IGenericRepository<Skill> Skills
        => _skills ??= new GenericRepository<Skill>(_context);

    public IGenericRepository<JobSkill> JobSkills
        => _jobSkills ??= new GenericRepository<JobSkill>(_context);

    public IGenericRepository<CandidateCv> CandidateCvs
        => _candidateCvs ??= new GenericRepository<CandidateCv>(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}

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
    private IGenericRepository<ApplicationStatusHistory>? _applicationStatusHistory;
    private IGenericRepository<Notification>? _notifications;
    private IGenericRepository<Interview>? _interviews;
    private IGenericRepository<OutboxMessage>? _outboxMessages;
    private IGenericRepository<NotificationPreference>? _notificationPreferences;
    private IGenericRepository<JobAlert>? _jobAlerts;
    private IGenericRepository<AdminAuditLog>? _adminAuditLogs;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
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

    public IGenericRepository<ApplicationStatusHistory> ApplicationStatusHistory
        => _applicationStatusHistory ??= new GenericRepository<ApplicationStatusHistory>(_context);

    public IGenericRepository<Notification> Notifications
        => _notifications ??= new GenericRepository<Notification>(_context);

    public IGenericRepository<Interview> Interviews
        => _interviews ??= new GenericRepository<Interview>(_context);

    public IGenericRepository<OutboxMessage> OutboxMessages
        => _outboxMessages ??= new GenericRepository<OutboxMessage>(_context);

    public IGenericRepository<NotificationPreference> NotificationPreferences
        => _notificationPreferences ??= new GenericRepository<NotificationPreference>(_context);

    public IGenericRepository<JobAlert> JobAlerts
        => _jobAlerts ??= new GenericRepository<JobAlert>(_context);
    public IGenericRepository<AdminAuditLog> AdminAuditLogs
        => _adminAuditLogs ??= new GenericRepository<AdminAuditLog>(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public void Dispose()
        => _context.Dispose();
}

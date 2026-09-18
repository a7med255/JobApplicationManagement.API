using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Unit of Work contract.
/// Coordinates repositories and controls the database transaction boundary.
/// The Application layer calls SaveChangesAsync() through this interface —
/// never directly through a repository.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Job> Jobs { get; }

    IGenericRepository<JobApplication> JobApplications { get; }

    IGenericRepository<Candidate> Candidates { get; }

    IGenericRepository<Recruiter> Recruiters { get; }

    /// <summary>
    /// Persists all pending changes to the database within a single transaction.
    /// Returns the number of state entries written.
    /// </summary>
    Task<int> SaveChangesAsync();
}

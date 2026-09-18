using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Infrastructure.Identity;

namespace JobApplicationManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the JobApplicationManagement system.
/// Inherits from IdentityDbContext to support ASP.NET Core Identity tables.
/// Applies entity configurations via Fluent API (IEntityTypeConfiguration).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Recruiter> Recruiters => Set<Recruiter>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applies all IEntityTypeConfiguration<T> classes in this assembly automatically.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

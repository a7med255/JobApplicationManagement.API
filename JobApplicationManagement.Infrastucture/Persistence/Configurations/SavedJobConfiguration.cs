using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the SavedJob entity.
/// Configures FK relationships and the unique constraint on CandidateId + JobId.
/// </summary>
public class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
{
    public void Configure(EntityTypeBuilder<SavedJob> builder)
    {
        builder.HasKey(sj => sj.Id);

        builder.HasOne(sj => sj.Candidate)
            .WithMany()
            .HasForeignKey(sj => sj.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sj => sj.Job)
            .WithMany()
            .HasForeignKey(sj => sj.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent a candidate from saving the same job more than once
        builder.HasIndex(sj => new { sj.CandidateId, sj.JobId })
            .IsUnique();

        builder.Property(sj => sj.CreatedAt)
            .IsRequired();
    }
}

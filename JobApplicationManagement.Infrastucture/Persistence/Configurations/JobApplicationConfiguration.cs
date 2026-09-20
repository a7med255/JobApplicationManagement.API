using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the JobApplication entity.
/// Configures FK relationships and the new CancelledAt field.
/// </summary>
public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasKey(ja => ja.Id);

        builder.HasOne(ja => ja.Candidate)
            .WithMany()
            .HasForeignKey(ja => ja.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ja => ja.Job)
            .WithMany()
            .HasForeignKey(ja => ja.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate applications for the same job by the same candidate
        builder.HasIndex(ja => new { ja.CandidateId, ja.JobId })
            .IsUnique();

        builder.Property(ja => ja.JobApplicationStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(ja => ja.AppliedAt)
            .IsRequired();

        builder.Property(ja => ja.StatusUpdatedAt)
            .IsRequired();

        builder.Property(ja => ja.CancelledAt);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the Job entity.
/// </summary>
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(j => j.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(j => j.Recruiter)
            .WithMany(r => r.Jobs)
            .HasForeignKey(j => j.RecruiterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.ClosedBy)
            .WithMany()
            .HasForeignKey(j => j.ClosedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(j => j.ClosedAt);
    }
}

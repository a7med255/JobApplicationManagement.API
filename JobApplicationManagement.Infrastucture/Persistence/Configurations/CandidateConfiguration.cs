using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the Candidate entity.
/// </summary>
public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CvUrl)
            .HasMaxLength(2000);

        builder.Property(c => c.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(c => c.UserId)
            .IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the Recruiter entity.
/// </summary>
public class RecruiterConfiguration : IEntityTypeConfiguration<Recruiter>
{
    public void Configure(EntityTypeBuilder<Recruiter> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(r => r.UserId)
            .IsUnique();
    }
}

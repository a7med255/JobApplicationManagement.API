using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public sealed class JobAlertConfiguration : IEntityTypeConfiguration<JobAlert>
{
    public void Configure(EntityTypeBuilder<JobAlert> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Keyword).HasMaxLength(200);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.EmploymentType).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.Candidate).WithMany(x => x.JobAlerts)
            .HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CandidateId, x.IsActive });
    }
}

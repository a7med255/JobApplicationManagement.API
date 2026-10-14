using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Ignore(j => j.IsActive);

        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Description).IsRequired().HasMaxLength(20000);
        builder.Property(j => j.Department).IsRequired().HasMaxLength(150);
        builder.Property(j => j.Category).IsRequired().HasMaxLength(150);
        builder.Property(j => j.Country).IsRequired().HasMaxLength(100);
        builder.Property(j => j.City).IsRequired().HasMaxLength(100);
        builder.Property(j => j.Location).HasMaxLength(300);
        builder.Property(j => j.Responsibilities).IsRequired().HasMaxLength(10000);
        builder.Property(j => j.Requirements).IsRequired().HasMaxLength(10000);
        builder.Property(j => j.PreferredQualifications).HasMaxLength(10000);
        builder.Property(j => j.EducationRequirements).HasMaxLength(5000);
        builder.Property(j => j.Benefits).HasMaxLength(10000);
        builder.Property(j => j.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(j => j.MinimumSalary).HasPrecision(18, 2);
        builder.Property(j => j.MaximumSalary).HasPrecision(18, 2);

        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(j => j.WorkplaceType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(j => j.EmploymentType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(j => j.ExperienceLevel).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(j => j.SalaryPeriod).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(j => j.Recruiter)
            .WithMany(r => r.Jobs)
            .HasForeignKey(j => j.RecruiterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.ClosedBy)
            .WithMany()
            .HasForeignKey(j => j.ClosedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.PostedAt);
        builder.HasIndex(j => j.ApplicationDeadline);
        builder.HasIndex(j => new { j.Country, j.City });
        builder.HasIndex(j => new { j.CompanyId, j.Status });
        builder.HasIndex(j => new { j.RecruiterId, j.Status });
    }
}

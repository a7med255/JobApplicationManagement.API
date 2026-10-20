using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public class CandidateCvConfiguration : IEntityTypeConfiguration<CandidateCv>
{
    public void Configure(EntityTypeBuilder<CandidateCv> builder)
    {
        builder.HasKey(cv => cv.Id);
        builder.Property(cv => cv.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(cv => cv.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(cv => cv.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(cv => cv.Content);
        builder.Property(cv => cv.StorageKey).HasMaxLength(300);
        builder.Property(cv => cv.StorageProvider).IsRequired().HasMaxLength(50);
        builder.Property(cv => cv.ResourceType).IsRequired().HasMaxLength(30);
        builder.Property(cv => cv.DeliveryType).IsRequired().HasMaxLength(30);
        builder.Property(cv => cv.ChecksumSha256).HasMaxLength(64);
        builder.HasOne(cv => cv.Candidate)
            .WithMany(c => c.Cvs)
            .HasForeignKey(cv => cv.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(cv => new { cv.CandidateId, cv.IsDeleted });
        builder.HasIndex(cv => new { cv.CandidateId, cv.IsDefault })
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0")
            .IsUnique();
        builder.HasIndex(cv => cv.StorageKey);
    }
}

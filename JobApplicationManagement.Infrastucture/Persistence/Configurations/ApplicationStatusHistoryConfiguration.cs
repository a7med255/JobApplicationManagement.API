using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public sealed class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OldStatus).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.ChangedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.HasOne(x => x.Application).WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ApplicationId, x.ChangedAt });
    }
}

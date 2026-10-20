using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public sealed class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdminUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TargetType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TargetId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.HasIndex(x => new { x.CreatedAt, x.AdminUserId });
    }
}

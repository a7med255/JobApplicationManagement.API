using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public sealed class AccountOneTimeCodeConfiguration : IEntityTypeConfiguration<AccountOneTimeCode>
{
    public void Configure(EntityTypeBuilder<AccountOneTimeCode> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CodeHash).HasMaxLength(88).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.Purpose, x.ExpiresAt });
    }
}

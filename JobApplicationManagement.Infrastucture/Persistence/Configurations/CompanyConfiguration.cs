using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.LogoUrl).HasMaxLength(2000);
        builder.Property(c => c.Description).HasMaxLength(5000);
        builder.Property(c => c.Website).HasMaxLength(2000);
        builder.Property(c => c.Industry).HasMaxLength(150);
        builder.Property(c => c.CompanySize).HasMaxLength(100);
        builder.Property(c => c.Headquarters).HasMaxLength(300);
        builder.Property(c => c.Country).HasMaxLength(100);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.HasIndex(c => c.Name);
    }
}

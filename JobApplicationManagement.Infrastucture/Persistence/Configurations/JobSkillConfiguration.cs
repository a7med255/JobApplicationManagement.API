using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManagement.Infrastructure.Persistence.Configurations;

public class JobSkillConfiguration : IEntityTypeConfiguration<JobSkill>
{
    public void Configure(EntityTypeBuilder<JobSkill> builder)
    {
        builder.HasKey(js => new { js.JobId, js.SkillId });
        builder.HasOne(js => js.Job).WithMany(j => j.JobSkills).HasForeignKey(js => js.JobId);
        builder.HasOne(js => js.Skill).WithMany(s => s.JobSkills).HasForeignKey(js => js.SkillId);
        builder.HasIndex(js => new { js.SkillId, js.IsRequired });
    }
}

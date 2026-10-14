namespace JobApplicationManagement.Domain.Entities;

public class JobSkill
{
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public bool IsRequired { get; set; }
}

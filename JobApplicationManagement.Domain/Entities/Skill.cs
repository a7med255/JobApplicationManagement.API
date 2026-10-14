namespace JobApplicationManagement.Domain.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}

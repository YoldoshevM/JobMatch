namespace JobMatch.Models;

public class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<VacancyRequirement> VacancyRequirements { get; set; }
        = new List<VacancyRequirement>();

    public ICollection<UserSkill> UserSkills { get; set; }
        = new List<UserSkill>();
}
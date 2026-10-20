namespace JobMatch.Models;

public class VacancyRequirement
{
    public int Id { get; set; }

    public int VacancyId { get; set; }

    public int SkillId { get; set; }

    public int Weight { get; set; }

    public bool IsRequired { get; set; }

    public Vacancy? Vacancy { get; set; }

    public Skill? Skill { get; set; }
}

namespace JobMatch.DTOs;

public class CreateVacancyRequirementDto
{
    // Qaysi vakansiyaga skill biriktiramiz?
    public int VacancyId { get; set; }

    // Qaysi skill'ni biriktiramiz?
    public int SkillId { get; set; }

    // Skill'ning muhimligi: 1 dan 5 gacha.
    public int Weight { get; set; }

    // Skill majburiymi?
    public bool IsRequired { get; set; }
}

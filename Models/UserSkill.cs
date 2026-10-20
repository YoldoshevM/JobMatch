namespace JobMatch.Models;

public class UserSkill
{
    public int Id { get; set; }

    public int ResumeId { get; set; }

    public int SkillId { get; set; }

    public string? Level { get; set; }

    public Resume? Resume { get; set; }

    public Skill? Skill { get; set; }
}
namespace JobMatch.Models;

public class Resume
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public ICollection<UserSkill> Skills { get; set; }
        = new List<UserSkill>();
}
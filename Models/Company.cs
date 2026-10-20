namespace JobMatch.Models;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
namespace JobMatch.Models;

public class Vacancy
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Location { get; set; }

    public decimal? SalaryFrom { get; set; }

    public decimal? SalaryTo { get; set; }

    public string ApplicationUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company? Company { get; set; }

    public ICollection<VacancyRequirement> Requirements { get; set; }
        = new List<VacancyRequirement>();
}
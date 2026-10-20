
namespace JobMatch.DTOs;

public class UpdateVacancyDto
{
    public int CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Location { get; set; }

    public decimal? SalaryFrom { get; set; }

    public decimal? SalaryTo { get; set; }

    public string ApplicationUrl { get; set; } = string.Empty;
}

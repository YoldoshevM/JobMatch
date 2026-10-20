
namespace JobMatch.DTOs;

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }
}

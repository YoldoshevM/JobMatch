
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Kompaniyalar jadvali
    public DbSet<Company> Companies { get; set; }

    // Vakansiyalar jadvali
    public DbSet<Vacancy> Vacancies { get; set; }

    // Skilllar jadvali
    public DbSet<Skill> Skills { get; set; }

    // Vakansiya talablari jadvali
    public DbSet<VacancyRequirement> VacancyRequirements { get; set; }

    // Foydalanuvchilarning CV ma'lumotlari
    public DbSet<Resume> Resumes { get; set; }

    // CV ichidagi skilllar
    public DbSet<UserSkill> UserSkills { get; set; }
}

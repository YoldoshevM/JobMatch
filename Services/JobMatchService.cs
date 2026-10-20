
using JobMatch.Data;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Services;

public class JobMatchService
{
    private readonly AppDbContext _context;
    private readonly ResumeService _resumeService;

    // Database va CV skill'larini aniqlovchi Service.
    public JobMatchService(
        AppDbContext context,
        ResumeService resumeService)
    {
        _context = context;
        _resumeService = resumeService;
    }

    // CV va vakansiya mosligini hisoblash.
    public async Task<object?> CalculateMatchAsync(
        int resumeId,
        int vacancyId)
    {
        // 1. Resume va vakansiyani bazadan topamiz.
        var resume = await _context.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resumeId);

        var vacancy = await _context.Vacancies
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vacancyId);

        if (resume == null || vacancy == null)
        {
            return null;
        }

        // 2. CV'dan aniqlangan skill'larni olamiz.
        var cvSkills = await _resumeService.FindSkillsInCvAsync(resumeId);

        // 3. Vakansiya talablarini yuklaymiz.
        var requirements = await _context.VacancyRequirements
            .AsNoTracking()
            .Include(r => r.Skill)
            .Where(r => r.VacancyId == vacancyId)
            .ToListAsync();

        if (requirements.Count == 0)
        {
            return new
            {
                message = "This vacancy has no skill requirements."
            };
        }

        // 4. Hisoblash uchun o'zgaruvchilar.
        int totalWeight = 0;
        int matchedWeight = 0;

        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();
        var missingRequiredSkills = new List<string>();
        var missingOptionalSkills = new List<string>();

        // 5. Talablarni CV bilan solishtiramiz.
        foreach (var requirement in requirements)
        {
            if (requirement.Skill == null)
            {
                continue;
            }

            string skillName = requirement.Skill.Name;

            totalWeight += requirement.Weight;

            bool hasSkill = cvSkills.Any(s =>
                s.Equals(
                    skillName,
                    StringComparison.OrdinalIgnoreCase));

            if (hasSkill)
            {
                matchedWeight += requirement.Weight;
                matchedSkills.Add(skillName);
            }
            else
            {
                missingSkills.Add(skillName);

                if (requirement.IsRequired)
                {
                    missingRequiredSkills.Add(skillName);
                }
                else
                {
                    missingOptionalSkills.Add(skillName);
                }
            }
        }

        // 6. Moslik foizini hisoblaymiz.
        double matchPercentage = 0;

        if (totalWeight > 0)
        {
            matchPercentage =
                (double)matchedWeight / totalWeight * 100;
        }

        // 7. Yetishmayotgan talablarni ustuvorlik bo'yicha tartiblaymiz.
        var prioritizedMissing = requirements
            .Where(r =>
                r.Skill != null &&
                missingSkills.Contains(r.Skill.Name))
            .OrderByDescending(r => r.IsRequired)
            .ThenByDescending(r => r.Weight)
            .ToList();

        // 8. Avtomatik tavsiyalar yaratamiz.
        var recommendations = new List<string>();

        foreach (var requirement in prioritizedMissing)
        {
            string skillName = requirement.Skill!.Name;

            if (requirement.IsRequired)
            {
                recommendations.Add(
                    $"High priority: Learn {skillName}. " +
                    $"It is required for this vacancy (weight: {requirement.Weight}).");
            }
            else
            {
                recommendations.Add(
                    $"Consider learning {skillName} to improve your match score.");
            }
        }

        // Hamma talablar bajarilgan bo'lsa.
        if (missingSkills.Count == 0)
        {
            recommendations.Add(
                "Great match! Your CV contains all the listed skills for this vacancy.");
        }

        // 9. Natijani Controller'ga qaytaramiz.
        return new
        {
            resumeId,
            vacancyId,
            vacancyTitle = vacancy.Title,

            matchPercentage = Math.Round(matchPercentage, 1),

            matchedSkills,
            missingSkills,
            missingRequiredSkills,
            missingOptionalSkills,
            recommendations
        };
    }
}

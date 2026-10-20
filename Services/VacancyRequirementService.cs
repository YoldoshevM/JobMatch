
using JobMatch.Data;
using JobMatch.DTOs;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Services;

public class VacancyRequirementService
{
    private readonly AppDbContext _context;

    // Database bilan ishlash uchun AppDbContext olamiz.
    public VacancyRequirementService(AppDbContext context)
    {
        _context = context;
    }

    // Barcha vakansiya talablarini olish.
    public async Task<List<VacancyRequirement>> GetAllAsync()
    {
        return await _context.VacancyRequirements
            .AsNoTracking()
            .ToListAsync();
    }

    // Bitta vakansiyaga tegishli skill talablarini olish.
    public async Task<List<VacancyRequirement>> GetByVacancyIdAsync(int vacancyId)
    {
        return await _context.VacancyRequirements
            .AsNoTracking()
            .Where(r => r.VacancyId == vacancyId)
            .ToListAsync();
    }

    // Vakansiyaga yangi skill talabini qo'shish.
    public async Task<VacancyRequirement?> CreateAsync(
        CreateVacancyRequirementDto dto)
    {
        // 1. Muhimlik darajasi 1 dan 5 gacha bo'lishi kerak.
        if (dto.Weight < 1 || dto.Weight > 5)
        {
            return null;
        }

        // 2. Vakansiya mavjudligini tekshiramiz.
        bool vacancyExists = await _context.Vacancies
            .AnyAsync(v => v.Id == dto.VacancyId);

        if (!vacancyExists)
        {
            return null;
        }

        // 3. Skill mavjudligini tekshiramiz.
        bool skillExists = await _context.Skills
            .AnyAsync(s => s.Id == dto.SkillId);

        if (!skillExists)
        {
            return null;
        }

        // 4. Bu skill oldin shu vakansiyaga qo'shilganmi?
        bool alreadyExists = await _context.VacancyRequirements
            .AnyAsync(r =>
                r.VacancyId == dto.VacancyId &&
                r.SkillId == dto.SkillId);

        if (alreadyExists)
        {
            return null;
        }

        // 5. Yangi talab obyektini yaratamiz.
        var requirement = new VacancyRequirement
        {
            VacancyId = dto.VacancyId,
            SkillId = dto.SkillId,
            Weight = dto.Weight,
            IsRequired = dto.IsRequired
        };

        // 6. Bazaga qo'shamiz.
        _context.VacancyRequirements.Add(requirement);

        // 7. PostgreSQL'ga saqlaymiz.
        await _context.SaveChangesAsync();

        return requirement;
    }

    // ID orqali vakansiyaga biriktirilgan skill talabini o'chirish.
    public async Task<bool> DeleteAsync(int id)
    {
        // 1. O'chirilishi kerak bo'lgan talabni topamiz.
        var requirement = await _context.VacancyRequirements
            .FindAsync(id);

        // 2. Talab mavjud bo'lmasa false qaytaramiz.
        if (requirement == null)
        {
            return false;
        }

        // 3. Talabni o'chirishga belgilaymiz.
        _context.VacancyRequirements.Remove(requirement);

        // 4. O'zgarishni PostgreSQL'ga saqlaymiz.
        await _context.SaveChangesAsync();

        // 5. O'chirish muvaffaqiyatli bajarildi.
        return true;
    }
}

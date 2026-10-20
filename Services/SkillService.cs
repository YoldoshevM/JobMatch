
using JobMatch.Data;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Services;

public class SkillService
{
    private readonly AppDbContext _context;

    public SkillService(AppDbContext context)
    {
        _context = context;
    }

    // Barcha ko'nikmalarni olish
    public async Task<List<Skill>> GetAllAsync()
    {
        return await _context.Skills
            .AsNoTracking()
            .ToListAsync();
    }

    // ID orqali ko'nikmani olish
    public async Task<Skill?> GetByIdAsync(int id)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // Yangi ko'nikma yaratish
    public async Task<Skill> CreateAsync(string name)
    {
        var skill = new Skill
        {
            Name = name.Trim()
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return skill;
    }

    // Ko'nikma nomini yangilash
    public async Task<Skill?> UpdateAsync(int id, string name)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill == null)
        {
            return null;
        }

        skill.Name = name.Trim();

        await _context.SaveChangesAsync();

        return skill;
    }

    // Ko'nikmani o'chirish
    public async Task<bool> DeleteAsync(int id)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill == null)
        {
            return false;
        }

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();

        return true;
    }
}

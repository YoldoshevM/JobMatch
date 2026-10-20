
using JobMatch.Data;
using JobMatch.DTOs;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Services;

public class VacancyService
{
    private readonly AppDbContext _context;

    public VacancyService(AppDbContext context)
    {
        _context = context;
    }


    // Barcha vakansiyalarni kompaniya va skill talablari bilan olish
    public async Task<List<Vacancy>> GetAllAsync()
    {
        return await _context.Vacancies
            .Include(v => v.Company)
            .Include(v => v.Requirements)
                .ThenInclude(r => r.Skill)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    // Bitta vakansiyani barcha talablari bilan olish
    public async Task<Vacancy?> GetByIdAsync(int id)
    {
        return await _context.Vacancies
            .Include(v => v.Company)
            .Include(v => v.Requirements)
                .ThenInclude(r => r.Skill)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == id);
    }


    // Yangi vakansiya yaratish
    public async Task<Vacancy> CreateAsync(CreateVacancyDto dto)
    {
        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == dto.CompanyId);

        if (!companyExists)
        {
            throw new ArgumentException("Company not found.");
        }


        // Bir kompaniyaning aynan bir xil e'lonini
        // qayta qo'shishni oldini olamiz.
        var duplicateExists = await _context.Vacancies
            .AnyAsync(v =>
                v.CompanyId == dto.CompanyId &&
                v.Title == dto.Title &&
                v.ApplicationUrl == dto.ApplicationUrl);

        if (duplicateExists)
        {
            throw new ArgumentException(
                "This vacancy already exists in the database.");
        }


        var vacancy = new Vacancy
        {
            CompanyId = dto.CompanyId,
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            SalaryFrom = dto.SalaryFrom,
            SalaryTo = dto.SalaryTo,
            ApplicationUrl = dto.ApplicationUrl
        };

        _context.Vacancies.Add(vacancy);

        await _context.SaveChangesAsync();

        return await _context.Vacancies
            .Include(v => v.Company)
            .FirstAsync(v => v.Id == vacancy.Id);
    }

    // Vakansiyani yangilash
    public async Task<Vacancy?> UpdateAsync(
        int id,
        UpdateVacancyDto dto)
    {
        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
        {
            return null;
        }

        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == dto.CompanyId);

        if (!companyExists)
        {
            throw new ArgumentException("Company not found.");
        }

        vacancy.CompanyId = dto.CompanyId;
        vacancy.Title = dto.Title;
        vacancy.Description = dto.Description;
        vacancy.Location = dto.Location;
        vacancy.SalaryFrom = dto.SalaryFrom;
        vacancy.SalaryTo = dto.SalaryTo;
        vacancy.ApplicationUrl = dto.ApplicationUrl;

        await _context.SaveChangesAsync();

        return await _context.Vacancies
            .Include(v => v.Company)
            .AsNoTracking()
            .FirstAsync(v => v.Id == id);
    }

    // Vakansiyani o'chirish
    public async Task<bool> DeleteAsync(int id)
    {
        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
        {
            return false;
        }

        _context.Vacancies.Remove(vacancy);

        await _context.SaveChangesAsync();

        return true;
    }
}

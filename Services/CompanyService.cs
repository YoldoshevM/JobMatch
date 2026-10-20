
using JobMatch.Data;
using JobMatch.DTOs;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Services;

public class CompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    // Barcha kompaniyalarni olish
    public async Task<List<Company>> GetAllAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .ToListAsync();
    }

    // ID orqali kompaniyani topish
    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Yangi kompaniya yaratish
    public async Task<Company> CreateAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name,
            Description = dto.Description,
            WebsiteUrl = dto.WebsiteUrl
        };

        _context.Companies.Add(company);

        await _context.SaveChangesAsync();

        return company;
    }

    // Kompaniya ma'lumotlarini yangilash
    public async Task<Company?> UpdateAsync(
        int id,
        UpdateCompanyDto dto)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return null;
        }

        company.Name = dto.Name;
        company.Description = dto.Description;
        company.WebsiteUrl = dto.WebsiteUrl;

        await _context.SaveChangesAsync();

        return company;
    }

    // Kompaniyani o'chirish
    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return false;
        }

        _context.Companies.Remove(company);

        await _context.SaveChangesAsync();

        return true;
    }
}

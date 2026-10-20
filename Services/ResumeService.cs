
using JobMatch.Data;
using JobMatch.DTOs;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using System.IO;
using System.Text.RegularExpressions;

namespace JobMatch.Services;

public class ResumeService
{
    private readonly AppDbContext _context;

    // Database bilan ishlash uchun AppDbContext olamiz.
    public ResumeService(AppDbContext context)
    {
        _context = context;
    }

    // Barcha Resume'larni olish.
    public async Task<List<Resume>> GetAllAsync()
    {
        return await _context.Resumes
            .AsNoTracking()
            .ToListAsync();
    }

    // ID orqali Resume topish.
    public async Task<Resume?> GetByIdAsync(int id)
    {
        return await _context.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    // Yangi Resume yaratish.
    public async Task<Resume> CreateAsync(CreateResumeDto dto)
    {
        var resume = new Resume
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim()
        };

        _context.Resumes.Add(resume);

        await _context.SaveChangesAsync();

        return resume;
    }

    // Resume ma'lumotlarini yangilash.
    public async Task<Resume?> UpdateAsync(int id, UpdateResumeDto dto)
    {
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resume == null)
        {
            return null;
        }

        resume.UserName = dto.UserName.Trim();
        resume.Email = dto.Email.Trim();

        await _context.SaveChangesAsync();

        return resume;
    }

    // Resume'ni o'chirish.
    public async Task<bool> DeleteAsync(int id)
    {
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resume == null)
        {
            return false;
        }

        _context.Resumes.Remove(resume);

        await _context.SaveChangesAsync();

        return true;
    }

    // Resume'ga yuklangan CV faylining nomini saqlash.
    public async Task<Resume?> SetFileNameAsync(int id, string fileName)
    {
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resume == null)
        {
            return null;
        }

        resume.FileName = fileName;

        await _context.SaveChangesAsync();

        return resume;
    }

    // CV ichidagi IT skill'larni avtomatik aniqlash.
    public async Task<List<string>> FindSkillsInCvAsync(int id)
    {
        // 1. Resume'ni bazadan topamiz.
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == id);

        // Resume yoki CV mavjud bo'lmasa, bo'sh ro'yxat qaytaramiz.
        if (resume == null || string.IsNullOrEmpty(resume.FileName))
        {
            return new List<string>();
        }

        // 2. CV faylining manzilini topamiz.
        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "uploads",
            resume.FileName);

        // Fayl kompyuterda mavjudligini tekshiramiz.
        if (!File.Exists(filePath))
        {
            return new List<string>();
        }

        // 3. PDF faylni ochamiz.
        using var document = PdfDocument.Open(filePath);

        string cvText = "";

        // PDF'ning barcha sahifalaridan matnni olamiz.
        foreach (var page in document.GetPages())
        {
            cvText += page.Text + " ";
        }

        // 4. PostgreSQL bazasidagi barcha skill nomlarini olamiz.
        var skills = await _context.Skills
            .Select(s => s.Name)
            .ToListAsync();

        // 5. Topilgan skill'larni saqlash uchun ro'yxat yaratamiz.
        var foundSkills = new List<string>();

        // 6. Har bir skill'ni CV matnidan qidiramiz.
        foreach (var skill in skills)
        {
            // Skill nomini alohida so'z sifatida qidiramiz.
            string pattern = @"(?<![\w])" +
                             Regex.Escape(skill) +
                             @"(?![\w])";

            // CV ichida skill mavjudligini tekshiramiz.
            bool found = Regex.IsMatch(
                cvText,
                pattern,
                RegexOptions.IgnoreCase);

            // Skill topilsa, ro'yxatga qo'shamiz.
            if (found)
            {
                foundSkills.Add(skill);
            }
        }

        // 7. Topilgan barcha skill'larni qaytaramiz.
        return foundSkills;
    }
}

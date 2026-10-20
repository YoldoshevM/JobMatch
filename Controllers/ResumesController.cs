
using JobMatch.DTOs;
using JobMatch.Services;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;

namespace JobMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumesController : ControllerBase
{
    private readonly ResumeService _resumeService;

    public ResumesController(ResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    // GET: api/Resumes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resumes = await _resumeService.GetAllAsync();

        return Ok(resumes);
    }

    // GET: api/Resumes/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var resume = await _resumeService.GetByIdAsync(id);

        if (resume == null)
        {
            return NotFound(new
            {
                message = "Resume not found."
            });
        }

        return Ok(resume);
    }

    // POST: api/Resumes
    [HttpPost]
    public async Task<IActionResult> Create(CreateResumeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName) ||
            string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new
            {
                message = "Name and email are required."
            });
        }

        if (!System.Net.Mail.MailAddress.TryCreate(dto.Email, out _))
        {
            return BadRequest(new
            {
                message = "Email address is invalid."
            });
        }

        var resume = await _resumeService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = resume.Id },
            resume);
    }

    // PUT: api/Resumes/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateResumeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName) ||
            string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new
            {
                message = "Name and email are required."
            });
        }

        if (!System.Net.Mail.MailAddress.TryCreate(dto.Email, out _))
        {
            return BadRequest(new
            {
                message = "Email address is invalid."
            });
        }

        var resume = await _resumeService.UpdateAsync(id, dto);

        if (resume == null)
        {
            return NotFound(new
            {
                message = "Resume not found."
            });
        }

        return Ok(resume);
    }

    // DELETE: api/Resumes/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _resumeService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Resume not found."
            });
        }

        return NoContent();
    }



    // POST: api/Resumes/1/upload-cv
    // Resume uchun PDF yoki DOCX CV faylini yuklaydi.
    [HttpPost("{id:int}/upload-cv")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCv(int id, IFormFile file)
    {
        // Fayl yuborilganini tekshiramiz.
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Please select a CV file."
            });
        }

        // Faqat PDF va DOCX formatlariga ruxsat beramiz.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension != ".pdf" && extension != ".docx")
        {
            return BadRequest(new
            {
                message = "Only PDF and DOCX files are allowed."
            });
        }

        // Fayl hajmini 5 MB bilan cheklaymiz.
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new
            {
                message = "File size must not exceed 5 MB."
            });
        }

        // Resume mavjudligini tekshiramiz.
        var resume = await _resumeService.GetByIdAsync(id);

        if (resume == null)
        {
            return NotFound(new
            {
                message = "Resume not found."
            });
        }

        // Fayl nomini serverning o'zi yaratadi.
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "uploads");

        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Faylni kompyuterimizdagi uploads papkasiga saqlaymiz.
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Resume ichida saqlangan fayl nomini yangilaymiz.
        var updatedResume = await _resumeService.SetFileNameAsync(
            id, uniqueFileName);

        return Ok(new
        {
            message = "CV uploaded successfully.",
            resumeId = id,
            fileName = uniqueFileName
        });
    }



    // Resume ichidagi PDF matnini o'qiydi.
    [HttpGet("{id:int}/read-cv")]
    public async Task<IActionResult> ReadCv(int id)
    {
        // Resume bazada mavjudligini tekshiramiz.
        var resume = await _resumeService.GetByIdAsync(id);

        if (resume == null)
        {
            return NotFound(new
            {
                message = "Resume not found."
            });
        }

        // Resume uchun CV yuklanganmi?
        if (string.IsNullOrEmpty(resume.FileName))
        {
            return BadRequest(new
            {
                message = "This resume has no uploaded CV."
            });
        }

        // Yuklangan PDF faylining manzilini topamiz.
        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "uploads",
            resume.FileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new
            {
                message = "CV file not found."
            });
        }

        // PDF ichidagi matnni o'qib olamiz.
        using var document = PdfDocument.Open(filePath);

        string text = "";

        foreach (var page in document.GetPages())
        {
            text += page.Text + "\n";
        }

        // O'qilgan matnni javob sifatida qaytaramiz.
        return Ok(new
        {
            resumeId = id,
            cvText = text
        });
    }



    // CV ichidan topilgan skill'larni qaytaradi.
    [HttpGet("{id:int}/find-skills")]
    public async Task<IActionResult> FindSkills(int id)
    {
        var skills = await _resumeService.FindSkillsInCvAsync(id);

        return Ok(new
        {
            resumeId = id,
            foundSkills = skills
        });
    }



}

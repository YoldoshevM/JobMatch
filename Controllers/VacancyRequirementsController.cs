
using JobMatch.DTOs;
using JobMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacancyRequirementsController : ControllerBase
{
    private readonly VacancyRequirementService _service;

    // Service'ni Controller ichida ishlatish uchun olamiz.
    public VacancyRequirementsController(VacancyRequirementService service)
    {
        _service = service;
    }

    // GET: api/VacancyRequirements
    // Barcha vakansiyalarning skill talablarini oladi.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requirements = await _service.GetAllAsync();

        return Ok(requirements);
    }

    // GET: api/VacancyRequirements/vacancy/1
    // Faqat bitta vakansiyaga tegishli talablarni oladi.
    [HttpGet("vacancy/{vacancyId:int}")]
    public async Task<IActionResult> GetByVacancyId(int vacancyId)
    {
        var requirements = await _service.GetByVacancyIdAsync(vacancyId);

        return Ok(requirements);
    }

    // POST: api/VacancyRequirements
    // Vakansiyaga yangi skill talabini biriktiradi.
    [HttpPost]
    public async Task<IActionResult> Create(CreateVacancyRequirementDto dto)
    {
        // Muhimlik darajasini tekshiramiz.
        if (dto.Weight < 1 || dto.Weight > 5)
        {
            return BadRequest(new
            {
                message = "Weight must be between 1 and 5."
            });
        }

        // Service orqali yangi talab yaratamiz.
        var requirement = await _service.CreateAsync(dto);

        // Service null qaytarsa, talab qo'shilmadi.
        if (requirement == null)
        {
            return BadRequest(new
            {
                message = "Invalid vacancy, skill, or duplicate requirement."
            });
        }

        // Yangi talab muvaffaqiyatli yaratildi.
        return StatusCode(201, requirement);
    }


    // DELETE: api/VacancyRequirements/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Service orqali talabni o'chiramiz.
        bool deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Requirement not found."
            });
        }

        return NoContent();
    }



}

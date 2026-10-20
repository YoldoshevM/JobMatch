
using JobMatch.DTOs;
using JobMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacanciesController : ControllerBase
{
    private readonly VacancyService _vacancyService;

    public VacanciesController(VacancyService vacancyService)
    {
        _vacancyService = vacancyService;
    }

    // GET: api/Vacancies
    // Barcha vakansiyalarni olish
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vacancies = await _vacancyService.GetAllAsync();

        return Ok(vacancies);
    }

    // GET: api/Vacancies/1
    // ID orqali bitta vakansiyani olish
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vacancy = await _vacancyService.GetByIdAsync(id);

        if (vacancy == null)
        {
            return NotFound(new
            {
                message = "Vacancy not found."
            });
        }

        return Ok(vacancy);
    }

    // POST: api/Vacancies
    // Yangi vakansiya yaratish
    [HttpPost]
    public async Task<IActionResult> Create(CreateVacancyDto dto)
    {
        if (dto.CompanyId <= 0 ||
            string.IsNullOrWhiteSpace(dto.Title) ||
            string.IsNullOrWhiteSpace(dto.Description) ||
            string.IsNullOrWhiteSpace(dto.ApplicationUrl))
        {
            return BadRequest(new
            {
                message = "CompanyId, title, description and application URL are required."
            });
        }

        if (!Uri.TryCreate(dto.ApplicationUrl, UriKind.Absolute, out var url) ||
            (url.Scheme != Uri.UriSchemeHttp &&
             url.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest(new
            {
                message = "ApplicationUrl must be a valid HTTP or HTTPS URL."
            });
        }

        if (dto.SalaryFrom < 0 || dto.SalaryTo < 0 ||
            (dto.SalaryFrom.HasValue && dto.SalaryTo.HasValue &&
             dto.SalaryFrom > dto.SalaryTo))
        {
            return BadRequest(new
            {
                message = "Salary values are invalid."
            });
        }

        try
        {
            var vacancy = await _vacancyService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = vacancy.Id },
                vacancy);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/Vacancies/1
    // Vakansiyani yangilash
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateVacancyDto dto)
    {
        if (dto.CompanyId <= 0 ||
            string.IsNullOrWhiteSpace(dto.Title) ||
            string.IsNullOrWhiteSpace(dto.Description) ||
            string.IsNullOrWhiteSpace(dto.ApplicationUrl))
        {
            return BadRequest(new
            {
                message = "CompanyId, title, description and application URL are required."
            });
        }

        if (!Uri.TryCreate(dto.ApplicationUrl, UriKind.Absolute, out var url) ||
            (url.Scheme != Uri.UriSchemeHttp &&
             url.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest(new
            {
                message = "ApplicationUrl must be a valid HTTP or HTTPS URL."
            });
        }

        if (dto.SalaryFrom < 0 || dto.SalaryTo < 0 ||
            (dto.SalaryFrom.HasValue && dto.SalaryTo.HasValue &&
             dto.SalaryFrom > dto.SalaryTo))
        {
            return BadRequest(new
            {
                message = "Salary values are invalid."
            });
        }

        try
        {
            var vacancy = await _vacancyService.UpdateAsync(id, dto);

            if (vacancy == null)
            {
                return NotFound(new
                {
                    message = "Vacancy not found."
                });
            }

            return Ok(vacancy);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/Vacancies/1
    // Vakansiyani o'chirish
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _vacancyService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Vacancy not found."
            });
        }

        return NoContent();
    }
}

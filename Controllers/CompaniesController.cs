
using JobMatch.DTOs;
using JobMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly CompanyService _companyService;

    public CompaniesController(CompanyService companyService)
    {
        _companyService = companyService;
    }

    // GET: api/companies
    // Barcha kompaniyalarni olish
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await _companyService.GetAllAsync();

        return Ok(companies);
    }

    // GET: api/companies/1
    // ID orqali bitta kompaniyani olish
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var company = await _companyService.GetByIdAsync(id);

        if (company == null)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return Ok(company);
    }

    // POST: api/companies
    // Yangi kompaniya yaratish
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCompanyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Company name is required."
            });
        }

        var company = await _companyService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = company.Id },
            company);
    }

    // PUT: api/companies/1
    // Kompaniya ma'lumotlarini yangilash
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCompanyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Company name is required."
            });
        }

        var company = await _companyService.UpdateAsync(id, dto);

        if (company == null)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return Ok(company);
    }

    // DELETE: api/companies/1
    // Kompaniyani o'chirish
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _companyService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return NoContent();
    }
}

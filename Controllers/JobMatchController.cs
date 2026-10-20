
using JobMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobMatchController : ControllerBase
{
    private readonly JobMatchService _jobMatchService;

    // JobMatchService'ni Controller ichida ishlatamiz.
    public JobMatchController(JobMatchService jobMatchService)
    {
        _jobMatchService = jobMatchService;
    }

    // GET: api/JobMatch/compare?resumeId=1&vacancyId=1
    // CV va vakansiya mosligini hisoblaydi.
    [HttpGet("compare")]
    public async Task<IActionResult> Compare(int resumeId, int vacancyId)
    {
        // ID raqamlari to'g'ri kiritilganini tekshiramiz.
        if (resumeId <= 0 || vacancyId <= 0)
        {
            return BadRequest(new
            {
                message = "Resume ID and Vacancy ID must be greater than 0."
            });
        }

        // Service orqali moslikni hisoblaymiz.
        var result = await _jobMatchService.CalculateMatchAsync(
            resumeId,
            vacancyId);

        // Resume yoki vakansiya topilmasa.
        if (result == null)
        {
            return NotFound(new
            {
                message = "Resume or vacancy not found."
            });
        }

        // Hisoblangan natijani qaytaramiz.
        return Ok(result);
    }
}

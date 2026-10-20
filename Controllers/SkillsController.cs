
using JobMatch.Models;
using JobMatch.Services;
using Microsoft.AspNetCore.Mvc;
using JobMatch.DTOs;

namespace JobMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly SkillService _skillService;

    public SkillsController(SkillService skillService)
    {
        _skillService = skillService;
    }

    // GET: api/Skills
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var skills = await _skillService.GetAllAsync();

        return Ok(skills);
    }

    // GET: api/Skills/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var skill = await _skillService.GetByIdAsync(id);

        if (skill == null)
        {
            return NotFound(new
            {
                message = "Skill not found."
            });
        }

        return Ok(skill);
    }

    // POST: api/Skills
    [HttpPost]
    public async Task<IActionResult> Create(CreateSkillDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Skill name is required."
            });
        }

        var skill = await _skillService.CreateAsync(dto.Name);

        return CreatedAtAction(
            nameof(GetById),
            new { id = skill.Id },
            skill);
    }

    // PUT: api/Skills/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Skill skill)
    {
        if (string.IsNullOrWhiteSpace(skill.Name))
        {
            return BadRequest(new
            {
                message = "Skill name is required."
            });
        }

        var updatedSkill = await _skillService.UpdateAsync(id, skill.Name);

        if (updatedSkill == null)
        {
            return NotFound(new
            {
                message = "Skill not found."
            });
        }

        return Ok(updatedSkill);
    }

    // DELETE: api/Skills/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _skillService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Skill not found."
            });
        }

        return NoContent();
    }
}

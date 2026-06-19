using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Skills;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService) => _skillService = skillService;

    [HttpGet]
    public async Task<IActionResult> GetSkills([FromQuery] SkillQueryParams queryParams)
    {
        var skills = await _skillService.GetSkillsAsync(queryParams);
        return Ok(skills);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMySkills()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var skills = await _skillService.GetUserSkillsAsync(userId);
        return Ok(skills);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserSkills(string userId)
    {
        var skills = await _skillService.GetUserSkillsAsync(userId);
        return Ok(skills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkill(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        return Ok(skill);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var skill = await _skillService.CreateSkillAsync(userId, dto);
        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _skillService.DeleteSkillAsync(userId, id);
        return NoContent();
    }
}

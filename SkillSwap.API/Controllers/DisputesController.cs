using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Disputes;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;

    public DisputesController(IDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDispute([FromBody] CreateDisputeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var result = await _disputeService.CreateDisputeAsync(currentUserId, dto);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyDisputes()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var result = await _disputeService.GetUserDisputesAsync(currentUserId);
        return Ok(result);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Core.DTOs.Swaps;
using SkillSync.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSync.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SwapsController : ControllerBase
{
    private readonly ISwapService _swapService;

    public SwapsController(ISwapService swapService) => _swapService = swapService;

    [HttpGet]
    public async Task<IActionResult> GetSwaps()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swaps = await _swapService.GetUserSwapsAsync(userId);
        return Ok(swaps);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSwap([FromBody] CreateSwapDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.CreateSwapAsync(userId, dto);
        return CreatedAtAction(nameof(GetSwaps), swap);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSwapStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.UpdateSwapStatusAsync(userId, id, dto);
        return Ok(swap);
    }

    [HttpPut("{id}/timeslot")]
    public async Task<IActionResult> ProposeTimeSlot(int id, [FromBody] ProposeTimeSlotDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.ProposeTimeSlotAsync(userId, id, dto);
        return Ok(swap);
    }

    [HttpPut("{id}/pick-time")]
    public async Task<IActionResult> PickTime(int id, [FromBody] PickTimeDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.PickTimeAsync(userId, id, dto);
        return Ok(swap);
    }

    [HttpPut("{id}/schedule/propose")]
    public async Task<IActionResult> ProposeSchedule(int id, [FromBody] ProposeScheduleDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.ProposeScheduleAsync(userId, id, dto);
        return Ok(swap);
    }

    [HttpPut("{id}/schedule/confirm")]
    public async Task<IActionResult> ConfirmSchedule(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.ConfirmScheduleAsync(userId, id);
        return Ok(swap);
    }

    [HttpPut("{id}/schedule/request-change")]
    public async Task<IActionResult> RequestScheduleChange(int id, [FromBody] RequestScheduleChangeDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.RequestScheduleChangeAsync(userId, id, dto);
        return Ok(swap);
    }

    [HttpPut("{id}/sessions/{sessionId}/validate")]
    public async Task<IActionResult> ValidateSession(int id, int sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.ValidateSessionAsync(userId, id, sessionId);
        return Ok(swap);
    }

    [HttpPut("{id}/sessions/{sessionId}/invalidate")]
    public async Task<IActionResult> InvalidateSession(int id, int sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.InvalidateSessionAsync(userId, id, sessionId);
        return Ok(swap);
    }

    [HttpPut("{id}/validate")]
    public async Task<IActionResult> Validate(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.ValidateSwapAsync(userId, id);
        return Ok(swap);
    }

    [HttpPut("{id}/invalidate")]
    public async Task<IActionResult> Invalidate(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swap = await _swapService.InvalidateSwapAsync(userId, id);
        return Ok(swap);
    }
}

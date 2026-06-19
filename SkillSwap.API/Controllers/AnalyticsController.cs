using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Analytics;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardAnalyticsDto>> GetDashboardAnalytics()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var analytics = await _analyticsService.GetDashboardAnalyticsAsync(userId);
        return Ok(analytics);
    }
}

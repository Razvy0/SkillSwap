using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeTransactionsController : ControllerBase
{
    private readonly ITimeTransactionService _service;

    public TimeTransactionsController(ITimeTransactionService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTransactions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var transactions = await _service.GetUserTransactionsAsync(userId);
        return Ok(transactions);
    }
}

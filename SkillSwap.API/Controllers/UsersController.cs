using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Users;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] UserSearchParams searchParams)
    {
        var results = await _userService.SearchUsersAsync(searchParams);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(string id)
    {
        var profile = await _userService.GetProfileAsync(id);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _userService.UpdateProfileAsync(userId, dto);
        return Ok(profile);
    }
}

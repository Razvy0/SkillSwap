using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.Core.DTOs.Auth;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Exceptions;
using SkillSwap.Core.Interfaces.Services;

namespace SkillSwap.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<AppUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new BadRequestException("A user with this email already exists.");

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            TimeBalance = 5
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return GenerateToken(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedException("Invalid email or password.");

        var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
            throw new UnauthorizedException("Invalid email or password.");

        return GenerateToken(user);
    }

    private AuthResponseDto GenerateToken(AppUser user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var expiration = DateTime.UtcNow.AddHours(24);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Expiration = expiration
        };
    }
}

using SkillSwap.Core.DTOs.Auth;

namespace SkillSwap.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

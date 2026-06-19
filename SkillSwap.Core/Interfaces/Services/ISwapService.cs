using SkillSwap.Core.DTOs.Swaps;

namespace SkillSwap.Core.Interfaces.Services;

public interface ISwapService
{
    Task<IEnumerable<SwapDto>> GetUserSwapsAsync(string userId);
    Task<SwapDto> CreateSwapAsync(string requesterId, CreateSwapDto dto);
    Task<SwapDto> UpdateSwapStatusAsync(string userId, int swapId, UpdateSwapStatusDto dto);
    Task<SwapDto> ProposeTimeSlotAsync(string userId, int swapId, ProposeTimeSlotDto dto);
    Task<SwapDto> PickTimeAsync(string userId, int swapId, PickTimeDto dto);
    Task<SwapDto> ProposeScheduleAsync(string userId, int swapId, ProposeScheduleDto dto);
    Task<SwapDto> ConfirmScheduleAsync(string userId, int swapId);
    Task<SwapDto> RequestScheduleChangeAsync(string userId, int swapId, RequestScheduleChangeDto dto);
    Task<SwapDto> ValidateSessionAsync(string userId, int swapId, int sessionId);
    Task<SwapDto> InvalidateSessionAsync(string userId, int swapId, int sessionId);
    Task<SwapDto> ValidateSwapAsync(string userId, int swapId);
    Task<SwapDto> InvalidateSwapAsync(string userId, int swapId);
}

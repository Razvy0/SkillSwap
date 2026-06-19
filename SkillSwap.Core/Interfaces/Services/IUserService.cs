using SkillSwap.Core.DTOs.Users;

using SkillSwap.Core.DTOs.Common;

namespace SkillSwap.Core.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateUserProfileDto dto);
    Task<PagedResult<UserSearchResultDto>> SearchUsersAsync(UserSearchParams searchParams);
}

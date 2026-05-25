using SkillSync.Core.DTOs.Common;
using SkillSync.Core.DTOs.Skills;
using SkillSync.Core.Entities;
using SkillSync.Core.Enums;
using SkillSync.Core.Exceptions;
using SkillSync.Core.Interfaces.Repositories;
using SkillSync.Core.Interfaces.Services;

namespace SkillSync.API.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ISwapRepository _swapRepo;

    public SkillService(ISkillRepository skillRepo, ICategoryRepository categoryRepo, ISwapRepository swapRepo)
    {
        _skillRepo = skillRepo;
        _categoryRepo = categoryRepo;
        _swapRepo = swapRepo;
    }

    public async Task<PagedResult<SkillDto>> GetSkillsAsync(SkillQueryParams queryParams)
    {
        var (items, totalCount) = await _skillRepo.GetSkillsWithDetailsAsync(
            queryParams.Category, queryParams.Search, queryParams.IsOffering,
            queryParams.Page, queryParams.PageSize);

        return new PagedResult<SkillDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount
        };
    }

    public async Task<SkillDto> GetSkillByIdAsync(int id)
    {
        var skill = await _skillRepo.GetSkillWithDetailsAsync(id)
            ?? throw new NotFoundException("Skill", id);
        return MapToDto(skill);
    }

    public async Task<SkillDto> CreateSkillAsync(string userId, CreateSkillDto dto)
    {
        if (!await _categoryRepo.ExistsAsync(dto.CategoryId))
            throw new NotFoundException("Category", dto.CategoryId);

        if (dto.RequiredSessions < 1 || dto.RequiredSessions > 8)
            throw new BadRequestException("Required sessions must be between 1 and 8.");

        if (dto.LessonMode == LessonMode.SingleOnly && dto.RequiredSessions != 1)
            throw new BadRequestException("Single-only skills must require exactly 1 session.");

        if (dto.LessonMode == LessonMode.RecurringOnly && dto.RequiredSessions < 2)
            throw new BadRequestException("Recurring-only skills must require at least 2 sessions.");

        var skill = new Skill
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Description = dto.Description,
            ProficiencyLevel = dto.ProficiencyLevel,
            IsOffering = dto.IsOffering,
            LessonMode = dto.LessonMode,
            RequiredSessions = dto.RequiredSessions
        };

        await _skillRepo.AddAsync(skill);
        return await GetSkillByIdAsync(skill.Id);
    }

    public async Task DeleteSkillAsync(string userId, int id)
    {
        var skill = await _skillRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Skill", id);

        if (skill.UserId != userId)
            throw new UnauthorizedException("You can only delete your own skills.");

        if (await _swapRepo.HasActiveSwapForSkillAsync(id))
            throw new BadRequestException("You cannot delete a skill while it is part of an active swap.");

        await _skillRepo.DeleteAsync(skill);
    }

    public async Task<IEnumerable<SkillDto>> GetUserSkillsAsync(string userId)
    {
        var skills = await _skillRepo.GetSkillsByUserIdAsync(userId);
        return skills.Select(MapToDto);
    }

    private static SkillDto MapToDto(Skill s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        ProficiencyLevel = s.ProficiencyLevel,
        IsOffering = s.IsOffering,
        LessonMode = s.LessonMode,
        RequiredSessions = s.RequiredSessions,
        CategoryName = s.Category?.Name ?? "",
        CategoryId = s.CategoryId,
        UserId = s.UserId,
        UserFullName = s.User?.FullName ?? "",
        CreatedAt = s.CreatedAt
    };
}

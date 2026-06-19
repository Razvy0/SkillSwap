using SkillSwap.Core.DTOs.Categories;

namespace SkillSwap.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
}

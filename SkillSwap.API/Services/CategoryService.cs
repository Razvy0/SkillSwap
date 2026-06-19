using SkillSwap.Core.DTOs.Categories;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Exceptions;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Core.Interfaces.Services;

namespace SkillSwap.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(ICategoryRepository categoryRepo) => _categoryRepo = categoryRepo;

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description });
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var existing = await _categoryRepo.GetByNameAsync(dto.Name);
        if (existing != null)
            throw new BadRequestException($"Category '{dto.Name}' already exists.");

        var category = new Category { Name = dto.Name, Description = dto.Description };
        await _categoryRepo.AddAsync(category);

        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }
}

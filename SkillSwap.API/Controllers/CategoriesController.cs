using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Categories;
using SkillSwap.Core.Interfaces.Services;

namespace SkillSwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetAll), category);
    }
}

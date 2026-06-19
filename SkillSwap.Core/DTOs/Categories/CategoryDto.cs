using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Core.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

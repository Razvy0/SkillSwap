using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new Category { Id = 1, Name = "Technology", Description = "Programming, software development, and IT skills" },
            new Category { Id = 2, Name = "Sciences", Description = "Physics, chemistry, biology, and other natural sciences" },
            new Category { Id = 3, Name = "Mathematics", Description = "Algebra, calculus, statistics, and applied mathematics" },
            new Category { Id = 4, Name = "Languages", Description = "Foreign languages, translation, and linguistics" },
            new Category { Id = 5, Name = "Music", Description = "Instruments, music theory, singing, and production" },
            new Category { Id = 6, Name = "Fitness", Description = "Exercise, personal training, yoga, and sports" },
            new Category { Id = 7, Name = "Art & Design", Description = "Drawing, painting, graphic design, and photography" },
            new Category { Id = 8, Name = "History", Description = "World history, civilizations, and historical research" },
            new Category { Id = 9, Name = "Literature", Description = "Creative writing, poetry, and literary analysis" },
            new Category { Id = 10, Name = "Business", Description = "Marketing, finance, entrepreneurship, and management" },
            new Category { Id = 11, Name = "Cooking", Description = "Culinary arts, baking, and nutrition" },
            new Category { Id = 12, Name = "Crafts & DIY", Description = "Woodworking, sewing, knitting, and home improvement" }
        );
    }
}

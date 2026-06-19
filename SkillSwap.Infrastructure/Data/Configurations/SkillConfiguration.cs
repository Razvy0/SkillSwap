using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.ProficiencyLevel).HasConversion<string>();
        builder.Property(s => s.LessonMode).HasConversion<string>();
        builder.Property(s => s.RequiredSessions).HasDefaultValue(1);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Skills)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Skills)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

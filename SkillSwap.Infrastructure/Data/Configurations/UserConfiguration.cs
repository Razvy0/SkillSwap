using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(500);
        builder.Property(u => u.TimeBalance).HasDefaultValue(5);
        builder.Property(u => u.Rating).HasDefaultValue(0.0);
    }
}

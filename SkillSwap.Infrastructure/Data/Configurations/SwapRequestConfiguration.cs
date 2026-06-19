using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class SwapRequestConfiguration : IEntityTypeConfiguration<SwapRequest>
{
    public void Configure(EntityTypeBuilder<SwapRequest> builder)
    {
        builder.Property(s => s.Status).HasConversion<string>();
        builder.Property(s => s.LessonType).HasConversion<string>();
        builder.Property(s => s.RequestedCadence).HasConversion<string>();
        builder.Property(s => s.OfferedCadence).HasConversion<string>();
        builder.Property(s => s.TwoWayScheduleMode).HasConversion<string>();

        builder.HasOne(s => s.Requester)
            .WithMany()
            .HasForeignKey(s => s.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Receiver)
            .WithMany()
            .HasForeignKey(s => s.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.OfferedSkill)
            .WithMany()
            .HasForeignKey(s => s.OfferedSkillId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(s => s.RequestedSkill)
            .WithMany()
            .HasForeignKey(s => s.RequestedSkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

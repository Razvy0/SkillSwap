using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSync.Core.Entities;

namespace SkillSync.Infrastructure.Data.Configurations;

public class SwapScheduleConfiguration : IEntityTypeConfiguration<SwapSchedule>
{
    public void Configure(EntityTypeBuilder<SwapSchedule> builder)
    {
        builder.Property(s => s.Cadence).HasConversion<string>();
        builder.Property(s => s.Track).HasConversion<string>();
        builder.Property(s => s.Status).HasConversion<string>();
        builder.Property(s => s.WeekDays).HasMaxLength(100);
        builder.Property(s => s.ChangeRequestNote).HasMaxLength(100);

        builder.HasOne(s => s.SwapRequest)
            .WithMany(r => r.Schedules)
            .HasForeignKey(s => s.SwapRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ProposedBy)
            .WithMany()
            .HasForeignKey(s => s.ProposedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

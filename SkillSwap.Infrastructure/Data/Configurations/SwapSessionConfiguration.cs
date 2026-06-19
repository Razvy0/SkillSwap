using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class SwapSessionConfiguration : IEntityTypeConfiguration<SwapSession>
{
    public void Configure(EntityTypeBuilder<SwapSession> builder)
    {
        builder.Property(s => s.Track).HasConversion<string>();
        builder.Property(s => s.Status).HasConversion<string>();

        builder.HasOne(s => s.SwapRequest)
            .WithMany(r => r.Sessions)
            .HasForeignKey(s => s.SwapRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SwapSchedule)
            .WithMany(sc => sc.Sessions)
            .HasForeignKey(s => s.SwapScheduleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.Property(d => d.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(d => d.AdminNotes).HasMaxLength(2000);

        builder.HasOne(d => d.Reporter)
            .WithMany(u => u.DisputesReported)
            .HasForeignKey(d => d.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ReportedUser)
            .WithMany(u => u.DisputesReceived)
            .HasForeignKey(d => d.ReportedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.SwapRequest)
            .WithMany(s => s.Disputes)
            .HasForeignKey(d => d.SwapRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
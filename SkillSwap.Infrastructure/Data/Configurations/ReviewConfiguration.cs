using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Core.Entities;

namespace SkillSwap.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(1000);

        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsGiven)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reviewee)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.SwapRequest)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.SwapRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.ReviewerId, r.SwapRequestId }).IsUnique();
    }
}

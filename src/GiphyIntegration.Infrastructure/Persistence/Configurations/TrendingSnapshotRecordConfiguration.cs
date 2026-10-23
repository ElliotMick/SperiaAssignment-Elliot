using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiphyIntegration.Infrastructure.Persistence.Configurations;

internal sealed class TrendingSnapshotRecordConfiguration : IEntityTypeConfiguration<TrendingSnapshotRecord>
{
    public void Configure(EntityTypeBuilder<TrendingSnapshotRecord> builder)
    {
        builder.ToTable("TrendingSnapshots");
        builder.HasKey(s => s.Id);

        builder.HasMany(s => s.Gifs)
            .WithOne()
            .HasForeignKey(g => g.TrendingSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

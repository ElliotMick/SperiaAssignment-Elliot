using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiphyIntegration.Infrastructure.Persistence.Configurations;

internal sealed class TrendingGifRecordConfiguration : GifRecordConfiguration<TrendingGifRecord>
{
    protected override void ConfigureOwner(EntityTypeBuilder<TrendingGifRecord> builder)
    {
        builder.ToTable("TrendingGifs");
        builder.HasIndex(g => new { g.TrendingSnapshotId, g.Position });
    }
}

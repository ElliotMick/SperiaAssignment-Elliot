using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiphyIntegration.Infrastructure.Persistence.Configurations;

internal sealed class SearchResultGifRecordConfiguration : GifRecordConfiguration<SearchResultGifRecord>
{
    protected override void ConfigureOwner(EntityTypeBuilder<SearchResultGifRecord> builder)
    {
        builder.ToTable("SearchResultGifs");
        builder.HasIndex(g => new { g.SearchResultId, g.Position });
    }
}

using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiphyIntegration.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the columns shared by every GIF table; subclasses add their table name and owner specific mapping.
/// </summary>
internal abstract class GifRecordConfiguration<TRecord> : IEntityTypeConfiguration<TRecord>
    where TRecord : GifRecord
{
    public void Configure(EntityTypeBuilder<TRecord> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.GiphyId).HasMaxLength(64).IsRequired();
        builder.Property(g => g.Title).HasMaxLength(512).IsRequired();
        builder.Property(g => g.Url).HasMaxLength(2048).IsRequired();
        builder.Property(g => g.PreviewUrl).HasMaxLength(2048).IsRequired();

        ConfigureOwner(builder);
    }

    protected abstract void ConfigureOwner(EntityTypeBuilder<TRecord> builder);
}

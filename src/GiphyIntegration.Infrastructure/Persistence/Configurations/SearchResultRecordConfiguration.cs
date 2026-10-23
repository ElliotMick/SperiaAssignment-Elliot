using GiphyIntegration.Core.Models;
using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiphyIntegration.Infrastructure.Persistence.Configurations;

internal sealed class SearchResultRecordConfiguration : IEntityTypeConfiguration<SearchResultRecord>
{
    public void Configure(EntityTypeBuilder<SearchResultRecord> builder)
    {
        builder.ToTable("SearchResults");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Term).HasMaxLength(SearchTerm.MaxLength).IsRequired();
        builder.HasIndex(r => r.Term).IsUnique();
        builder.HasIndex(r => r.LastAccessedAtUtc);

        builder.HasMany(r => r.Gifs)
            .WithOne(g => g.SearchResult)
            .HasForeignKey(g => g.SearchResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.UserSearches)
            .WithOne(s => s.SearchResult)
            .HasForeignKey(s => s.SearchResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

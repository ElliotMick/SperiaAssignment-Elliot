using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GiphyIntegration.Infrastructure.Persistence;

internal sealed class GifCacheDbContext(DbContextOptions<GifCacheDbContext> options) : DbContext(options)
{
    public DbSet<SearchResultRecord> SearchResults => Set<SearchResultRecord>();

    public DbSet<SearchResultGifRecord> SearchResultGifs => Set<SearchResultGifRecord>();

    public DbSet<UserSearchRecord> UserSearches => Set<UserSearchRecord>();

    public DbSet<TrendingSnapshotRecord> TrendingSnapshots => Set<TrendingSnapshotRecord>();

    public DbSet<TrendingGifRecord> TrendingGifs => Set<TrendingGifRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GifCacheDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        // SQLite has no time zone aware type: every timestamp is stored as UTC and read back flagged as UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

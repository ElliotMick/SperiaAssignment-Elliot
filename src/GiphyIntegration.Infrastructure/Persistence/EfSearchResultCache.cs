using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiphyIntegration.Infrastructure.Persistence;

internal sealed class EfSearchResultCache(
    GifCacheDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<EfSearchResultCache> logger) : ISearchResultCache
{
    public async Task<IReadOnlyList<Gif>?> TryGetAsync(SearchTerm term, CancellationToken cancellationToken)
    {
        // Touch first, then read: once LastAccessedAtUtc is refreshed the purge job leaves this result alone,
        // so it cannot disappear between this check and the read below.
        // Improvement: this makes every cache hit a write, and SQLite allows a single writer at a time. Under heavy
        // traffic, skip the touch when it was refreshed recently (e.g. within a minute) or serve hits from memory first.
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var touched = await dbContext.SearchResults
            .Where(r => r.Term == term.Value)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.LastAccessedAtUtc, now), cancellationToken);
        if (touched == 0)
        {
            return null;
        }

        return await dbContext.SearchResultGifs
            .AsNoTracking()
            .Where(g => g.SearchResult!.Term == term.Value)
            .OrderBy(g => g.Position)
            .Select(g => new Gif(g.GiphyId, g.Title, g.Url, g.PreviewUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SearchTerm term, IReadOnlyList<Gif> gifs, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.SearchResults.Add(new SearchResultRecord
        {
            Term = term.Value,
            CreatedAtUtc = now,
            LastAccessedAtUtc = now,
            Gifs = GifRecord.FromModels<SearchResultGifRecord>(gifs),
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            // Another instance cached the same term first; its result is just as good.
            logger.LogDebug("Search result for '{Term}' was already cached", term);
            dbContext.ChangeTracker.Clear();
        }
    }

    /// <remarks>
    /// Improvement: this walks every result not accessed since the cutoff, most of which are still referenced, while
    /// holding SQLite's write lock, so its cost grows with the table. Flag results as orphaned when their last user
    /// search is evicted (e.g. an indexed OrphanedAtUtc column) and purge only those, in batches.
    /// </remarks>
    public Task<int> PurgeUnreferencedAsync(DateTime olderThanUtc, CancellationToken cancellationToken) =>
        // The database cascades the delete to the result's GIFs.
        dbContext.SearchResults
            .Where(r => r.LastAccessedAtUtc < olderThanUtc && !r.UserSearches.Any())
            .ExecuteDeleteAsync(cancellationToken);
}

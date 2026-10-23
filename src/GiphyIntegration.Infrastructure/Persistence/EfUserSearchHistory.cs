using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Core.Options;
using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Infrastructure.Persistence;

/// <remarks>
/// Improvement: searches are capped per user, but users are never forgotten, and IP based users keep appearing
/// (dynamic IPs), so the tables grow without bound. Let the purge job also delete searches not repeated for a while
/// (e.g. 30 days, with an index on LastSearchedAtUtc); the results they referenced are then purged as usual.
/// </remarks>
internal sealed class EfUserSearchHistory(
    GifCacheDbContext dbContext,
    IOptions<SearchCacheOptions> options,
    TimeProvider timeProvider) : IUserSearchHistory
{
    private int MaxSearchesPerUser => options.Value.MaxSearchesPerUser;

    public async Task RecordAsync(string userKey, SearchTerm term, CancellationToken cancellationToken)
    {
        var searchResultId = await dbContext.SearchResults
            .Where(r => r.Term == term.Value)
            .Select(r => (int?)r.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (searchResultId is null)
        {
            // Only happens if the result was purged in the meantime: there is nothing left to link to.
            return;
        }

        // Upsert: bump the search if the user already has it, insert it otherwise.
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var updated = await dbContext.UserSearches
            .Where(s => s.UserKey == userKey && s.SearchResultId == searchResultId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.LastSearchedAtUtc, now), cancellationToken);
        if (updated == 0)
        {
            dbContext.UserSearches.Add(new UserSearchRecord
            {
                UserKey = userKey,
                SearchResultId = searchResultId.Value,
                LastSearchedAtUtc = now,
            });

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                // A concurrent request from the same user recorded the same search first.
                dbContext.ChangeTracker.Clear();
            }
        }

        await EvictOldestAsync(userKey, cancellationToken);
    }

    public async Task<IReadOnlyList<CachedSearch>> GetAsync(string userKey, CancellationToken cancellationToken)
    {
        var searches = await MostRecentFirst(userKey)
            .Take(MaxSearchesPerUser)
            .Select(s => new
            {
                s.SearchResult!.Term,
                s.LastSearchedAtUtc,
                Gifs = s.SearchResult.Gifs
                    .OrderBy(g => g.Position)
                    .Select(g => new Gif(g.GiphyId, g.Title, g.Url, g.PreviewUrl))
                    .ToList(),
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return searches.Select(s => new CachedSearch(s.Term, s.LastSearchedAtUtc, s.Gifs)).ToList();
    }

    /// <summary>
    /// Keeps only the user's most recent searches (LRU). Their results stay shared with other users and are
    /// purged by the background job once nobody references them anymore.
    /// </summary>
    private Task<int> EvictOldestAsync(string userKey, CancellationToken cancellationToken)
    {
        var searchesToKeep = MostRecentFirst(userKey).Take(MaxSearchesPerUser).Select(s => s.Id);

        return dbContext.UserSearches
            .Where(s => s.UserKey == userKey && !searchesToKeep.Contains(s.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private IQueryable<UserSearchRecord> MostRecentFirst(string userKey) =>
        dbContext.UserSearches
            .Where(s => s.UserKey == userKey)
            .OrderByDescending(s => s.LastSearchedAtUtc)
            .ThenByDescending(s => s.Id);
}

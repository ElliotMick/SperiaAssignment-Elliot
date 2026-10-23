using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;
using Microsoft.Extensions.Logging;

namespace GiphyIntegration.Core.Search;

/// <summary>
/// Serves searches from the shared result cache and records them in the caller's own history.
/// The upstream provider is called once per term, whichever user searched it first.
/// </summary>
public sealed class SearchGifService(
    IGifSource source,
    ISearchResultCache resultCache,
    IUserSearchHistory history,
    IKeyedLock keyedLock,
    ILogger<SearchGifService> logger) : ISearchGifService
{
    public async Task<GifQueryResult> SearchAsync(string userKey, SearchTerm term, CancellationToken cancellationToken)
    {
        var result = await GetOrFetchAsync(term, cancellationToken);

        // Recorded on cache hits too, so the user's cached searches reflect what they actually looked for.
        await history.RecordAsync(userKey, term, cancellationToken);

        return result;
    }

    private async Task<GifQueryResult> GetOrFetchAsync(SearchTerm term, CancellationToken cancellationToken)
    {
        var cached = await resultCache.TryGetAsync(term, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for '{Term}'", term);
            return new GifQueryResult(cached, FromCache: true);
        }

        // Single flight: concurrent requests for the same new term queue here, and all but the first
        // find the result cached by the double check below instead of paying for another upstream call.
        using (await keyedLock.AcquireAsync($"search:{term}", cancellationToken))
        {
            cached = await resultCache.TryGetAsync(term, cancellationToken);
            if (cached is not null)
            {
                return new GifQueryResult(cached, FromCache: true);
            }

            logger.LogInformation("Cache miss for '{Term}', calling the upstream provider", term);
            var gifs = await source.SearchAsync(term, cancellationToken);

            // Already paid for: persist even if the caller goes away. Empty results are cached too.
            await resultCache.AddAsync(term, gifs, CancellationToken.None);

            return new GifQueryResult(gifs, FromCache: false);
        }
    }
}

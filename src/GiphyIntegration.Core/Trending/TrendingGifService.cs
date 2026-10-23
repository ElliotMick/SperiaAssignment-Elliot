using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Core.Trending;

/// <summary>
/// Serves trending GIFs from a two level cache: process memory first, then the database, and the upstream
/// provider only when neither holds a snapshot. All slow paths share one lock, so requests and the refresh
/// worker never call the upstream provider at the same time.
/// </summary>
public sealed class TrendingGifService(
    IGifSource source,
    ITrendingGifCache memoryCache,
    ITrendingGifStore store,
    IKeyedLock keyedLock,
    IOptions<TrendingOptions> options,
    TimeProvider timeProvider,
    ILogger<TrendingGifService> logger) : ITrendingGifService
{
    private const string LockKey = "trending";

    public async Task<GifQueryResult> GetAsync(CancellationToken cancellationToken)
    {
        // Fast path: lock-free read of the in-memory snapshot.
        if (memoryCache.Current is { } cached)
        {
            return new GifQueryResult(cached.Gifs, FromCache: true);
        }

        using (await keyedLock.AcquireAsync(LockKey, cancellationToken))
        {
            // Another request, or the worker, may have filled the cache while we were waiting.
            if (memoryCache.Current is { } filled)
            {
                return new GifQueryResult(filled.Gifs, FromCache: true);
            }

            var persisted = await store.GetLatestAsync(cancellationToken);
            if (persisted is not null)
            {
                memoryCache.Set(persisted);
                return new GifQueryResult(persisted.Gifs, FromCache: true);
            }

            var fetched = await FetchAndStoreAsync(cancellationToken);
            return new GifQueryResult(fetched.Gifs, FromCache: false);
        }
    }

    public async Task WarmUpAsync(CancellationToken cancellationToken)
    {
        using (await keyedLock.AcquireAsync(LockKey, cancellationToken))
        {
            var persisted = await store.GetLatestAsync(cancellationToken);
            if (persisted is not null)
            {
                // Serve the persisted snapshot right away, even if stale: it beats nothing should the refresh fail.
                memoryCache.Set(persisted);
                if (!persisted.IsStale(UtcNow, options.Value.RefreshInterval))
                {
                    logger.LogInformation(
                        "Trending GIFs restored from the database (fetched at {FetchedAtUtc:u})", persisted.FetchedAtUtc);
                    return;
                }
            }

            await FetchAndStoreAsync(cancellationToken);
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using (await keyedLock.AcquireAsync(LockKey, cancellationToken))
        {
            await FetchAndStoreAsync(cancellationToken);
        }
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private async Task<TrendingSnapshot> FetchAndStoreAsync(CancellationToken cancellationToken)
    {
        var gifs = await source.GetTrendingAsync(cancellationToken);
        var snapshot = new TrendingSnapshot(gifs, UtcNow);

        // Serve the new snapshot immediately, then persist it. The upstream call is already paid for,
        // so persisting must not be cancelled by a caller going away.
        memoryCache.Set(snapshot);
        await store.ReplaceAsync(snapshot, CancellationToken.None);

        logger.LogInformation("Fetched {Count} trending GIFs from the upstream provider", gifs.Count);
        return snapshot;
    }
}

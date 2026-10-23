using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

public interface ITrendingGifService
{
    /// <summary>Returns the trending GIFs, calling the upstream provider only when nothing is cached at all.</summary>
    Task<GifQueryResult> GetAsync(CancellationToken cancellationToken);

    /// <summary>Loads the persisted snapshot into memory, fetching a new one only if it is missing or stale.</summary>
    Task WarmUpAsync(CancellationToken cancellationToken);

    /// <summary>Fetches a new snapshot from the upstream provider and replaces the cached one.</summary>
    Task RefreshAsync(CancellationToken cancellationToken);
}

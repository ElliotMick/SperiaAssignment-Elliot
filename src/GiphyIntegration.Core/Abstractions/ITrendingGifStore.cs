using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

/// <summary>
/// Durable storage of the trending snapshot, so it survives application restarts.
/// </summary>
public interface ITrendingGifStore
{
    Task<TrendingSnapshot?> GetLatestAsync(CancellationToken cancellationToken);

    /// <summary>Stores <paramref name="snapshot"/> and removes any previously stored one.</summary>
    Task ReplaceAsync(TrendingSnapshot snapshot, CancellationToken cancellationToken);
}

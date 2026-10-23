using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

/// <summary>
/// Process level (in memory) cache holding the current trending snapshot.
/// </summary>
public interface ITrendingGifCache
{
    TrendingSnapshot? Current { get; }

    void Set(TrendingSnapshot snapshot);
}

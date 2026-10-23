using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Trending;

/// <summary>
/// Holds the current trending snapshot for the whole process. Snapshots are immutable and swapped as a
/// single reference, so reads are lock-free and never observe a partially updated list.
/// </summary>
public sealed class InMemoryTrendingGifCache : ITrendingGifCache
{
    private volatile TrendingSnapshot? _current;

    public TrendingSnapshot? Current => _current;

    public void Set(TrendingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _current = snapshot;
    }
}

namespace GiphyIntegration.Core.Models;

/// <summary>
/// The trending GIFs as fetched from the upstream provider at a given point in time.
/// Immutable, so it can be shared across threads and swapped atomically.
/// </summary>
public sealed record TrendingSnapshot(IReadOnlyList<Gif> Gifs, DateTime FetchedAtUtc)
{
    public bool IsStale(DateTime nowUtc, TimeSpan maxAge) => nowUtc - FetchedAtUtc >= maxAge;
}

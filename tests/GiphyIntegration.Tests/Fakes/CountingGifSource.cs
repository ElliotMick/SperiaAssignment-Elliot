using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Exceptions;
using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Tests.Fakes;

/// <summary>
/// Stands in for the paid upstream API: counts calls and simulates network latency, so concurrent
/// callers genuinely overlap.
/// </summary>
internal sealed class CountingGifSource : IGifSource
{
    private int _trendingCalls;
    private int _searchCalls;

    public int TrendingCalls => _trendingCalls;

    public int SearchCalls => _searchCalls;

    public bool Fail { get; set; }

    public IReadOnlyList<Gif> Trending { get; set; } = TestGifs.Create("trending");

    public async Task<IReadOnlyList<Gif>> GetTrendingAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _trendingCalls);
        await Task.Delay(20, cancellationToken);
        return Fail ? throw new GifSourceException("Upstream down.") : Trending;
    }

    public async Task<IReadOnlyList<Gif>> SearchAsync(SearchTerm term, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _searchCalls);
        await Task.Delay(20, cancellationToken);
        return Fail ? throw new GifSourceException("Upstream down.") : TestGifs.Create(term.Value);
    }
}

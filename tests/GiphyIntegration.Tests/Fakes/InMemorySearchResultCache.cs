using System.Collections.Concurrent;
using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Tests.Fakes;

internal sealed class InMemorySearchResultCache : ISearchResultCache
{
    private readonly ConcurrentDictionary<SearchTerm, IReadOnlyList<Gif>> _results = new();

    public Task<IReadOnlyList<Gif>?> TryGetAsync(SearchTerm term, CancellationToken cancellationToken) =>
        Task.FromResult(_results.GetValueOrDefault(term));

    public Task AddAsync(SearchTerm term, IReadOnlyList<Gif> gifs, CancellationToken cancellationToken)
    {
        _results.TryAdd(term, gifs);
        return Task.CompletedTask;
    }

    public Task<int> PurgeUnreferencedAsync(DateTime olderThanUtc, CancellationToken cancellationToken) =>
        Task.FromResult(0);
}

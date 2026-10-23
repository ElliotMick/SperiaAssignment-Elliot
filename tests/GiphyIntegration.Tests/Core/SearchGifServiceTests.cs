using GiphyIntegration.Core.Concurrency;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Core.Search;
using GiphyIntegration.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace GiphyIntegration.Tests.Core;

public sealed class SearchGifServiceTests
{
    private const string Alice = "10.0.0.1";
    private const string Bob = "10.0.0.2";

    private readonly CountingGifSource _source = new();
    private readonly RecordingUserSearchHistory _history = new();
    private readonly SearchGifService _service;

    public SearchGifServiceTests() =>
        _service = new SearchGifService(
            _source,
            new InMemorySearchResultCache(),
            _history,
            new StripedAsyncLock(),
            NullLogger<SearchGifService>.Instance);

    [Fact]
    public async Task SearchAsync_SameTermWithDifferentCasing_CallsTheSourceOnce()
    {
        var first = await _service.SearchAsync(Alice, SearchTerm.Create("Funny Cats"), CancellationToken.None);
        var second = await _service.SearchAsync(Alice, SearchTerm.Create("  funny   CATS"), CancellationToken.None);

        Assert.False(first.FromCache);
        Assert.True(second.FromCache);
        Assert.Equal(first.Gifs, second.Gifs);
        Assert.Equal(1, _source.SearchCalls);
    }

    [Fact]
    public async Task SearchAsync_SameTermFromTwoUsers_SharesTheCachedResult()
    {
        await _service.SearchAsync(Alice, SearchTerm.Create("cats"), CancellationToken.None);
        var bobsResult = await _service.SearchAsync(Bob, SearchTerm.Create("cats"), CancellationToken.None);

        Assert.True(bobsResult.FromCache);
        Assert.Equal(1, _source.SearchCalls);
    }

    [Fact]
    public async Task SearchAsync_ConcurrentIdenticalSearches_CallTheSourceOnce()
    {
        var results = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(i =>
                Task.Run(() => _service.SearchAsync($"10.0.1.{i}", SearchTerm.Create("dogs"), CancellationToken.None))));

        Assert.Equal(1, _source.SearchCalls);
        Assert.Single(results, r => !r.FromCache);
    }

    [Fact]
    public async Task SearchAsync_ConcurrentDifferentSearches_EachCallTheSource()
    {
        await Task.WhenAll(
            Enumerable.Range(0, 5).Select(i =>
                Task.Run(() => _service.SearchAsync(Alice, SearchTerm.Create($"term {i}"), CancellationToken.None))));

        Assert.Equal(5, _source.SearchCalls);
    }

    [Fact]
    public async Task SearchAsync_RecordsEverySearchInTheCallersHistory()
    {
        await _service.SearchAsync(Alice, SearchTerm.Create("cats"), CancellationToken.None);
        await _service.SearchAsync(Bob, SearchTerm.Create("Cats"), CancellationToken.None);

        Assert.Equal([(Alice, "cats"), (Bob, "cats")], _history.Recorded);
    }
}

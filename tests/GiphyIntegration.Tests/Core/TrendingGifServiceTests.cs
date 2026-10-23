using GiphyIntegration.Core.Concurrency;
using GiphyIntegration.Core.Exceptions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Core.Options;
using GiphyIntegration.Core.Trending;
using GiphyIntegration.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace GiphyIntegration.Tests.Core;

public sealed class TrendingGifServiceTests
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);

    private readonly CountingGifSource _source = new();
    private readonly InMemoryTrendingGifCache _memoryCache = new();
    private readonly InMemoryTrendingGifStore _store = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero));
    private readonly TrendingGifService _service;

    public TrendingGifServiceTests() =>
        _service = new TrendingGifService(
            _source,
            _memoryCache,
            _store,
            new StripedAsyncLock(),
            Microsoft.Extensions.Options.Options.Create(new TrendingOptions { RefreshInterval = RefreshInterval }),
            _time,
            NullLogger<TrendingGifService>.Instance);

    [Fact]
    public async Task GetAsync_WhenNothingIsCached_FetchesOnceAndPersists()
    {
        var first = await _service.GetAsync(CancellationToken.None);
        var second = await _service.GetAsync(CancellationToken.None);

        Assert.False(first.FromCache);
        Assert.True(second.FromCache);
        Assert.Equal(1, _source.TrendingCalls);
        Assert.Equal(_source.Trending, _store.Snapshot?.Gifs);
    }

    [Fact]
    public async Task GetAsync_AfterARestart_ServesTheDatabaseSnapshotWithoutCallingTheSource()
    {
        _store.Snapshot = new TrendingSnapshot(TestGifs.Create("persisted"), _time.GetUtcNow().UtcDateTime);

        var result = await _service.GetAsync(CancellationToken.None);

        Assert.True(result.FromCache);
        Assert.Equal(_store.Snapshot.Gifs, result.Gifs);
        Assert.Same(_store.Snapshot, _memoryCache.Current);
        Assert.Equal(0, _source.TrendingCalls);
    }

    [Fact]
    public async Task GetAsync_ConcurrentColdCalls_CallTheSourceOnce()
    {
        var results = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(_ => Task.Run(() => _service.GetAsync(CancellationToken.None))));

        Assert.Equal(1, _source.TrendingCalls);
        Assert.Single(results, r => !r.FromCache);
    }

    [Fact]
    public async Task WarmUpAsync_WithAFreshPersistedSnapshot_DoesNotCallTheSource()
    {
        _store.Snapshot = new TrendingSnapshot(TestGifs.Create("persisted"), _time.GetUtcNow().UtcDateTime);
        _time.Advance(RefreshInterval - TimeSpan.FromMinutes(1));

        await _service.WarmUpAsync(CancellationToken.None);

        Assert.Equal(0, _source.TrendingCalls);
        Assert.Same(_store.Snapshot, _memoryCache.Current);
    }

    [Fact]
    public async Task WarmUpAsync_WithAStalePersistedSnapshot_FetchesANewOne()
    {
        _store.Snapshot = new TrendingSnapshot(TestGifs.Create("persisted"), _time.GetUtcNow().UtcDateTime);
        _time.Advance(RefreshInterval);

        await _service.WarmUpAsync(CancellationToken.None);

        Assert.Equal(1, _source.TrendingCalls);
        Assert.Equal(_source.Trending, _memoryCache.Current?.Gifs);
        Assert.Equal(_source.Trending, _store.Snapshot.Gifs);
    }

    [Fact]
    public async Task RefreshAsync_ReplacesTheSnapshotInMemoryAndInTheStore()
    {
        await _service.GetAsync(CancellationToken.None);
        _source.Trending = TestGifs.Create("newer");
        _time.Advance(RefreshInterval);

        await _service.RefreshAsync(CancellationToken.None);

        Assert.Equal(2, _source.TrendingCalls);
        Assert.Equal(_source.Trending, _memoryCache.Current?.Gifs);
        Assert.Equal(_source.Trending, _store.Snapshot?.Gifs);
        Assert.Equal(_time.GetUtcNow().UtcDateTime, _store.Snapshot?.FetchedAtUtc);
    }

    [Fact]
    public async Task RefreshAsync_WhenTheSourceFails_KeepsServingThePreviousSnapshot()
    {
        await _service.GetAsync(CancellationToken.None);
        var previous = _memoryCache.Current;
        _source.Fail = true;

        await Assert.ThrowsAsync<GifSourceException>(() => _service.RefreshAsync(CancellationToken.None));

        Assert.Same(previous, _memoryCache.Current);
        Assert.Same(previous, _store.Snapshot);
        Assert.True((await _service.GetAsync(CancellationToken.None)).FromCache);
    }
}

using GiphyIntegration.Core.Models;
using GiphyIntegration.Core.Options;
using GiphyIntegration.Infrastructure.Persistence;
using GiphyIntegration.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace GiphyIntegration.Tests.Persistence;

/// <summary>
/// Covers the shared search results and the per-user histories referencing them.
/// </summary>
public sealed class EfSearchCacheTests : IDisposable
{
    private const int MaxSearchesPerUser = 3;
    private const string Alice = "10.0.0.1";
    private const string Bob = "10.0.0.2";

    private readonly SqliteTestDatabase _database = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero));

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task TryGetAsync_ReturnsNullOnMissAndTheGifsInOrderOnHit()
    {
        var gifs = TestGifs.Create("cats", count: 5);
        await AddResultAsync("cats", gifs);

        Assert.Null(await CreateResultCache().TryGetAsync(SearchTerm.Create("dogs"), CancellationToken.None));
        Assert.Equal(gifs, await CreateResultCache().TryGetAsync(SearchTerm.Create("cats"), CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_WhenTheTermIsAlreadyCached_KeepsTheFirstResult()
    {
        var first = TestGifs.Create("first");
        await AddResultAsync("cats", first);

        await AddResultAsync("cats", TestGifs.Create("second"));

        Assert.Equal(first, await CreateResultCache().TryGetAsync(SearchTerm.Create("cats"), CancellationToken.None));
    }

    [Fact]
    public async Task RecordAsync_BeyondTheLimit_EvictsOnlyThatUsersOldestSearch()
    {
        await SearchAsync(Bob, "cats");
        foreach (var term in new[] { "cats", "dogs", "birds", "fish" })
        {
            await SearchAsync(Alice, term);
        }

        Assert.Equal(["fish", "birds", "dogs"], await GetTermsAsync(Alice));
        Assert.Equal(["cats"], await GetTermsAsync(Bob));
    }

    [Fact]
    public async Task RecordAsync_RepeatedSearch_MovesItToTheTopAndProtectsItFromEviction()
    {
        foreach (var term in new[] { "cats", "dogs", "birds", "cats", "fish" })
        {
            await SearchAsync(Alice, term);
        }

        Assert.Equal(["fish", "cats", "birds"], await GetTermsAsync(Alice));
    }

    [Fact]
    public async Task GetAsync_ReturnsEachSearchWithItsGifsInOrder()
    {
        var gifs = TestGifs.Create("cats", count: 4);
        var searchedAt = _time.GetUtcNow().UtcDateTime;
        await SearchAsync(Alice, "cats", gifs);

        var search = Assert.Single(await CreateHistory().GetAsync(Alice, CancellationToken.None));

        Assert.Equal("cats", search.Term);
        Assert.Equal(searchedAt, search.LastSearchedAtUtc);
        Assert.Equal(DateTimeKind.Utc, search.LastSearchedAtUtc.Kind);
        Assert.Equal(gifs, search.Gifs);
    }

    [Fact]
    public async Task PurgeUnreferencedAsync_RemovesOnlyOldUnreferencedResultsWithTheirGifs()
    {
        await AddResultAsync("orphan");
        await SearchAsync(Alice, "referenced");
        _time.Advance(TimeSpan.FromMinutes(30));
        await AddResultAsync("recent orphan");

        var purged = await CreateResultCache().PurgeUnreferencedAsync(
            _time.GetUtcNow().UtcDateTime - TimeSpan.FromMinutes(10), CancellationToken.None);

        Assert.Equal(1, purged);
        await using var dbContext = _database.CreateDbContext();
        Assert.Equal(["recent orphan", "referenced"], await dbContext.SearchResults.Select(r => r.Term).OrderBy(t => t).ToListAsync());
        Assert.Equal(6, await dbContext.SearchResultGifs.CountAsync());
    }

    [Fact]
    public async Task PurgeUnreferencedAsync_SparesResultsReadSinceTheCutoff()
    {
        await AddResultAsync("orphan");
        _time.Advance(TimeSpan.FromMinutes(30));
        await CreateResultCache().TryGetAsync(SearchTerm.Create("orphan"), CancellationToken.None);

        var purged = await CreateResultCache().PurgeUnreferencedAsync(
            _time.GetUtcNow().UtcDateTime - TimeSpan.FromMinutes(10), CancellationToken.None);

        Assert.Equal(0, purged);
    }

    [Fact]
    public async Task EvictedSearch_StopsReferencingItsResult()
    {
        foreach (var term in new[] { "cats", "dogs", "birds", "fish" })
        {
            await SearchAsync(Alice, term);
        }

        _time.Advance(TimeSpan.FromMinutes(30));
        var purged = await CreateResultCache().PurgeUnreferencedAsync(
            _time.GetUtcNow().UtcDateTime - TimeSpan.FromMinutes(10), CancellationToken.None);

        Assert.Equal(1, purged);
        Assert.Null(await CreateResultCache().TryGetAsync(SearchTerm.Create("cats"), CancellationToken.None));
    }

    private EfSearchResultCache CreateResultCache() =>
        new(_database.CreateDbContext(), _time, NullLogger<EfSearchResultCache>.Instance);

    private EfUserSearchHistory CreateHistory() =>
        new(
            _database.CreateDbContext(),
            Microsoft.Extensions.Options.Options.Create(new SearchCacheOptions { MaxSearchesPerUser = MaxSearchesPerUser }),
            _time);

    private Task AddResultAsync(string term, IReadOnlyList<Gif>? gifs = null) =>
        CreateResultCache().AddAsync(SearchTerm.Create(term), gifs ?? TestGifs.Create(term), CancellationToken.None);

    /// <summary>Mimics a search: caches the result if needed, then records it for the user.</summary>
    private async Task SearchAsync(string userKey, string term, IReadOnlyList<Gif>? gifs = null)
    {
        var searchTerm = SearchTerm.Create(term);
        if (await CreateResultCache().TryGetAsync(searchTerm, CancellationToken.None) is null)
        {
            await AddResultAsync(term, gifs);
        }

        await CreateHistory().RecordAsync(userKey, searchTerm, CancellationToken.None);

        // Distinct timestamps keep the expected recency order unambiguous.
        _time.Advance(TimeSpan.FromSeconds(1));
    }

    private async Task<IReadOnlyList<string>> GetTermsAsync(string userKey) =>
        (await CreateHistory().GetAsync(userKey, CancellationToken.None)).Select(s => s.Term).ToArray();
}

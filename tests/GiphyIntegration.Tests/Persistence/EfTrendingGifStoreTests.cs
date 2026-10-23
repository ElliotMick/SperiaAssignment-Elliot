using GiphyIntegration.Core.Models;
using GiphyIntegration.Infrastructure.Persistence;
using GiphyIntegration.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace GiphyIntegration.Tests.Persistence;

public sealed class EfTrendingGifStoreTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task GetLatestAsync_WhenNothingIsStored_ReturnsNull() =>
        Assert.Null(await CreateStore().GetLatestAsync(CancellationToken.None));

    [Fact]
    public async Task ReplaceAsync_KeepsOnlyTheLatestSnapshotAndItsGifs()
    {
        await CreateStore().ReplaceAsync(new TrendingSnapshot(TestGifs.Create("old", count: 5), Now), CancellationToken.None);
        var latest = new TrendingSnapshot(TestGifs.Create("new", count: 2), Now.AddHours(1));

        await CreateStore().ReplaceAsync(latest, CancellationToken.None);

        var stored = await CreateStore().GetLatestAsync(CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal(latest.Gifs, stored.Gifs);
        Assert.Equal(latest.FetchedAtUtc, stored.FetchedAtUtc);
        Assert.Equal(DateTimeKind.Utc, stored.FetchedAtUtc.Kind);

        await using var dbContext = _database.CreateDbContext();
        Assert.Equal(1, await dbContext.TrendingSnapshots.CountAsync());
        Assert.Equal(2, await dbContext.TrendingGifs.CountAsync());
    }

    private EfTrendingGifStore CreateStore() => new(_database.CreateDbContext());
}

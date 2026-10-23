using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace GiphyIntegration.Infrastructure.Persistence;

internal sealed class EfTrendingGifStore(GifCacheDbContext dbContext) : ITrendingGifStore
{
    public async Task<TrendingSnapshot?> GetLatestAsync(CancellationToken cancellationToken)
    {
        var latest = await dbContext.TrendingSnapshots
            .AsNoTracking()
            .OrderByDescending(s => s.FetchedAtUtc)
            .ThenByDescending(s => s.Id)
            .Select(s => new
            {
                s.FetchedAtUtc,
                Gifs = s.Gifs
                    .OrderBy(g => g.Position)
                    .Select(g => new Gif(g.GiphyId, g.Title, g.Url, g.PreviewUrl))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return latest is null ? null : new TrendingSnapshot(latest.Gifs, latest.FetchedAtUtc);
    }

    public async Task ReplaceAsync(TrendingSnapshot snapshot, CancellationToken cancellationToken)
    {
        // One transaction: readers see either the previous snapshot or the new one, never none or both.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var record = new TrendingSnapshotRecord
        {
            FetchedAtUtc = snapshot.FetchedAtUtc,
            Gifs = GifRecord.FromModels<TrendingGifRecord>(snapshot.Gifs),
        };
        dbContext.TrendingSnapshots.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Clean up the previous snapshots; the database cascades the delete to their GIFs.
        await dbContext.TrendingSnapshots
            .Where(s => s.Id != record.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}

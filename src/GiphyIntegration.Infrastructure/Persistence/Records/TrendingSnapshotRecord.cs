namespace GiphyIntegration.Infrastructure.Persistence.Records;

/// <summary>
/// A trending fetch. Only the latest one is kept: older snapshots are removed on every refresh.
/// </summary>
internal sealed class TrendingSnapshotRecord
{
    public int Id { get; set; }

    public DateTime FetchedAtUtc { get; set; }

    public List<TrendingGifRecord> Gifs { get; set; } = [];
}

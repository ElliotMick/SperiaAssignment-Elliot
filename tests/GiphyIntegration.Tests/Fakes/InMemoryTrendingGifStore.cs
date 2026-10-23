using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Tests.Fakes;

internal sealed class InMemoryTrendingGifStore : ITrendingGifStore
{
    public TrendingSnapshot? Snapshot { get; set; }

    public Task<TrendingSnapshot?> GetLatestAsync(CancellationToken cancellationToken) => Task.FromResult(Snapshot);

    public Task ReplaceAsync(TrendingSnapshot snapshot, CancellationToken cancellationToken)
    {
        Snapshot = snapshot;
        return Task.CompletedTask;
    }
}

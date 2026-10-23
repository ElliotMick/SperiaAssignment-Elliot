using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Infrastructure.BackgroundJobs;

/// <summary>
/// Keeps the trending GIFs fresh: restores them on startup, then fetches a new snapshot every
/// <see cref="TrendingOptions.RefreshInterval"/>, replacing the previous one in memory and in the database.
/// </summary>
/// <remarks>
/// Improvement: with several instances each one refreshes on its own. Use leader election or a distributed
/// lock so a single instance pays for the refresh.
/// </remarks>
internal sealed class TrendingRefreshWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TrendingOptions> options,
    ILogger<TrendingRefreshWorker> logger) : PeriodicBackgroundService(scopeFactory, logger)
{
    protected override TimeSpan Interval => options.Value.RefreshInterval;

    protected override Task OnStartupAsync(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<ITrendingGifService>().WarmUpAsync(cancellationToken);

    protected override Task ExecuteOnceAsync(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<ITrendingGifService>().RefreshAsync(cancellationToken);
}

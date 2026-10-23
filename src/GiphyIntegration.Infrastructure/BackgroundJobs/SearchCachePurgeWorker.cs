using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Infrastructure.BackgroundJobs;

/// <summary>
/// Removes cached search results no user references anymore, keeping that work off the request path.
/// </summary>
internal sealed class SearchCachePurgeWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SearchCacheOptions> options,
    TimeProvider timeProvider,
    ILogger<SearchCachePurgeWorker> logger) : PeriodicBackgroundService(scopeFactory, logger)
{
    protected override TimeSpan Interval => options.Value.PurgeInterval;

    protected override async Task ExecuteOnceAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().UtcDateTime - options.Value.UnreferencedRetention;
        var purged = await services.GetRequiredService<ISearchResultCache>().PurgeUnreferencedAsync(cutoff, cancellationToken);
        if (purged > 0)
        {
            Logger.LogInformation("Purged {Count} unreferenced search results", purged);
        }
    }
}

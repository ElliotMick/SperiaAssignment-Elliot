using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GiphyIntegration.Infrastructure.BackgroundJobs;

/// <summary>
/// Template method for recurring jobs: this class owns the schedule, the DI scope of each run and error handling;
/// subclasses only provide the interval and the work to do.
/// </summary>
internal abstract class PeriodicBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected PeriodicBackgroundService(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        _scopeFactory = scopeFactory;
        Logger = logger;
    }

    protected ILogger Logger { get; }

    protected abstract TimeSpan Interval { get; }

    /// <summary>The recurring work, run with services from a dedicated scope.</summary>
    protected abstract Task ExecuteOnceAsync(IServiceProvider services, CancellationToken cancellationToken);

    /// <summary>Hook run once when the service starts. Defaults to a regular run.</summary>
    protected virtual Task OnStartupAsync(IServiceProvider services, CancellationToken cancellationToken) =>
        ExecuteOnceAsync(services, cancellationToken);

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunSafelyAsync(OnStartupAsync, stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSafelyAsync(ExecuteOnceAsync, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Application shutting down.
        }
    }

    private async Task RunSafelyAsync(
        Func<IServiceProvider, CancellationToken, Task> work,
        CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await work(scope.ServiceProvider, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Application shutting down.
        }
        catch (Exception ex)
        {
            // A failed run must not stop the job: log it and try again on the next tick.
            Logger.LogError(ex, "{Job} run failed", GetType().Name);
        }
    }
}

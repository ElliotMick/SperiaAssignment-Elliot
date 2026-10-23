using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GiphyIntegration.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Creates or upgrades the database by applying pending migrations.
    /// </summary>
    /// <remarks>
    /// Convenient for local runs. Improvement: in production, apply migrations from the deployment pipeline
    /// with an EF Core migration bundle (<c>dotnet ef migrations bundle</c>) rather than from every starting instance.
    /// </remarks>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GifCacheDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        // Write-ahead logging lets readers proceed while a write is in progress. Persisted in the database file.
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);
    }
}

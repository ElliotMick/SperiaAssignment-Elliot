using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Concurrency;
using GiphyIntegration.Core.Options;
using GiphyIntegration.Core.Search;
using GiphyIntegration.Core.Trending;
using GiphyIntegration.Infrastructure.BackgroundJobs;
using GiphyIntegration.Infrastructure.Giphy;
using GiphyIntegration.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "GifCache";

    public static IServiceCollection AddGiphyIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<GiphyOptions>(GiphyOptions.SectionName);
        services.AddValidatedOptions<TrendingOptions>(TrendingOptions.SectionName);
        services.AddValidatedOptions<SearchCacheOptions>(SearchCacheOptions.SectionName);

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IKeyedLock, StripedAsyncLock>();

        // Upstream provider
        services.AddHttpClient(GiphyOptions.HttpClientName, (serviceProvider, client) =>
        {
            var giphy = serviceProvider.GetRequiredService<IOptions<GiphyOptions>>().Value;
            client.BaseAddress = new Uri(giphy.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(giphy.TimeoutSeconds);
        });
        services.AddSingleton<IGifSource, GiphyGifSource>();

        // Persistence
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is missing.");
        services.AddDbContext<GifCacheDbContext>((serviceProvider, dbOptions) =>
            dbOptions.UseSqlite(AnchorToContentRoot(connectionString, serviceProvider.GetRequiredService<IHostEnvironment>())));
        services.AddScoped<ISearchResultCache, EfSearchResultCache>();
        services.AddScoped<IUserSearchHistory, EfUserSearchHistory>();
        services.AddScoped<ITrendingGifStore, EfTrendingGifStore>();

        // Application services
        services.AddSingleton<ITrendingGifCache, InMemoryTrendingGifCache>();
        services.AddScoped<ITrendingGifService, TrendingGifService>();
        services.AddScoped<ISearchGifService, SearchGifService>();

        // Background jobs
        services.AddHostedService<TrendingRefreshWorker>();
        services.AddHostedService<SearchCachePurgeWorker>();

        return services;
    }

    private static void AddValidatedOptions<TOptions>(this IServiceCollection services, string sectionName)
        where TOptions : class =>
        services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

    /// <summary>
    /// Resolves a relative SQLite file path against the content root, so the database lands in the same place
    /// whatever the working directory the application is started from.
    /// </summary>
    private static string AnchorToContentRoot(string connectionString, IHostEnvironment environment)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (builder.DataSource != ":memory:" && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(environment.ContentRootPath, builder.DataSource);
        }

        return builder.ToString();
    }
}

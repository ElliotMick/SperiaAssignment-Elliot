using System.ComponentModel.DataAnnotations;

namespace GiphyIntegration.Core.Options;

public sealed class SearchCacheOptions
{
    public const string SectionName = "SearchCache";

    /// <summary>
    /// How many searches are kept per user. Caching saves upstream calls, but too many cached
    /// options degrade the "recent searches" experience.
    /// </summary>
    [Range(1, 100)]
    public int MaxSearchesPerUser { get; set; } = 10;

    /// <summary>How often results no user references anymore are purged.</summary>
    [Range(typeof(TimeSpan), "00:00:10", "1.00:00:00")]
    public TimeSpan PurgeInterval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How long an unreferenced result is kept after its last access before being purged.
    /// Must comfortably exceed a request's duration, so a result being read is never purged under it.
    /// </summary>
    [Range(typeof(TimeSpan), "00:01:00", "7.00:00:00")]
    public TimeSpan UnreferencedRetention { get; set; } = TimeSpan.FromMinutes(10);
}

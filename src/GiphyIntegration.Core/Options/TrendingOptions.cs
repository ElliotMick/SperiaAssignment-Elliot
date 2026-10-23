using System.ComponentModel.DataAnnotations;

namespace GiphyIntegration.Core.Options;

public sealed class TrendingOptions
{
    public const string SectionName = "Trending";

    /// <summary>How often the background worker fetches a new trending snapshot.</summary>
    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(1);
}

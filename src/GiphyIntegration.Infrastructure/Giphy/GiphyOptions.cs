using System.ComponentModel.DataAnnotations;

namespace GiphyIntegration.Infrastructure.Giphy;

public sealed class GiphyOptions
{
    public const string SectionName = "Giphy";
    public const string HttpClientName = "Giphy";

    [Required, Url]
    public string BaseUrl { get; set; } = "https://api.giphy.com/v1/";

    /// <summary>Never commit it: set it through user secrets or the <c>Giphy__ApiKey</c> environment variable.</summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Number of GIFs per request (Giphy beta keys allow up to 50).</summary>
    [Range(1, 50)]
    public int Limit { get; set; } = 25;

    /// <summary>Content rating filter: g, pg, pg-13 or r.</summary>
    [Required]
    public string Rating { get; set; } = "g";

    /// <summary>Search language (ISO 639-1).</summary>
    [Required]
    public string Language { get; set; } = "en";

    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 10;
}

namespace GiphyIntegration.Infrastructure.Persistence.Records;

/// <summary>
/// The upstream result for a search term, shared by every user who searched it.
/// </summary>
internal sealed class SearchResultRecord
{
    public int Id { get; set; }

    /// <summary>Normalized search term (unique).</summary>
    public string Term { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastAccessedAtUtc { get; set; }

    public List<SearchResultGifRecord> Gifs { get; set; } = [];

    public List<UserSearchRecord> UserSearches { get; set; } = [];
}

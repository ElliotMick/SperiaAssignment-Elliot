namespace GiphyIntegration.Infrastructure.Persistence.Records;

/// <summary>
/// Links a user to a search result they looked for. A user keeps only their most recent searches.
/// </summary>
internal sealed class UserSearchRecord
{
    public int Id { get; set; }

    /// <summary>
    /// Identifies the user (currently their IP address).
    /// Improvement: becomes a foreign key to a Users table once real accounts exist.
    /// </summary>
    public string UserKey { get; set; } = string.Empty;

    public int SearchResultId { get; set; }

    public SearchResultRecord? SearchResult { get; set; }

    public DateTime LastSearchedAtUtc { get; set; }
}

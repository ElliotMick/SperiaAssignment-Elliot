using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

/// <summary>
/// The searches cached for each user, limited to the most recent ones.
/// </summary>
public interface IUserSearchHistory
{
    /// <summary>Records that <paramref name="userKey"/> searched <paramref name="term"/>, evicting their oldest searches beyond the limit.</summary>
    Task RecordAsync(string userKey, SearchTerm term, CancellationToken cancellationToken);

    /// <summary>Returns the user's cached searches with their GIFs, most recent first.</summary>
    Task<IReadOnlyList<CachedSearch>> GetAsync(string userKey, CancellationToken cancellationToken);
}

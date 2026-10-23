using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

/// <summary>
/// Search results shared by all users: each term is fetched from the upstream provider at most once
/// for as long as it stays cached.
/// </summary>
public interface ISearchResultCache
{
    /// <summary>Returns the cached GIFs for <paramref name="term"/>, or <c>null</c> on a cache miss.</summary>
    Task<IReadOnlyList<Gif>?> TryGetAsync(SearchTerm term, CancellationToken cancellationToken);

    Task AddAsync(SearchTerm term, IReadOnlyList<Gif> gifs, CancellationToken cancellationToken);

    /// <summary>
    /// Removes results no user references anymore and that haven't been accessed since <paramref name="olderThanUtc"/>.
    /// </summary>
    /// <returns>The number of removed results.</returns>
    Task<int> PurgeUnreferencedAsync(DateTime olderThanUtc, CancellationToken cancellationToken);
}

using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

public interface ISearchGifService
{
    /// <summary>
    /// Searches GIFs for <paramref name="term"/> on behalf of <paramref name="userKey"/>,
    /// calling the upstream provider only for terms that aren't cached yet.
    /// </summary>
    Task<GifQueryResult> SearchAsync(string userKey, SearchTerm term, CancellationToken cancellationToken);
}

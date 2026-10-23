using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Api.Contracts;

/// <param name="MaxEntries">How many searches are cached per user, so clients can trim their own cache alike.</param>
/// <param name="Searches">The user's cached searches, most recent first.</param>
public sealed record CachedSearchesResponse(int MaxEntries, IReadOnlyList<CachedSearchDto> Searches);

/// <param name="Term">The normalized search term.</param>
/// <param name="LastSearchedAtUtc">When the user last searched it.</param>
/// <param name="Gifs">The GIFs found for it, in Giphy's order.</param>
public sealed record CachedSearchDto(string Term, DateTime LastSearchedAtUtc, IReadOnlyList<GifDto> Gifs)
{
    public static CachedSearchDto From(CachedSearch search) =>
        new(search.Term, search.LastSearchedAtUtc, GifDto.From(search.Gifs));
}

using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Api.Contracts;

/// <param name="Query">The normalized search term, or <c>null</c> for trending GIFs.</param>
/// <param name="FromCache">Whether the GIFs were served without calling Giphy.</param>
/// <param name="Count">Number of GIFs returned.</param>
/// <param name="Gifs">The GIFs, in Giphy's order.</param>
public sealed record GifsResponse(string? Query, bool FromCache, int Count, IReadOnlyList<GifDto> Gifs)
{
    public static GifsResponse From(GifQueryResult result, string? query = null) =>
        new(query, result.FromCache, result.Gifs.Count, GifDto.From(result.Gifs));
}

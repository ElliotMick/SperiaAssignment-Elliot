using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Api.Contracts;

/// <param name="Id">Giphy identifier.</param>
/// <param name="Title">GIF title (may be empty).</param>
/// <param name="Url">URL of the original GIF.</param>
/// <param name="PreviewUrl">URL of a lighter rendition, suited for grids.</param>
public sealed record GifDto(string Id, string Title, string Url, string PreviewUrl)
{
    public static GifDto From(Gif gif) => new(gif.Id, gif.Title, gif.Url, gif.PreviewUrl);

    public static IReadOnlyList<GifDto> From(IEnumerable<Gif> gifs) => gifs.Select(From).ToArray();
}

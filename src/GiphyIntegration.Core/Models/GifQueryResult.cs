namespace GiphyIntegration.Core.Models;

/// <summary>
/// The GIFs answering a query and whether they were served without calling the upstream provider.
/// </summary>
public sealed record GifQueryResult(IReadOnlyList<Gif> Gifs, bool FromCache);

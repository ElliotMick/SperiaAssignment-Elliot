namespace GiphyIntegration.Core.Models;

/// <summary>
/// A search kept in a user's cache, together with its GIFs.
/// </summary>
public sealed record CachedSearch(string Term, DateTime LastSearchedAtUtc, IReadOnlyList<Gif> Gifs);

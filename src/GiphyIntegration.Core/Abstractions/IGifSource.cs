using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Core.Abstractions;

/// <summary>
/// An upstream GIF provider (Giphy today). Implementations call the external API directly, without any caching,
/// so another provider can be plugged in without touching the caching logic.
/// </summary>
public interface IGifSource
{
    Task<IReadOnlyList<Gif>> GetTrendingAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Gif>> SearchAsync(SearchTerm term, CancellationToken cancellationToken);
}

namespace GiphyIntegration.Core.Models;

/// <summary>
/// A GIF as exposed by the application, independent of the upstream provider.
/// </summary>
/// <param name="Id">Provider specific identifier.</param>
/// <param name="Title">Human readable title (may be empty).</param>
/// <param name="Url">Direct URL of the original GIF.</param>
/// <param name="PreviewUrl">Direct URL of a lighter rendition, suited for grids and thumbnails.</param>
public sealed record Gif(string Id, string Title, string Url, string PreviewUrl);

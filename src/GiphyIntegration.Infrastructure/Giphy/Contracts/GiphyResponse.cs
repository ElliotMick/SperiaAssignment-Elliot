namespace GiphyIntegration.Infrastructure.Giphy.Contracts;

// Only the fields this application uses are mapped.
// See https://developers.giphy.com/docs/api/schema#gif-object

internal sealed record GiphyResponse(IReadOnlyList<GiphyGif>? Data);

internal sealed record GiphyGif(string? Id, string? Title, GiphyImages? Images);

internal sealed record GiphyImages(GiphyImage? Original, GiphyImage? FixedHeight);

internal sealed record GiphyImage(string? Url);

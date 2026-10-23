using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Infrastructure.Persistence.Records;

/// <summary>
/// Columns shared by every stored GIF. Not mapped as an EF inheritance hierarchy: each subclass gets its own table.
/// </summary>
internal abstract class GifRecord
{
    public int Id { get; set; }

    /// <summary>Rank of the GIF in the upstream response, preserved when reading it back.</summary>
    public int Position { get; set; }

    public string GiphyId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string PreviewUrl { get; set; } = string.Empty;

    public static List<TRecord> FromModels<TRecord>(IEnumerable<Gif> gifs)
        where TRecord : GifRecord, new() =>
        gifs.Select((gif, index) => new TRecord
        {
            Position = index,
            GiphyId = gif.Id,
            Title = gif.Title,
            Url = gif.Url,
            PreviewUrl = gif.PreviewUrl,
        }).ToList();
}

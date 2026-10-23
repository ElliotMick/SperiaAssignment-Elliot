namespace GiphyIntegration.Infrastructure.Persistence.Records;

internal sealed class SearchResultGifRecord : GifRecord
{
    public int SearchResultId { get; set; }

    public SearchResultRecord? SearchResult { get; set; }
}

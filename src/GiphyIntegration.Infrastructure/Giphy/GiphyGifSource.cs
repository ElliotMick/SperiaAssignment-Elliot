using System.Net.Http.Json;
using System.Text.Json;
using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Exceptions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Infrastructure.Giphy.Contracts;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Infrastructure.Giphy;

/// <summary>
/// <see cref="IGifSource"/> calling the Giphy REST API.
/// </summary>
/// <remarks>
/// Uses <see cref="IHttpClientFactory"/> rather than a typed client so it can safely be a singleton.
/// Deliberately no automatic retries, as every call is billed.
/// Improvements: a circuit breaker (Microsoft.Extensions.Http.Resilience) to stop hammering Giphy while it is down,
/// and pagination through the <c>offset</c> parameter.
/// </remarks>
internal sealed class GiphyGifSource(IHttpClientFactory httpClientFactory, IOptions<GiphyOptions> options) : IGifSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public Task<IReadOnlyList<Gif>> GetTrendingAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        return GetGifsAsync(
            "gifs/trending",
            [("api_key", settings.ApiKey), ("limit", settings.Limit.ToString()), ("rating", settings.Rating)],
            cancellationToken);
    }

    public Task<IReadOnlyList<Gif>> SearchAsync(SearchTerm term, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        return GetGifsAsync(
            "gifs/search",
            [
                ("api_key", settings.ApiKey),
                ("q", term.Value),
                ("limit", settings.Limit.ToString()),
                ("rating", settings.Rating),
                ("lang", settings.Language),
            ],
            cancellationToken);
    }

    private async Task<IReadOnlyList<Gif>> GetGifsAsync(
        string path,
        IEnumerable<(string Name, string Value)> query,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(GiphyOptions.HttpClientName);
        var requestUri = path + "?" + string.Join('&', query.Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value)}"));

        // The request URI contains the API key: it must never end up in exception messages or logs.
        try
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new GifSourceException($"Giphy responded with {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            var payload = await response.Content.ReadFromJsonAsync<GiphyResponse>(SerializerOptions, cancellationToken);
            if (payload?.Data is null)
            {
                throw new GifSourceException("Giphy returned a response without data.");
            }

            return payload.Data.Select(ToGif).OfType<Gif>().ToArray();
        }
        catch (HttpRequestException ex)
        {
            throw new GifSourceException("Giphy could not be reached.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new GifSourceException("Giphy did not respond in time.", ex);
        }
        catch (JsonException ex)
        {
            throw new GifSourceException("Giphy returned an unreadable response.", ex);
        }
    }

    private static Gif? ToGif(GiphyGif gif)
    {
        var url = gif.Images?.Original?.Url;
        if (string.IsNullOrEmpty(gif.Id) || string.IsNullOrEmpty(url))
        {
            return null;
        }

        var previewUrl = gif.Images?.FixedHeight?.Url;
        return new Gif(gif.Id, gif.Title ?? string.Empty, url, string.IsNullOrEmpty(previewUrl) ? url : previewUrl);
    }
}

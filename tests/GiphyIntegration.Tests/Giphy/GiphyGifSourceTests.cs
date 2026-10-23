using System.Net;
using System.Text;
using GiphyIntegration.Core.Exceptions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Infrastructure.Giphy;

namespace GiphyIntegration.Tests.Giphy;

public sealed class GiphyGifSourceTests
{
    private const string ApiKey = "test-key";

    private const string SamplePayload =
        """
        {
          "data": [
            {
              "id": "abc",
              "title": "Funny cat",
              "url": "https://giphy.com/gifs/abc",
              "images": {
                "original": { "url": "https://media.giphy.com/abc/giphy.gif" },
                "fixed_height": { "url": "https://media.giphy.com/abc/200.gif" }
              }
            },
            {
              "id": "no-preview",
              "title": null,
              "images": { "original": { "url": "https://media.giphy.com/def/giphy.gif" } }
            },
            {
              "id": "no-image",
              "title": "Skipped",
              "images": {}
            }
          ],
          "pagination": { "total_count": 3, "count": 3, "offset": 0 },
          "meta": { "status": 200, "msg": "OK" }
        }
        """;

    [Fact]
    public async Task SearchAsync_MapsTheGifsAndSkipsThoseWithoutAnImage()
    {
        var source = CreateSource(new StubHandler(HttpStatusCode.OK, SamplePayload));

        var gifs = await source.SearchAsync(SearchTerm.Create("funny cats"), CancellationToken.None);

        Assert.Equal(
            [
                new Gif("abc", "Funny cat", "https://media.giphy.com/abc/giphy.gif", "https://media.giphy.com/abc/200.gif"),
                new Gif("no-preview", "", "https://media.giphy.com/def/giphy.gif", "https://media.giphy.com/def/giphy.gif"),
            ],
            gifs);
    }

    [Fact]
    public async Task SearchAsync_SendsTheConfiguredQueryParameters()
    {
        var handler = new StubHandler(HttpStatusCode.OK, SamplePayload);

        await CreateSource(handler).SearchAsync(SearchTerm.Create("funny cats"), CancellationToken.None);

        Assert.Equal("/v1/gifs/search", handler.LastRequestUri?.AbsolutePath);
        Assert.Equal(
            $"?api_key={ApiKey}&q=funny%20cats&limit=25&rating=g&lang=en",
            handler.LastRequestUri?.Query);
    }

    [Fact]
    public async Task GetTrendingAsync_CallsTheTrendingEndpoint()
    {
        var handler = new StubHandler(HttpStatusCode.OK, SamplePayload);

        var gifs = await CreateSource(handler).GetTrendingAsync(CancellationToken.None);

        Assert.Equal(2, gifs.Count);
        Assert.Equal("/v1/gifs/trending", handler.LastRequestUri?.AbsolutePath);
        Assert.Equal($"?api_key={ApiKey}&limit=25&rating=g", handler.LastRequestUri?.Query);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "{}")]
    [InlineData(HttpStatusCode.TooManyRequests, "{}")]
    [InlineData(HttpStatusCode.OK, "not json")]
    [InlineData(HttpStatusCode.OK, "{}")]
    public async Task GetTrendingAsync_OnAnUnusableResponse_ThrowsWithoutLeakingTheApiKey(HttpStatusCode status, string body)
    {
        var source = CreateSource(new StubHandler(status, body));

        var exception = await Assert.ThrowsAsync<GifSourceException>(() => source.GetTrendingAsync(CancellationToken.None));

        Assert.DoesNotContain(ApiKey, exception.ToString());
    }

    private static GiphyGifSource CreateSource(HttpMessageHandler handler) =>
        new(
            new StubHttpClientFactory(handler),
            Microsoft.Extensions.Options.Options.Create(new GiphyOptions { ApiKey = ApiKey }));

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            new(handler, disposeHandler: false) { BaseAddress = new Uri(new GiphyOptions().BaseUrl) };
    }
}

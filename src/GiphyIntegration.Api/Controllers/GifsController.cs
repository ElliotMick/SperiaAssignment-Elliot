using GiphyIntegration.Api.Contracts;
using GiphyIntegration.Api.Users;
using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;
using GiphyIntegration.Core.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GiphyIntegration.Api.Controllers;

[ApiController]
[Route("api/gifs")]
[Produces("application/json")]
public sealed class GifsController(
    ITrendingGifService trendingService,
    ISearchGifService searchService,
    IUserSearchHistory searchHistory,
    ICurrentUser currentUser,
    IOptions<SearchCacheOptions> searchCacheOptions) : ControllerBase
{
    /// <summary>Returns the trending GIFs of the day.</summary>
    [HttpGet("trending")]
    [ProducesResponseType<GifsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<GifsResponse>> GetTrending(CancellationToken cancellationToken)
    {
        var result = await trendingService.GetAsync(cancellationToken);
        return GifsResponse.From(result);
    }

    /// <summary>Searches GIFs and adds the search to the caller's cached searches.</summary>
    /// <param name="q">The search term (1 to 50 characters).</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    [HttpGet("search")]
    [ProducesResponseType<GifsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<GifsResponse>> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        if (!SearchTerm.TryCreate(q, out var term))
        {
            ModelState.AddModelError(nameof(q), $"A search term of 1 to {SearchTerm.MaxLength} characters is required.");
            return ValidationProblem(ModelState);
        }

        var result = await searchService.SearchAsync(currentUser.Key, term, cancellationToken);
        return GifsResponse.From(result, term.Value);
    }

    /// <summary>
    /// Returns the caller's cached searches with their GIFs, most recent first.
    /// Clients call it to (re)build their local cache, the server being the source of truth.
    /// </summary>
    [HttpGet("searches")]
    [ProducesResponseType<CachedSearchesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CachedSearchesResponse>> GetCachedSearches(CancellationToken cancellationToken)
    {
        var searches = await searchHistory.GetAsync(currentUser.Key, cancellationToken);
        return new CachedSearchesResponse(
            searchCacheOptions.Value.MaxSearchesPerUser,
            searches.Select(CachedSearchDto.From).ToArray());
    }
}

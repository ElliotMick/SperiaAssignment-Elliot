using System.Collections.Concurrent;
using GiphyIntegration.Core.Abstractions;
using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Tests.Fakes;

internal sealed class RecordingUserSearchHistory : IUserSearchHistory
{
    public ConcurrentQueue<(string UserKey, string Term)> Recorded { get; } = new();

    public Task RecordAsync(string userKey, SearchTerm term, CancellationToken cancellationToken)
    {
        Recorded.Enqueue((userKey, term.Value));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CachedSearch>> GetAsync(string userKey, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CachedSearch>>([]);
}

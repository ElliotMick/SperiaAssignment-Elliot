namespace GiphyIntegration.Core.Abstractions;

/// <summary>
/// Asynchronous mutual exclusion per key: callers using the same key run one at a time,
/// callers using different keys can run concurrently.
/// </summary>
public interface IKeyedLock
{
    /// <returns>A handle releasing the lock when disposed.</returns>
    Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken);
}

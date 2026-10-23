using GiphyIntegration.Core.Abstractions;

namespace GiphyIntegration.Core.Concurrency;

/// <summary>
/// <see cref="IKeyedLock"/> backed by a fixed set of semaphores ("lock striping"): a key always maps to
/// the same semaphore, so callers sharing a key are serialized while other keys usually proceed in parallel.
/// </summary>
/// <remarks>
/// Trade-off: two different keys can land on the same stripe and briefly wait on each other. In exchange the
/// memory used is bounded and there is no per-key bookkeeping to clean up.
/// Improvement: this only coordinates a single process. When running several instances, use a distributed lock
/// (e.g. Redis) so instances don't each pay for the same upstream call.
/// </remarks>
public sealed class StripedAsyncLock : IKeyedLock
{
    private readonly SemaphoreSlim[] _stripes;

    public StripedAsyncLock(int stripeCount = 64)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stripeCount);
        _stripes = Enumerable.Range(0, stripeCount).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    }

    public async Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken)
    {
        var stripe = _stripes[GetStripeIndex(key)];
        await stripe.WaitAsync(cancellationToken);
        return new Releaser(stripe);
    }

    private int GetStripeIndex(string key) =>
        (int)((uint)StringComparer.Ordinal.GetHashCode(key) % (uint)_stripes.Length);

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                semaphore.Release();
            }
        }
    }
}

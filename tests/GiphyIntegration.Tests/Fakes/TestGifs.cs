using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Tests.Fakes;

internal static class TestGifs
{
    public static IReadOnlyList<Gif> Create(string prefix, int count = 3) =>
        Enumerable.Range(1, count)
            .Select(i => new Gif(
                $"{prefix}-{i}",
                $"{prefix} {i}",
                $"https://media.giphy.com/{prefix}-{i}.gif",
                $"https://media.giphy.com/{prefix}-{i}-preview.gif"))
            .ToArray();
}

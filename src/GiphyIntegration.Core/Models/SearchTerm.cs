using System.Diagnostics.CodeAnalysis;

namespace GiphyIntegration.Core.Models;

/// <summary>
/// A validated, normalized search term. It is the cache key for search results, so equivalent inputs
/// such as "  Funny   CATS " and "funny cats" share a single cache entry (and a single paid upstream call).
/// </summary>
public sealed record SearchTerm
{
    /// <summary>Maximum length accepted by the Giphy search endpoint.</summary>
    public const int MaxLength = 50;

    private SearchTerm(string value) => Value = value;

    public string Value { get; }

    public static bool TryCreate(string? input, [NotNullWhen(true)] out SearchTerm? term)
    {
        term = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var words = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var normalized = string.Join(' ', words).ToLowerInvariant();
        if (normalized.Length > MaxLength)
        {
            return false;
        }

        term = new SearchTerm(normalized);
        return true;
    }

    public static SearchTerm Create(string? input) =>
        TryCreate(input, out var term)
            ? term
            : throw new ArgumentException($"A search term must contain 1 to {MaxLength} characters.", nameof(input));

    public override string ToString() => Value;
}

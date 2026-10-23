using GiphyIntegration.Core.Models;

namespace GiphyIntegration.Tests.Core;

public sealed class SearchTermTests
{
    [Theory]
    [InlineData("cats", "cats")]
    [InlineData("  Funny   CATS ", "funny cats")]
    [InlineData("dogs\tand\ncats", "dogs and cats")]
    public void TryCreate_NormalizesTheInput(string input, string expected)
    {
        Assert.True(SearchTerm.TryCreate(input, out var term));
        Assert.Equal(expected, term.Value);
    }

    [Fact]
    public void EquivalentInputs_AreEqual() =>
        Assert.Equal(SearchTerm.Create("Funny Cats"), SearchTerm.Create(" funny  cats"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_RejectsBlankInput(string? input) =>
        Assert.False(SearchTerm.TryCreate(input, out _));

    [Fact]
    public void TryCreate_AcceptsUpToTheMaximumLength()
    {
        Assert.True(SearchTerm.TryCreate(new string('a', SearchTerm.MaxLength), out _));
        Assert.False(SearchTerm.TryCreate(new string('a', SearchTerm.MaxLength + 1), out _));
    }

    [Fact]
    public void Create_ThrowsOnInvalidInput() =>
        Assert.Throws<ArgumentException>(() => SearchTerm.Create(" "));
}

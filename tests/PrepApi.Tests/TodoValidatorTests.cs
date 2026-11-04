using PrepApi.Services;

namespace PrepApi.Tests;

public class TodoValidatorTests
{
    [Fact]
    public void Validate_ValidTitle_ReturnsNoErrors()
    {
        // Arrange
        var title = "Buy milk";

        // Act
        var errors = TodoValidator.Validate(title);

        // Assert  re
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_ReturnsRequiredError(string? title)
    {
        var errors = TodoValidator.Validate(title);

        var error = Assert.Single(errors);
        Assert.Contains("required", error);
    }

    [Fact]
    public void Validate_TitleAtMaxLength_IsValid()
    {
        var title = new string('a', TodoValidator.MaxTitleLength);

        var errors = TodoValidator.Validate(title);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_TitleOverMaxLength_ReturnsLengthError()
    {
        var title = new string('a', TodoValidator.MaxTitleLength + 1);

        var errors = TodoValidator.Validate(title);

        var error = Assert.Single(errors);
        Assert.Contains("at most", error);
    }

    [Theory]
    [InlineData("  Buy   milk  ", "Buy milk")]
    [InlineData("Buy\tmilk", "Buy milk")]
    [InlineData("Buy milk", "Buy milk")]
    public void NormalizeTitle_CollapsesWhitespace(string input, string expected)
    {
        var result = TodoValidator.NormalizeTitle(input);

        Assert.Equal(expected, result);
    }
}
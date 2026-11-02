using System.Text.RegularExpressions;

namespace PrepApi.Services;

public static class TodoValidator
{
    public const int MaxTitleLength = 200;

    public static IReadOnlyList<string> Validate(string? title)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
            errors.Add("Title is required.");
        else if (title.Trim().Length > MaxTitleLength)
            errors.Add($"Title must be at most {MaxTitleLength} characters.");

        return errors;
    }

    public static string NormalizeTitle(string title) =>
        Regex.Replace(title.Trim(), @"\s+", " ");
}
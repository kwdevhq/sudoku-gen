using System.Text.RegularExpressions;

namespace SudokuGen.Tests;

// Repository paths named in markdown must exist, so moving files cannot leave stale references behind.
public partial class DocumentedPathTests
{
    [GeneratedRegex(@"`((?:src|docs|scripts|\.github|\.githooks)/[^`\s*]*)`")]
    private static partial Regex CodePathPattern();

    [GeneratedRegex(@"\]\(([^)#\s:]+)\)")]
    private static partial Regex LinkPattern();

    public static IEnumerable<string> MarkdownFiles() =>
        Directory.EnumerateFiles(Repository.Root, "*.md", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(file => Path.GetRelativePath(Repository.Root, file));

    [Test]
    [MethodDataSource(nameof(MarkdownFiles))]
    public async Task ReferencedPaths_Exist(string markdownFile)
    {
        var text = await File.ReadAllTextAsync(Path.Combine(Repository.Root, markdownFile));
        var directory = Path.GetDirectoryName(Path.Combine(Repository.Root, markdownFile))!;

        var missing = CodePathPattern().Matches(text).Select(match => Path.Combine(Repository.Root, match.Groups[1].Value))
            .Concat(LinkPattern().Matches(text).Select(match => Path.Combine(directory, match.Groups[1].Value)))
            .Where(path => !File.Exists(path) && !Directory.Exists(path))
            .ToList();

        await Assert.That(missing).IsEmpty();
    }
}

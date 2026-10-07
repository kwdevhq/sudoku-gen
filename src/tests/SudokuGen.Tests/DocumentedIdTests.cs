using System.Text.RegularExpressions;

namespace SudokuGen.Tests;

// Example ids in docs and the CLI usage text must stay valid ids.
public partial class DocumentedIdTests
{
    [GeneratedRegex("[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{3}")]
    private static partial Regex IdPattern();

    [Test]
    [Arguments("README.md")]
    [Arguments("src/core/SudokuId.cs")]
    [Arguments("docs/adr/0003-reproduce-with-sudoku-id.md")]
    [Arguments("GLOSSARY.md")]
    [Arguments("src/samples/SudokuGen.Cli/Program.cs")]
    public async Task ExampleIds_AreValid(string relativePath)
    {
        var text = await File.ReadAllTextAsync(Path.Combine(Repository.Root, relativePath));
        var matches = IdPattern().Matches(text);

        await Assert.That(matches.Count).IsGreaterThan(0);
        foreach (Match match in matches)
        {
            await Assert.That(SudokuId.TryParse(match.Value, out _)).IsTrue();
        }
    }
}

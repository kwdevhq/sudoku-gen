namespace SudokuGen.Tests;

public class CliTests
{
    private const string KnownId = "2ZKG-VH7J-46X";

    private static readonly string[] HelpArgs = ["--help"];

    private static (int Code, string Output, string Error) Run(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var code = SudokuCli.Run(args, output, error);
        return (code, output.ToString(), error.ToString());
    }

    [Test]
    [Arguments("-h")]
    [Arguments("--help")]
    public async Task Help_PrintsUsage(string flag)
    {
        var (code, output, _) = Run(flag);

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains("sudoku-gen --id <id>");
    }

    [Test]
    public async Task EntryPoint_RunsTheCli()
    {
        var code = typeof(SudokuCli).Assembly.EntryPoint!.Invoke(null, [HelpArgs]);

        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Default_PrintsATextSudokuWithoutSolution()
    {
        var (code, output, _) = Run("--random-seed", "1");

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains("Id: ");
        await Assert.That(output).DoesNotContain("Solution:");
    }

    [Test]
    public async Task WithoutRandomSeed_UsesTheSharedRandom()
    {
        var (code, output, _) = Run();

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains("Id: ");
    }

    [Test]
    public async Task Solution_AddsTheSolutionGrid()
    {
        var (code, output, _) = Run("--random-seed", "1", "--solution");

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains("Solution:");
    }

    [Test]
    [Arguments("-d")]
    [Arguments("--difficulty")]
    public async Task Difficulty_IsAppliedCaseInsensitively(string flag)
    {
        var (code, output, _) = Run(flag, "EXPERT", "--random-seed", "3");

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains("Difficulty: Expert");
    }

    [Test]
    public async Task RandomSeed_MakesRunsReproducible()
    {
        var first = Run("--random-seed", "42", "-d", "hard");
        var second = Run("--random-seed", "42", "-d", "hard");

        await Assert.That(first.Output).IsEqualTo(second.Output);
        await Assert.That(first.Output).Contains($"Id: {KnownId}");
    }

    [Test]
    public async Task Id_RecreatesTheSudoku()
    {
        var generated = Run("--random-seed", "42", "-d", "hard", "--solution");
        var recreated = Run("--id", KnownId, "--solution");

        await Assert.That(recreated.Code).IsEqualTo(0);
        await Assert.That(recreated.Output).IsEqualTo(generated.Output);
    }

    [Test]
    [Arguments("-f")]
    [Arguments("--format")]
    public async Task JsonFormat_PrintsAllFields(string flag)
    {
        var (code, output, _) = Run("--id", KnownId, flag, "JSON");

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains("\"puzzle\"");
        await Assert.That(output).Contains("\"solution\"");
        await Assert.That(output).Contains("\"difficulty\": \"hard\"");
        await Assert.That(output).Contains($"\"id\": \"{KnownId}\"");
    }

    [Test]
    public async Task TextFormat_IsAccepted()
    {
        var (code, output, _) = Run("--id", KnownId, "--format", "text");

        await Assert.That(code).IsEqualTo(0);
        await Assert.That(output).Contains($"Id: {KnownId}");
    }

    [Test]
    [Arguments("--difficulty")]
    [Arguments("--difficulty", "nope")]
    [Arguments("--difficulty", "42")]
    [Arguments("--id")]
    [Arguments("--id", "12-3-457-1022-203456")]
    [Arguments("--id", "0000-0000-001")]
    [Arguments("--random-seed")]
    [Arguments("--random-seed", "abc")]
    [Arguments("--format")]
    [Arguments("--format", "xml")]
    [Arguments("--bogus")]
    public async Task InvalidArguments_FailWithUsage(params string[] args)
    {
        var (code, output, error) = Run(args);

        await Assert.That(code).IsEqualTo(2);
        await Assert.That(output).IsEqualTo(string.Empty);
        await Assert.That(error).Contains("sudoku-gen --id <id>");
    }

    [Test]
    [Arguments("--id", "Missing value for --id")]
    [Arguments("--id", "DHS6-RJN0-C37", "the last character doesn't match the others")]
    [Arguments("--id", "12-3-457-1022-203456", "has 16")]
    public async Task InvalidId_ExplainsTheProblem(params string[] argsAndExpected)
    {
        var (code, _, error) = Run(argsAndExpected[..^1]);

        await Assert.That(code).IsEqualTo(2);
        await Assert.That(error).Contains(argsAndExpected[^1]);
    }

    [Test]
    [Arguments("--difficulty", "easy")]
    [Arguments("--random-seed", "1")]
    public async Task Id_CannotBeCombinedWithDifficultyOrRandomSeed(string flag, string value)
    {
        var (code, _, error) = Run("--id", KnownId, flag, value);

        await Assert.That(code).IsEqualTo(2);
        await Assert.That(error).Contains("--id cannot be combined");
    }
}

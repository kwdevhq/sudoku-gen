namespace SudokuGen.Tests;

public class SudokuGeneratorTests
{
    private static readonly Difficulty[] AllDifficulties = Enum.GetValues<Difficulty>();

    [Test]
    [Arguments(Difficulty.Easy)]
    [Arguments(Difficulty.Medium)]
    [Arguments(Difficulty.Hard)]
    [Arguments(Difficulty.Expert)]
    public async Task Generate_ReturnsRequestedDifficulty(Difficulty difficulty)
    {
        var rng = new Random(1);
        for (var i = 0; i < 50; i++)
        {
            var sudoku = SudokuGenerator.Generate(difficulty, random: rng);

            await Assert.That(sudoku.Difficulty).IsEqualTo(difficulty);
            await Assert.That(sudoku.Id.Difficulty).IsEqualTo(difficulty);
        }
    }

    [Test]
    public async Task Generate_WithoutDifficulty_CoversAllDifficulties()
    {
        var rng = new Random(2);
        var seen = Enumerable.Range(0, 500).Select(_ => SudokuGenerator.Generate(random: rng).Difficulty).ToHashSet();

        await Assert.That(seen.Count).IsEqualTo(AllDifficulties.Length);
    }

    [Test]
    public async Task Generate_UsesSharedRandomByDefault()
    {
        var sudoku = SudokuGenerator.Generate();

        await Assert.That(SudokuChecks.IsValidSolution(sudoku.Solution)).IsTrue();
    }

    [Test]
    public async Task Generate_ProducesValidSolutionsAndConsistentPuzzles()
    {
        var rng = new Random(3);
        for (var i = 0; i < 2000; i++)
        {
            var sudoku = SudokuGenerator.Generate(random: rng);

            await Assert.That(sudoku.Puzzle.Length).IsEqualTo(81);
            await Assert.That(SudokuChecks.IsValidSolution(sudoku.Solution)).IsTrue();
            await Assert.That(SudokuChecks.PuzzleMatchesSolution(sudoku.Puzzle, sudoku.Solution)).IsTrue();
        }
    }

    [Test]
    public async Task Generate_ProducesPuzzlesWithExactlyOneSolution()
    {
        var rng = new Random(4);
        for (var i = 0; i < 200; i++)
        {
            var sudoku = SudokuGenerator.Generate(random: rng);

            await Assert.That(SudokuChecks.CountSolutions(sudoku.Puzzle, limit: 2)).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Generate_PreservesClueCountOfSeed()
    {
        var rng = new Random(5);
        foreach (var difficulty in AllDifficulties)
        {
            var clueCounts = Seeds.For(difficulty).Select(s => s.Puzzle.Count(c => c != '-')).ToHashSet();
            for (var i = 0; i < 100; i++)
            {
                var sudoku = SudokuGenerator.Generate(difficulty, random: rng);
                await Assert.That(clueCounts.Contains(sudoku.Puzzle.Count(c => c != '-'))).IsTrue();
            }
        }
    }

    [Test]
    public async Task Generate_IsDeterministicForSeededRandom()
    {
        var a = SudokuGenerator.Generate(Difficulty.Hard, random: new Random(42));
        var b = SudokuGenerator.Generate(Difficulty.Hard, random: new Random(42));

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Generate_VariesAcrossCalls()
    {
        var rng = new Random(6);
        var distinct = Enumerable.Range(0, 100).Select(_ => SudokuGenerator.Generate(Difficulty.Easy, random: rng).Puzzle).ToHashSet();

        await Assert.That(distinct.Count).IsGreaterThan(90);
    }

    [Test]
    public async Task FromId_RecreatesTheSameSudoku()
    {
        var original = SudokuGenerator.Generate(random: new Random(7));

        var recreated = SudokuGenerator.FromId(SudokuId.Parse(original.Id.ToString()));

        await Assert.That(recreated).IsEqualTo(original);
    }

    [Test]
    public async Task Generate_ThrowsForUndefinedDifficulty() =>
        await Assert.That(() => SudokuGenerator.Generate((Difficulty)42))
            .Throws<ArgumentOutOfRangeException>();

    [Test]
    public async Task FromId_IdentityId_OnlyRelabelsDigits()
    {
        var sudoku = SudokuGenerator.FromId(new SudokuId(0, 0, 0, 0, 0));
        var seed = Seeds.Easy[0];

        await Assert.That(sudoku.Solution).IsEqualTo(string.Concat(seed.Solution.Select(c => (char)('1' + (c - 'a')))));
    }

    [Test]
    public async Task FromId_ExtremeIds_AreValidAndUnique()
    {
        var corners = new[]
        {
            new SudokuId(Seeds.Expert[^1].Id, 3, SudokuId.LineArrangementCount - 1, SudokuId.LineArrangementCount - 1, SudokuId.DigitArrangementCount - 1),
            new SudokuId(0, 1, SudokuId.LineArrangementCount - 1, 0, 0),
            new SudokuId(0, 2, 0, SudokuId.LineArrangementCount - 1, SudokuId.DigitArrangementCount - 1),
        };

        foreach (var id in corners)
        {
            var sudoku = SudokuGenerator.FromId(id);

            await Assert.That(SudokuChecks.IsValidSolution(sudoku.Solution)).IsTrue();
            await Assert.That(SudokuChecks.CountSolutions(sudoku.Puzzle, limit: 2)).IsEqualTo(1);
        }
    }
}

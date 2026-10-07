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
    public async Task Generate_WithId_RecreatesTheSameSudoku()
    {
        var original = SudokuGenerator.Generate(random: new Random(7));

        var recreated = SudokuGenerator.Generate(id: SudokuId.Parse(original.Id.ToString()), random: new Random(999));

        await Assert.That(recreated).IsEqualTo(original);
    }

    [Test]
    public async Task Generate_WithId_DoesNotConsumeRandomness()
    {
        var id = new SudokuId(3, 1, 10, 20, 30);
        var rng = new Random(11);
        var next = new Random(11).Next();

        SudokuGenerator.Generate(id: id, random: rng);

        await Assert.That(rng.Next()).IsEqualTo(next);
    }

    [Test]
    public async Task Generate_WithIdAndMatchingDifficulty_Works()
    {
        var sudoku = SudokuGenerator.Generate(Difficulty.Easy, new SudokuId(0, 0, 0, 0, 0));

        await Assert.That(sudoku.Difficulty).IsEqualTo(Difficulty.Easy);
    }

    [Test]
    public async Task Generate_WithIdAndConflictingDifficulty_Throws() =>
        await Assert.That(() => SudokuGenerator.Generate(Difficulty.Expert, new SudokuId(0, 0, 0, 0, 0)))
            .Throws<ArgumentException>();

    [Test]
    public async Task Generate_ThrowsForUndefinedDifficulty() =>
        await Assert.That(() => SudokuGenerator.Generate((Difficulty)42))
            .Throws<ArgumentOutOfRangeException>();

    [Test]
    public async Task Generate_IdentityId_OnlyRelabelsDigits()
    {
        var sudoku = SudokuGenerator.Generate(id: new SudokuId(0, 0, 0, 0, 0));
        var seed = Seeds.Easy[0];

        await Assert.That(sudoku.Solution).IsEqualTo(string.Concat(seed.Solution.Select(c => (char)('1' + (c - 'a')))));
    }

    [Test]
    public async Task Generate_ExtremeIds_AreValidAndUnique()
    {
        var corners = new[]
        {
            new SudokuId(Seeds.Total - 1, 3, SudokuId.LineArrangementCount - 1, SudokuId.LineArrangementCount - 1, SudokuId.DigitArrangementCount - 1),
            new SudokuId(0, 1, SudokuId.LineArrangementCount - 1, 0, 0),
            new SudokuId(0, 2, 0, SudokuId.LineArrangementCount - 1, SudokuId.DigitArrangementCount - 1),
        };

        foreach (var id in corners)
        {
            var sudoku = SudokuGenerator.Generate(id: id);

            await Assert.That(SudokuChecks.IsValidSolution(sudoku.Solution)).IsTrue();
            await Assert.That(SudokuChecks.CountSolutions(sudoku.Puzzle, limit: 2)).IsEqualTo(1);
        }
    }
}

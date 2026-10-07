namespace SudokuGen.Tests;

public class SeedTests
{
    [Test]
    public async Task ThereAreTenSeedsPerDifficulty()
    {
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            await Assert.That(Seeds.For(difficulty).Length).IsEqualTo(10);
        }
    }

    [Test]
    public async Task AllSeeds_AreWellFormed()
    {
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            foreach (var seed in Seeds.For(difficulty))
            {
                await Assert.That(seed.Puzzle).Matches("^[a-i-]{81}$");
                await Assert.That(seed.Solution).Matches("^[a-i]{81}$");
                await Assert.That(SudokuChecks.IsValidSolution(Digits(seed.Solution))).IsTrue();
                await Assert.That(SudokuChecks.PuzzleMatchesSolution(seed.Puzzle, seed.Solution)).IsTrue();
            }
        }
    }

    [Test]
    public async Task AllSeeds_HaveExactlyOneSolution()
    {
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            foreach (var seed in Seeds.For(difficulty))
            {
                await Assert.That(SudokuChecks.CountSolutions(Digits(seed.Puzzle), limit: 2)).IsEqualTo(1);
            }
        }
    }

    [Test]
    public async Task At_EnumeratesEverySeedOnce()
    {
        var all = Enumerable.Range(0, Seeds.Total).Select(i => Seeds.At(i).Seed.Puzzle).ToHashSet();

        await Assert.That(Seeds.Total).IsEqualTo(40);
        await Assert.That(all.Count).IsEqualTo(40);
        await Assert.That(() => Seeds.At(Seeds.Total)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task For_ThrowsForUndefinedDifficulty()
    {
        await Assert.That(() => Seeds.For((Difficulty)42)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => Seeds.Offset((Difficulty)42)).Throws<ArgumentOutOfRangeException>();
    }

    private static string Digits(string tokens) =>
        string.Create(tokens.Length, tokens, static (span, t) =>
        {
            for (var i = 0; i < t.Length; i++)
            {
                span[i] = t[i] == '-' ? '-' : (char)('1' + (t[i] - 'a'));
            }
        });
}

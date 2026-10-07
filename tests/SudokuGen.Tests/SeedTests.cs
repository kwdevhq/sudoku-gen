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
    public async Task SeedIds_AreUniqueAndFollowDifficultyBlockPlusPosition()
    {
        var ids = new HashSet<int>();
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            var seeds = Seeds.For(difficulty);
            await Assert.That(seeds.Length).IsLessThanOrEqualTo(Seeds.IdsPerDifficulty);

            for (var position = 0; position < seeds.Length; position++)
            {
                var id = seeds[position].Id;
                await Assert.That(id).IsEqualTo(((int)difficulty * Seeds.IdsPerDifficulty) + position);
                await Assert.That(ids.Add(id)).IsTrue();
                await Assert.That(Seeds.Find(id)).IsEqualTo((seeds[position], difficulty));
            }
        }
    }

    [Test]
    public async Task EverySeedId_RoundTripsThroughTextAndGeneratesItsOwnSeed()
    {
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            foreach (var seed in Seeds.For(difficulty))
            {
                var id = new SudokuId(seed.Id, 0, 0, 0, 0);
                var sudoku = SudokuGenerator.FromId(SudokuId.Parse(id.ToString()));

                await Assert.That(sudoku.Id).IsEqualTo(id);
                await Assert.That(sudoku.Difficulty).IsEqualTo(difficulty);
                await Assert.That(sudoku.Puzzle.Count(c => c != '-')).IsEqualTo(seed.Puzzle.Count(c => c != '-'));
            }
        }
    }

    [Test]
    [Arguments(-1)]
    [Arguments(10)]
    [Arguments(63)]
    [Arguments(74)]
    [Arguments(202)]
    [Arguments(256)]
    public async Task UnusedSeedIds_AreRejected(int id)
    {
        await Assert.That(Seeds.Contains(id)).IsFalse();
        await Assert.That(() => Seeds.Find(id)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task DifficultyCount_MatchesTheEnum() =>
        await Assert.That(Enum.GetValues<Difficulty>().Length).IsEqualTo(Seeds.DifficultyCount);

    [Test]
    public async Task For_ThrowsForUndefinedDifficulty() =>
        await Assert.That(() => Seeds.For((Difficulty)42)).Throws<ArgumentOutOfRangeException>();

    private static string Digits(string tokens) =>
        string.Create(tokens.Length, tokens, static (span, t) =>
        {
            for (var i = 0; i < t.Length; i++)
            {
                span[i] = t[i] == '-' ? '-' : (char)('1' + (t[i] - 'a'));
            }
        });
}

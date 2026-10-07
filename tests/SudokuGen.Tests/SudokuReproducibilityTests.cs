namespace SudokuGen.Tests;

public class SudokuReproducibilityTests
{
    private const int Draws = 5;

    private static readonly Difficulty?[] Requests = [null, .. Enum.GetValues<Difficulty>().Cast<Difficulty?>()];

    [Test]
    public async Task Generate_Then_FromId_RecreatesTheSameSudoku()
    {
        var rng = new Random(100);
        foreach (var difficulty in Requests)
        {
            for (var i = 0; i < 300; i++)
            {
                var original = SudokuGenerator.Generate(difficulty, rng);

                await Assert.That(SudokuGenerator.FromId(original.Id)).IsEqualTo(original);
            }
        }
    }

    [Test]
    public async Task Generate_Then_TextId_Then_FromId_RecreatesTheSameSudoku()
    {
        var rng = new Random(101);
        for (var i = 0; i < 1000; i++)
        {
            var original = SudokuGenerator.Generate(random: rng);
            var text = original.Id.ToString();

            var parsed = SudokuId.Parse(text);
            var recreated = SudokuGenerator.FromId(parsed);

            await Assert.That(parsed).IsEqualTo(original.Id);
            await Assert.That(recreated).IsEqualTo(original);
            await Assert.That(recreated.Id.ToString()).IsEqualTo(text);
        }
    }

    [Test]
    public async Task FromId_Then_Generate_Path_IsStable()
    {
        // The id embedded in a recreated sudoku is the id it was created from, so ids are fixed points.
        var rng = new Random(102);
        for (var i = 0; i < 300; i++)
        {
            var id = RandomComponents(rng);
            var sudoku = SudokuGenerator.FromId(id);

            await Assert.That(sudoku.Id).IsEqualTo(id);
            await Assert.That(sudoku.Difficulty).IsEqualTo(id.Difficulty);
            await Assert.That(SudokuGenerator.FromId(sudoku.Id)).IsEqualTo(sudoku);
        }
    }

    [Test]
    public async Task TextId_RoundTrips_ForRandomComponents()
    {
        var rng = new Random(103);
        for (var i = 0; i < 20_000; i++)
        {
            var id = RandomComponents(rng);

            await Assert.That(SudokuId.TryParse(id.ToString(), out var parsed)).IsTrue();
            await Assert.That(parsed).IsEqualTo(id);
        }
    }

    [Test]
    public async Task TextId_RoundTrips_ForEverySeedAtEveryRotationAndExtremeArrangement()
    {
        var lines = new[] { 0, 1, SudokuId.LineArrangementCount - 1 };
        var digits = new[] { 0, 1, SudokuId.DigitArrangementCount - 1 };

        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            foreach (var seed in Seeds.For(difficulty))
            {
                for (var rotation = 0; rotation < SudokuId.RotationCount; rotation++)
                {
                    foreach (var rows in lines)
                    {
                        foreach (var columns in lines)
                        {
                            foreach (var digit in digits)
                            {
                                var id = new SudokuId(seed.Id, rotation, rows, columns, digit);

                                await Assert.That(SudokuId.Parse(id.ToString())).IsEqualTo(id);
                            }
                        }
                    }
                }
            }
        }
    }

    [Test]
    public async Task TextIds_AreInjective()
    {
        var rng = new Random(104);
        var byText = new Dictionary<string, SudokuId>();
        for (var i = 0; i < 20_000; i++)
        {
            var id = RandomComponents(rng);
            var text = id.ToString();

            if (byText.TryGetValue(text, out var existing))
            {
                await Assert.That(existing).IsEqualTo(id);
            }

            byText[text] = id;
        }
    }

    [Test]
    public async Task SameSeed_OnDifferentRandomInstances_ProducesTheSameSequence()
    {
        foreach (var difficulty in Requests)
        {
            var a = new Random(42);
            var b = new Random(42);

            for (var i = 0; i < 100; i++)
            {
                await Assert.That(SudokuGenerator.Generate(difficulty, b)).IsEqualTo(SudokuGenerator.Generate(difficulty, a));
            }
        }
    }

    [Test]
    public async Task SameRandomInstance_ProducesDifferentSudokusOnConsecutiveCalls()
    {
        var rng = new Random(42);

        var first = SudokuGenerator.Generate(random: rng);
        var second = SudokuGenerator.Generate(random: rng);

        await Assert.That(second.Id).IsNotEqualTo(first.Id);
    }

    [Test]
    public async Task SameRandomState_ReplayedViaFreshInstance_ProducesTheSameSudoku()
    {
        var rng = new Random(7);
        var first = SudokuGenerator.Generate(Difficulty.Medium, rng);
        var second = SudokuGenerator.Generate(Difficulty.Medium, rng);

        var replay = new Random(7);

        await Assert.That(SudokuGenerator.Generate(Difficulty.Medium, replay)).IsEqualTo(first);
        await Assert.That(SudokuGenerator.Generate(Difficulty.Medium, replay)).IsEqualTo(second);
    }

    [Test]
    public async Task DifferentSeeds_ProduceDifferentSequences()
    {
        var a = new Random(1);
        var b = new Random(2);

        var idsA = Enumerable.Range(0, 20).Select(_ => SudokuGenerator.Generate(random: a).Id).ToArray();
        var idsB = Enumerable.Range(0, 20).Select(_ => SudokuGenerator.Generate(random: b).Id).ToArray();

        await Assert.That(idsA.SequenceEqual(idsB)).IsFalse();
    }

    [Test]
    public async Task Generate_ConsumesFiveDrawsWithADifficultyAndSixWithout()
    {
        foreach (var difficulty in Requests)
        {
            var expected = difficulty is null ? Draws + 1 : Draws;
            var used = new CountingRandom(5);
            var untouched = new Random(5);

            SudokuGenerator.Generate(difficulty, used);

            await Assert.That(used.Calls).IsEqualTo(expected);
            for (var i = 0; i < expected; i++)
            {
                untouched.Next();
            }

            await Assert.That(used.Next()).IsEqualTo(untouched.Next());
        }
    }

    [Test]
    public async Task ExplicitDifficulty_MatchesTheRandomPick_WhenItIsTheSameDifficulty()
    {
        var picked = new HashSet<Difficulty>();
        for (var seed = 0; seed < 500; seed++)
        {
            var random = SudokuGenerator.Generate(random: new Random(seed));
            var explicitly = SudokuGenerator.Generate(random.Difficulty, new Random(seed));

            picked.Add(random.Difficulty);
            await Assert.That(explicitly).IsEqualTo(random);
        }

        await Assert.That(picked.Count).IsEqualTo(Enum.GetValues<Difficulty>().Length);
    }

    [Test]
    public async Task ExplicitDifficulty_ChangesOnlyTheSeed_NotTheTransformations()
    {
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var anyDifficulty = SudokuGenerator.Generate(random: new Random(seed)).Id;
                var forced = SudokuGenerator.Generate(difficulty, new Random(seed)).Id;

                await Assert.That((forced.Rotation, forced.Rows, forced.Columns, forced.Digits))
                    .IsEqualTo((anyDifficulty.Rotation, anyDifficulty.Rows, anyDifficulty.Columns, anyDifficulty.Digits));
                await Assert.That(forced.Seed % Seeds.IdsPerDifficulty).IsEqualTo(anyDifficulty.Seed % Seeds.IdsPerDifficulty);
            }
        }
    }

    [Test]
    public async Task FromId_DoesNotNeedOrConsumeRandomness()
    {
        var before = new Random(11).Next();
        var rng = new Random(11);

        SudokuGenerator.FromId(new SudokuId(3, 1, 10, 20, 30));

        await Assert.That(rng.Next()).IsEqualTo(before);
    }

    [Test]
    public async Task GoldenVectors_PinTheSeededOutput()
    {
        // Guards the order and meaning of the five draws, and the id encoding. Seeded System.Random is stable across .NET versions.
        var hard = SudokuGenerator.Generate(Difficulty.Hard, new Random(42));
        var any = SudokuGenerator.Generate(random: new Random(42));

        await Assert.That(hard.Id.ToString()).IsEqualTo("2ZKG-VH7J-46X");
        await Assert.That(any.Id.ToString()).IsEqualTo("2ZKG-VH7J-26B");
        await Assert.That(any.Difficulty).IsEqualTo(Difficulty.Medium);
        await Assert.That(SudokuGenerator.FromId(SudokuId.Parse("2ZKG-VH7J-46X"))).IsEqualTo(hard);
    }

    [Test]
    public async Task Generate_EventuallyReachesEverySeed()
    {
        var rng = new Random(105);
        var any = Enumerable.Range(0, 3000).Select(_ => SudokuGenerator.Generate(random: rng).Id.Seed).ToHashSet();

        await Assert.That(any.Count).IsEqualTo(Seeds.Total);

        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            var perDifficulty = Enumerable.Range(0, 500).Select(_ => SudokuGenerator.Generate(difficulty, rng).Id.Seed).ToHashSet();

            await Assert.That(perDifficulty.SetEquals(Seeds.For(difficulty).Select(s => s.Id))).IsTrue();
        }
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    public async Task EveryRotation_YieldsValidUniquelySolvableSudokus(int rotation)
    {
        var rng = new Random(106 + rotation);
        for (var i = 0; i < 100; i++)
        {
            var seed = Seeds.At(rng.Next(Seeds.Total)).Seed.Id;
            var id = new SudokuId(
                seed,
                rotation,
                rng.Next(SudokuId.LineArrangementCount),
                rng.Next(SudokuId.LineArrangementCount),
                rng.Next(SudokuId.DigitArrangementCount));

            var sudoku = SudokuGenerator.FromId(id);

            await Assert.That(SudokuChecks.IsValidSolution(sudoku.Solution)).IsTrue();
            await Assert.That(SudokuChecks.PuzzleMatchesSolution(sudoku.Puzzle, sudoku.Solution)).IsTrue();
            await Assert.That(SudokuChecks.CountSolutions(sudoku.Puzzle, limit: 2)).IsEqualTo(1);
        }
    }

    [Test]
    public async Task DefaultId_IsTheValidIdentityId()
    {
        var sudoku = SudokuGenerator.FromId(default);

        await Assert.That(sudoku.Id).IsEqualTo(new SudokuId(0, 0, 0, 0, 0));
        await Assert.That(sudoku.Id.ToString()).IsEqualTo("0000-0000-000");
    }

    private static SudokuId RandomComponents(Random rng) => new(
        Seeds.At(rng.Next(Seeds.Total)).Seed.Id,
        rng.Next(SudokuId.RotationCount),
        rng.Next(SudokuId.LineArrangementCount),
        rng.Next(SudokuId.LineArrangementCount),
        rng.Next(SudokuId.DigitArrangementCount));

    private sealed class CountingRandom(int seed) : Random(seed)
    {
        public int Calls { get; private set; }

        public override int Next()
        {
            Calls++;
            return base.Next();
        }

        public override int Next(int maxValue)
        {
            Calls++;
            return base.Next(maxValue);
        }
    }
}

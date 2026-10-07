namespace SudokuGen;

/// <summary>A known, solvable puzzle/solution pair using tokens 'a'-'i' for digits and '-' for empty cells.</summary>
/// <param name="Id">Stable identifier: difficulty * <see cref="Seeds.IdsPerDifficulty"/> + position within the difficulty's array.</param>
/// <param name="Puzzle">The puzzle cells.</param>
/// <param name="Solution">The solution cells.</param>
internal readonly record struct Seed(int Id, string Puzzle, string Solution);

internal static partial class Seeds
{
    /// <summary>Seed ids reserved per difficulty, so appending seeds to one difficulty never shifts another's ids.</summary>
    internal const int IdsPerDifficulty = 64;

    /// <summary>Number of difficulties; each owns a block of <see cref="IdsPerDifficulty"/> seed ids.</summary>
    internal const int DifficultyCount = 4;

    /// <summary>Size of the seed id space (every difficulty's block).</summary>
    internal const int IdCount = DifficultyCount * IdsPerDifficulty;

    internal static int Total => Easy.Length + Medium.Length + Hard.Length + Expert.Length;

    internal static bool Contains(int id) =>
        (uint)id < IdCount && id % IdsPerDifficulty < For((Difficulty)(id / IdsPerDifficulty)).Length;

    /// <summary>Finds the seed with the given id.</summary>
    internal static (Seed Seed, Difficulty Difficulty) Find(int id)
    {
        if (!Contains(id))
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown seed id.");
        }

        var difficulty = (Difficulty)(id / IdsPerDifficulty);
        return (For(difficulty)[id % IdsPerDifficulty], difficulty);
    }

    internal static Seed[] For(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => Easy,
        Difficulty.Medium => Medium,
        Difficulty.Hard => Hard,
        Difficulty.Expert => Expert,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown difficulty."),
    };

    /// <summary>Picks the seed at the dense <paramref name="index"/> (0 to <see cref="Total"/> - 1) across all difficulties; not an id.</summary>
    internal static (Seed Seed, Difficulty Difficulty) At(int index)
    {
        foreach (var difficulty in Enum.GetValues<Difficulty>())
        {
            var seeds = For(difficulty);
            if (index < seeds.Length)
            {
                return (seeds[index], difficulty);
            }

            index -= seeds.Length;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }
}

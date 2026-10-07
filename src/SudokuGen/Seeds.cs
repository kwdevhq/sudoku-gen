namespace SudokuGen;

/// <summary>A known, solvable puzzle/solution pair using tokens 'a'-'i' for digits and '-' for empty cells.</summary>
internal readonly record struct Seed(string Puzzle, string Solution);

internal static partial class Seeds
{
    internal static int Total => Easy.Length + Medium.Length + Hard.Length + Expert.Length;

    /// <summary>Index of the first seed of <paramref name="difficulty"/> across all difficulties.</summary>
    internal static int Offset(Difficulty difficulty)
    {
        var offset = 0;
        foreach (var d in Enum.GetValues<Difficulty>())
        {
            if (d == difficulty)
            {
                return offset;
            }

            offset += For(d).Length;
        }

        throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown difficulty.");
    }

    internal static Seed[] For(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => Easy,
        Difficulty.Medium => Medium,
        Difficulty.Hard => Hard,
        Difficulty.Expert => Expert,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown difficulty."),
    };

    /// <summary>Picks the seed at <paramref name="index"/> across all difficulties.</summary>
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

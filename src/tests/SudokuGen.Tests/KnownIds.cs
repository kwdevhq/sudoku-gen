namespace SudokuGen.Tests;

/// <summary>Golden ids for seeded <see cref="Random"/> runs; a change in draw order is a one-place edit here.</summary>
internal static class KnownIds
{
    /// <summary><c>Generate(Difficulty.Hard, new Random(42))</c>.</summary>
    public const string Random42Hard = "2ZKG-VH7J-46X";

    /// <summary><c>Generate(null, new Random(42))</c>; the random pick is Medium.</summary>
    public const string Random42Any = "2ZKG-VH7J-26B";
}

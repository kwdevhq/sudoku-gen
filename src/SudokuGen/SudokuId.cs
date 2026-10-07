using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SudokuGen;

/// <summary>
/// Identifies exactly one sudoku: the seed it was derived from plus every transformation applied to it.
/// Passing the same id to <see cref="SudokuGenerator.Generate"/> always yields the same sudoku.
/// The text form is <c>seed-rotation-rows-columns-digits</c> (e.g. <c>12-3-0457-1022-203456</c>) and is a stable, persisted format.
/// </summary>
public readonly record struct SudokuId
{
    /// <summary>Number of rotations (0°, 90°, 180°, 270°).</summary>
    public const int RotationCount = 4;

    /// <summary>Number of row (or column) arrangements: band order (3!) x row order within each of the 3 bands (3!^3).</summary>
    public const int LineArrangementCount = 6 * 6 * 6 * 6;

    /// <summary>Number of digit relabellings (9!).</summary>
    public const int DigitArrangementCount = 362_880;

    /// <summary>Creates an id, validating every component.</summary>
    /// <param name="seed">Index of the seed across all difficulties (easy, medium, hard, expert, in that order).</param>
    /// <param name="rotation">0-3, clockwise quarter turns.</param>
    /// <param name="rows">Row arrangement, 0 to <see cref="LineArrangementCount"/> - 1.</param>
    /// <param name="columns">Column arrangement, 0 to <see cref="LineArrangementCount"/> - 1.</param>
    /// <param name="digits">Digit relabelling, 0 to <see cref="DigitArrangementCount"/> - 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">A component is out of range.</exception>
    public SudokuId(int seed, int rotation, int rows, int columns, int digits)
    {
        Seed = InRange(seed, Seeds.Total, nameof(seed));
        Rotation = InRange(rotation, RotationCount, nameof(rotation));
        Rows = InRange(rows, LineArrangementCount, nameof(rows));
        Columns = InRange(columns, LineArrangementCount, nameof(columns));
        Digits = InRange(digits, DigitArrangementCount, nameof(digits));
    }

    /// <summary>Index of the seed across all difficulties.</summary>
    public int Seed { get; }

    /// <summary>Clockwise quarter turns (0-3).</summary>
    public int Rotation { get; }

    /// <summary>Row arrangement.</summary>
    public int Rows { get; }

    /// <summary>Column arrangement.</summary>
    public int Columns { get; }

    /// <summary>Digit relabelling.</summary>
    public int Digits { get; }

    /// <summary>The difficulty of the seed, hence of the sudoku.</summary>
    public Difficulty Difficulty => Seeds.At(Seed).Difficulty;

    /// <summary>Parses the text form produced by <see cref="ToString"/>.</summary>
    /// <exception cref="FormatException">The text is not a valid id.</exception>
    public static SudokuId Parse(string text) =>
        TryParse(text, out var id) ? id : throw new FormatException($"'{text}' is not a valid sudoku id.");

    /// <summary>Tries to parse the text form produced by <see cref="ToString"/>.</summary>
    public static bool TryParse([NotNullWhen(true)] string? text, out SudokuId id)
    {
        id = default;
        var parts = text?.Split('-');
        if (parts is not { Length: 5 })
        {
            return false;
        }

        Span<int> values = stackalloc int[5];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out values[i]))
            {
                return false;
            }
        }

        if (values[0] >= Seeds.Total || values[1] >= RotationCount || values[2] >= LineArrangementCount
            || values[3] >= LineArrangementCount || values[4] >= DigitArrangementCount)
        {
            return false;
        }

        id = new SudokuId(values[0], values[1], values[2], values[3], values[4]);
        return true;
    }

    /// <summary>Returns the stable text form <c>seed-rotation-rows-columns-digits</c>.</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Seed}-{Rotation}-{Rows}-{Columns}-{Digits}");

    private static int InRange(int value, int count, string name) =>
        (uint)value < (uint)count
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"Must be between 0 and {count - 1}.");
}

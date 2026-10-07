using System.Diagnostics.CodeAnalysis;

namespace SudokuGen;

/// <summary>
/// Identifies exactly one sudoku: the seed it was derived from plus every transformation applied to it.
/// Passing the same id to <see cref="SudokuGenerator.FromId"/> always yields the same sudoku.
/// The text form is 11 Crockford Base32 characters, the last being a check character, grouped for reading
/// (e.g. <c>DHS6-RJN0-C38</c>). Parsing ignores hyphens and case and reads <c>O</c> as <c>0</c> and <c>I</c>/<c>L</c> as <c>1</c>.
/// It is a stable, persisted format.
/// </summary>
public readonly record struct SudokuId
{
    /// <summary>Number of rotations (0°, 90°, 180°, 270°).</summary>
    public const int RotationCount = 4;

    /// <summary>Number of row (or column) arrangements: band order (3!) x row order within each of the 3 bands (3!^3).</summary>
    public const int LineArrangementCount = 6 * 6 * 6 * 6;

    /// <summary>Number of digit relabellings (9!).</summary>
    public const int DigitArrangementCount = 362_880;

    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private const int PayloadLength = 10;
    private const int TextLength = PayloadLength + 1;
    private const int CheckModulus = 31;

    /// <summary>Creates an id, validating every component.</summary>
    /// <param name="seed">Stable id of the seed (difficulty * 64 + position within the difficulty); must refer to an existing seed.</param>
    /// <param name="rotation">0-3, clockwise quarter turns.</param>
    /// <param name="rows">Row arrangement, 0 to <see cref="LineArrangementCount"/> - 1.</param>
    /// <param name="columns">Column arrangement, 0 to <see cref="LineArrangementCount"/> - 1.</param>
    /// <param name="digits">Digit relabelling, 0 to <see cref="DigitArrangementCount"/> - 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">A component is out of range or <paramref name="seed"/> does not exist.</exception>
    public SudokuId(int seed, int rotation, int rows, int columns, int digits)
    {
        Seed = Seeds.Contains(seed)
            ? seed
            : throw new ArgumentOutOfRangeException(nameof(seed), seed, "Unknown seed id.");
        Rotation = InRange(rotation, RotationCount, nameof(rotation));
        Rows = InRange(rows, LineArrangementCount, nameof(rows));
        Columns = InRange(columns, LineArrangementCount, nameof(columns));
        Digits = InRange(digits, DigitArrangementCount, nameof(digits));
    }

    /// <summary>Stable id of the seed (difficulty * 64 + position within the difficulty).</summary>
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
    public Difficulty Difficulty => Seeds.Find(Seed).Difficulty;

    /// <summary>Parses the text form produced by <see cref="ToString"/>.</summary>
    /// <exception cref="FormatException">The text is not a valid id; the message says why.</exception>
    public static SudokuId Parse(string text) =>
        Validate(text, out var id) is { } problem
            ? throw new FormatException($"'{text}' is not a valid sudoku id: {problem}")
            : id;

    /// <summary>Tries to parse the text form produced by <see cref="ToString"/>.</summary>
    public static bool TryParse([NotNullWhen(true)] string? text, out SudokuId id) => Validate(text, out id) is null;

    // Returns null when the text is a valid id, otherwise what is wrong with it.
    private static string? Validate(string? text, out SudokuId id)
    {
        id = default;
        if (text is null)
        {
            return "no id was given.";
        }

        Span<int> symbols = stackalloc int[TextLength];
        var count = 0;
        foreach (var c in text)
        {
            if (c == '-')
            {
                continue;
            }

            var symbol = SymbolValue(c);
            if (symbol < 0)
            {
                return $"'{c}' is not a valid character. Ids use the digits 0-9 and the letters A-Z (except U).";
            }

            if (count == TextLength)
            {
                return WrongLength(text);
            }

            symbols[count++] = symbol;
        }

        if (count != TextLength)
        {
            return WrongLength(text);
        }

        ulong value = 0;
        var sum = 0;
        for (var i = 0; i < PayloadLength; i++)
        {
            value = (value << 5) | (uint)symbols[i];
            sum += (i + 1) * symbols[i];
        }

        if (symbols[PayloadLength] != sum % CheckModulus)
        {
            return "the last character doesn't match the others, so the id contains a typo.";
        }

        return TryUnpack(value, out id) ? null : "no sudoku belongs to this id.";
    }

    private static string WrongLength(string text) =>
        $"an id has {TextLength} characters (hyphens don't count), but this one has {text.Count(c => c != '-')}.";

    /// <summary>Returns the stable text form, e.g. <c>DHS6-RJN0-C38</c>.</summary>
    public override string ToString()
    {
        var value = Pack();
        Span<char> text = stackalloc char[TextLength + 2];
        var sum = 0;
        for (var i = PayloadLength - 1; i >= 0; i--)
        {
            var symbol = (int)(value & 31);
            value >>= 5;
            text[Position(i)] = Alphabet[symbol];
            sum += (i + 1) * symbol;
        }

        text[Position(PayloadLength)] = Alphabet[sum % CheckModulus];
        text[4] = '-';
        text[9] = '-';
        return new string(text);
    }

    // Groups of four, separated by a hyphen.
    private static int Position(int symbolIndex) => symbolIndex + (symbolIndex / 4);

    private static int SymbolValue(char c) => char.ToUpperInvariant(c) switch
    {
        'O' => 0,
        'I' or 'L' => 1,
        var upper => Alphabet.IndexOf(upper, StringComparison.Ordinal),
    };

    // Mixed radix with the seed id least significant; the radices are fixed, so adding seeds never changes existing ids.
    private ulong Pack() =>
        (ulong)Seed + (Seeds.IdCount * ((ulong)Rotation + (RotationCount * ((ulong)Rows
            + (LineArrangementCount * ((ulong)Columns + ((ulong)LineArrangementCount * (ulong)Digits)))))));

    private static bool TryUnpack(ulong value, out SudokuId id)
    {
        id = default;
        var seed = (int)(value % Seeds.IdCount);
        value /= Seeds.IdCount;
        var rotation = (int)(value % RotationCount);
        value /= RotationCount;
        var rows = (int)(value % LineArrangementCount);
        value /= LineArrangementCount;
        var columns = (int)(value % LineArrangementCount);
        value /= LineArrangementCount;

        if (value >= DigitArrangementCount || !Seeds.Contains(seed))
        {
            return false;
        }

        id = new SudokuId(seed, rotation, rows, columns, (int)value);
        return true;
    }

    private static int InRange(int value, int count, string name) =>
        (uint)value < (uint)count
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"Must be between 0 and {count - 1}.");
}

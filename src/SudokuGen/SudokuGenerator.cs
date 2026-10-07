namespace SudokuGen;

/// <summary>
/// Generates sudokus by transforming known seed puzzles (rotation, band/stack/row/column shuffles and digit relabelling).
/// Port of petewritescode/sudoku-gen.
/// </summary>
public static class SudokuGenerator
{
    private const int Size = 9;
    private const int Cells = Size * Size;
    private const int Box = 3;

    /// <summary>Generates a sudoku.</summary>
    /// <param name="difficulty">The desired difficulty, or <see langword="null"/> for any difficulty.</param>
    /// <param name="id">
    /// Recreates exactly the sudoku with this id; no randomness is used. When <see langword="null"/> a random id is chosen
    /// (matching <paramref name="difficulty"/> if given).
    /// </param>
    /// <param name="random">Randomness source for choosing an id; defaults to <see cref="Random.Shared"/>. Ignored when <paramref name="id"/> is given.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="difficulty"/> is not a defined <see cref="Difficulty"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="id"/> and <paramref name="difficulty"/> are both given but disagree.</exception>
    public static Sudoku Generate(Difficulty? difficulty = null, SudokuId? id = null, Random? random = null)
    {
        if (difficulty is { } requested && !Enum.IsDefined(requested))
        {
            throw new ArgumentOutOfRangeException(nameof(difficulty), requested, "Unknown difficulty.");
        }

        if (id is { } given)
        {
            if (difficulty is { } expected && given.Difficulty != expected)
            {
                throw new ArgumentException($"Sudoku id {given} is {given.Difficulty}, not {expected}.", nameof(id));
            }

            return Create(given);
        }

        return Create(RandomId(difficulty, random ?? Random.Shared));
    }

    private static SudokuId RandomId(Difficulty? difficulty, Random random)
    {
        var seed = difficulty is { } d
            ? Seeds.Offset(d) + random.Next(Seeds.For(d).Length)
            : random.Next(Seeds.Total);

        return new SudokuId(
            seed,
            random.Next(SudokuId.RotationCount),
            random.Next(SudokuId.LineArrangementCount),
            random.Next(SudokuId.LineArrangementCount),
            random.Next(SudokuId.DigitArrangementCount));
    }

    private static Sudoku Create(SudokuId id)
    {
        var (seed, difficulty) = Seeds.At(id.Seed);

        Span<int> layout = stackalloc int[Cells];
        BuildLayout(layout, id);

        Span<char> tokenMap = stackalloc char[Size];
        BuildTokenMap(tokenMap, id.Digits);

        return new Sudoku(Apply(seed.Puzzle, layout, tokenMap), Apply(seed.Solution, layout, tokenMap), difficulty, id);
    }

    private static string Apply(string seed, ReadOnlySpan<int> layout, ReadOnlySpan<char> tokenMap)
    {
        Span<char> result = stackalloc char[Cells];
        for (var i = 0; i < Cells; i++)
        {
            var token = seed[layout[i]];
            result[i] = token == '-' ? '-' : tokenMap[token - 'a'];
        }

        return new string(result);
    }

    // layout[cell] is the index of the seed cell that ends up at `cell`.
    private static void BuildLayout(Span<int> layout, SudokuId id)
    {
        Span<int> rows = stackalloc int[Size];
        Span<int> columns = stackalloc int[Size];
        BuildLineArrangement(rows, id.Rows);
        BuildLineArrangement(columns, id.Columns);

        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                var r = rows[row];
                var c = columns[column];
                layout[(row * Size) + column] = id.Rotation switch
                {
                    0 => (r * Size) + c,
                    1 => ((Size - 1 - c) * Size) + r,
                    2 => ((Size - 1 - r) * Size) + (Size - 1 - c),
                    _ => (c * Size) + (Size - 1 - r),
                };
            }
        }
    }

    // arrangement = band order (3!) + 6 * (order within band 0 + 6 * (order within band 1 + 6 * order within band 2)).
    private static void BuildLineArrangement(Span<int> lines, int arrangement)
    {
        Span<int> bands = stackalloc int[Box];
        DecodePermutation(bands, arrangement % 6);
        arrangement /= 6;

        Span<int> inner = stackalloc int[Box];
        for (var band = 0; band < Box; band++)
        {
            DecodePermutation(inner, arrangement % 6);
            arrangement /= 6;

            for (var i = 0; i < Box; i++)
            {
                lines[(band * Box) + i] = (bands[band] * Box) + inner[i];
            }
        }
    }

    private static void BuildTokenMap(Span<char> tokenMap, int arrangement)
    {
        Span<int> permutation = stackalloc int[Size];
        DecodePermutation(permutation, arrangement);

        for (var i = 0; i < Size; i++)
        {
            tokenMap[i] = (char)('1' + permutation[i]);
        }
    }

    // Factorial number system: index 0 is the identity permutation.
    private static void DecodePermutation(Span<int> result, int index)
    {
        Span<int> remaining = stackalloc int[result.Length];
        for (var i = 0; i < remaining.Length; i++)
        {
            remaining[i] = i;
        }

        var factorial = 1;
        for (var i = 2; i < result.Length; i++)
        {
            factorial *= i;
        }

        for (var i = 0; i < result.Length; i++)
        {
            var left = result.Length - i;
            var pick = index / factorial;
            index %= factorial;

            result[i] = remaining[pick];
            for (var k = pick; k < left - 1; k++)
            {
                remaining[k] = remaining[k + 1];
            }

            if (left > 1)
            {
                factorial /= left - 1;
            }
        }
    }
}

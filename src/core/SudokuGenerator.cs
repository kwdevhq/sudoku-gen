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

    /// <summary>Generates a random sudoku.</summary>
    /// <param name="difficulty">
    /// The desired difficulty, or <see langword="null"/> to let <paramref name="random"/> pick one (every difficulty equally likely).
    /// The pick is made last: with the same <paramref name="random"/> state, passing the difficulty it would have picked gives the same sudoku.
    /// </param>
    /// <param name="random">Randomness source for choosing the id; defaults to <see cref="Random.Shared"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="difficulty"/> is not a defined <see cref="Difficulty"/>.</exception>
    public static Sudoku Generate(Difficulty? difficulty = null, Random? random = null)
    {
        if (difficulty is { } requested && !Enum.IsDefined(requested))
        {
            throw new ArgumentOutOfRangeException(nameof(difficulty), requested, "Unknown difficulty.");
        }

        return FromId(RandomId(difficulty, random ?? Random.Shared));
    }

    /// <summary>Recreates exactly the sudoku with the given id; no randomness is involved.</summary>
    /// <param name="id">The id of the sudoku.</param>
    public static Sudoku FromId(SudokuId id) => Create(id);

    private static SudokuId RandomId(Difficulty? difficulty, Random random)
    {
        // The draws that do not depend on the difficulty come first and the difficulty last, so a Random in the same state
        // gives the same sudoku with or without an explicit difficulty, as long as that difficulty is the one it would pick.
        var position = random.Next();
        var rotation = random.Next(SudokuId.RotationCount);
        var rows = random.Next(SudokuId.LineArrangementCount);
        var columns = random.Next(SudokuId.LineArrangementCount);
        var digits = random.Next(SudokuId.DigitArrangementCount);
        var seeds = Seeds.For(difficulty ?? (Difficulty)random.Next(Seeds.DifficultyCount));

        return new SudokuId(seeds[(int)((long)position * seeds.Length / int.MaxValue)].Id, rotation, rows, columns, digits);
    }

    private static Sudoku Create(SudokuId id)
    {
        var (seed, difficulty) = Seeds.Find(id.Seed);

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

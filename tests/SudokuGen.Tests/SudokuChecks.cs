namespace SudokuGen.Tests;

/// <summary>Independent reference checks, deliberately not sharing code with the generator.</summary>
internal static class SudokuChecks
{
    public static bool IsValidSolution(string cells)
    {
        if (cells.Length != 81 || cells.Any(c => c is < '1' or > '9'))
        {
            return false;
        }

        for (var i = 0; i < 9; i++)
        {
            var row = 0;
            var column = 0;
            var box = 0;
            for (var j = 0; j < 9; j++)
            {
                row |= 1 << (cells[(i * 9) + j] - '1');
                column |= 1 << (cells[(j * 9) + i] - '1');
                var r = ((i / 3) * 3) + (j / 3);
                var c = ((i % 3) * 3) + (j % 3);
                box |= 1 << (cells[(r * 9) + c] - '1');
            }

            if (row != 0x1FF || column != 0x1FF || box != 0x1FF)
            {
                return false;
            }
        }

        return true;
    }

    // Works for both token ('a'-'i') and digit forms as long as both strings use the same alphabet.
    public static bool PuzzleMatchesSolution(string puzzle, string solution) =>
        puzzle.Length == solution.Length && puzzle.Zip(solution).All(p => p.First == '-' || p.First == p.Second);

    public static int CountSolutions(string puzzle, int limit)
    {
        var grid = puzzle.Select(c => c == '-' ? 0 : c - '0').ToArray();
        var count = 0;
        Solve(grid, limit, ref count);
        return count;
    }

    private static void Solve(int[] grid, int limit, ref int count)
    {
        var best = -1;
        var bestMask = 0;
        var bestOptions = 10;

        for (var i = 0; i < 81; i++)
        {
            if (grid[i] != 0)
            {
                continue;
            }

            var mask = Candidates(grid, i);
            var options = int.PopCount(mask);
            if (options < bestOptions)
            {
                (best, bestMask, bestOptions) = (i, mask, options);
                if (options == 0)
                {
                    return;
                }
            }
        }

        if (best < 0)
        {
            count++;
            return;
        }

        for (var digit = 1; digit <= 9 && count < limit; digit++)
        {
            if ((bestMask & (1 << (digit - 1))) == 0)
            {
                continue;
            }

            grid[best] = digit;
            Solve(grid, limit, ref count);
            grid[best] = 0;
        }
    }

    private static int Candidates(int[] grid, int index)
    {
        var row = index / 9;
        var column = index % 9;
        var used = 0;
        for (var k = 0; k < 9; k++)
        {
            foreach (var value in new[] { grid[(row * 9) + k], grid[(k * 9) + column], grid[((((row / 3) * 3) + (k / 3)) * 9) + ((column / 3) * 3) + (k % 3)] })
            {
                if (value != 0)
                {
                    used |= 1 << (value - 1);
                }
            }
        }

        return ~used & 0x1FF;
    }
}

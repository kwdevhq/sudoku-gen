using System.Text;
using System.Text.Json;
using SudokuGen;

return SudokuCli.Run(args, Console.Out, Console.Error);

internal static class SudokuCli
{
    private const string Usage = """
        Usage: sudoku-gen [options]

          -d, --difficulty <easy|medium|hard|expert>   Difficulty (default: any)
              --id <id>                                Recreate the sudoku with this id (e.g. 12-3-457-1022-203456)
              --random-seed <int>                      Seed the random generator for reproducible runs
              --solution                               Also output the solution (text format)
          -f, --format <text|json>                     Output format (default: text)
          -h, --help                                   Show this help
        """;

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        Difficulty? difficulty = null;
        SudokuId? id = null;
        int? randomSeed = null;
        var showSolution = false;
        var format = "text";

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help":
                    output.WriteLine(Usage);
                    return 0;
                case "--solution":
                    showSolution = true;
                    break;
                case "-d" or "--difficulty":
                    if (i + 1 >= args.Length || !Enum.TryParse(args[++i], ignoreCase: true, out Difficulty parsed) || !Enum.IsDefined(parsed))
                    {
                        return Fail(error, "Invalid difficulty, expected one of: easy, medium, hard, expert");
                    }

                    difficulty = parsed;
                    break;
                case "--id":
                    if (i + 1 >= args.Length || !SudokuId.TryParse(args[++i], out var parsedId))
                    {
                        return Fail(error, "Invalid id, expected seed-rotation-rows-columns-digits");
                    }

                    id = parsedId;
                    break;
                case "--random-seed":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out var value))
                    {
                        return Fail(error, "Invalid random seed, expected an integer");
                    }

                    randomSeed = value;
                    break;
                case "-f" or "--format":
                    if (i + 1 >= args.Length || (args[++i].ToLowerInvariant() is var f && f is not ("text" or "json")))
                    {
                        return Fail(error, "Invalid format, expected one of: text, json");
                    }

                    format = f;
                    break;
                default:
                    return Fail(error, $"Unknown argument: {args[i]}");
            }
        }

        Sudoku sudoku;
        try
        {
            sudoku = SudokuGenerator.Generate(difficulty, id, randomSeed is { } s ? new Random(s) : null);
        }
        catch (ArgumentException ex)
        {
            return Fail(error, ex.Message);
        }

        if (format == "json")
        {
            WriteJson(output, sudoku);
        }
        else
        {
            WriteText(output, sudoku, showSolution);
        }

        return 0;
    }

    private static int Fail(TextWriter error, string message)
    {
        error.WriteLine(message);
        error.WriteLine(Usage);
        return 2;
    }

    private static void WriteJson(TextWriter output, Sudoku sudoku)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("puzzle", sudoku.Puzzle);
            writer.WriteString("solution", sudoku.Solution);
            writer.WriteString("difficulty", sudoku.Difficulty.ToString().ToLowerInvariant());
            writer.WriteString("id", sudoku.Id.ToString());
            writer.WriteEndObject();
        }

        output.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static void WriteText(TextWriter output, Sudoku sudoku, bool showSolution)
    {
        output.WriteLine($"Difficulty: {sudoku.Difficulty}");
        output.WriteLine($"Id: {sudoku.Id}");
        output.WriteLine();
        WriteGrid(output, sudoku.Puzzle);

        if (showSolution)
        {
            output.WriteLine();
            output.WriteLine("Solution:");
            output.WriteLine();
            WriteGrid(output, sudoku.Solution);
        }
    }

    private static void WriteGrid(TextWriter output, string cells)
    {
        const string Separator = "+-------+-------+-------+";

        for (var row = 0; row < 9; row++)
        {
            if (row % 3 == 0)
            {
                output.WriteLine(Separator);
            }

            var line = new StringBuilder("|");
            for (var column = 0; column < 9; column++)
            {
                var cell = cells[(row * 9) + column];
                line.Append(' ').Append(cell == '-' ? '.' : cell);
                if (column % 3 == 2)
                {
                    line.Append(" |");
                }
            }

            output.WriteLine(line.ToString());
        }

        output.WriteLine(Separator);
    }
}

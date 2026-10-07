using SudokuGen.Tui;

namespace SudokuGen.Tests;

internal sealed class ManualClock : TimeProvider
{
    private long ticks;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => ticks;

    public void Advance(TimeSpan by) => ticks += by.Ticks;
}

internal sealed class TempDirectory : IDisposable
{
    public TempDirectory() => Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sudoku-tui-" + Guid.NewGuid().ToString("N"));

    public string Path { get; }

    public string SaveFile => System.IO.Path.Combine(Path, "tui-save.json");

    public string StatsFile => System.IO.Path.Combine(Path, "tui-stats.json");

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}

internal static class TuiHelpers
{
    public static Game NewGame(ManualClock? clock = null, string id = KnownIds.Random42Hard) =>
        new(SudokuGenerator.FromId(SudokuId.Parse(id)), clock ?? new ManualClock());

    public static int FirstEmpty(Game game, int after = -1) =>
        Enumerable.Range(after + 1, Game.CellCount - after - 1).First(i => !game.IsGiven(i));

    public static char Wrong(Game game, int cell) => game.Sudoku.Solution[cell] == '1' ? '2' : '1';

    public static void Put(Game game, int cell, char digit)
    {
        game.Select(cell);
        game.Enter(digit);
    }

    /// <summary>Fills every empty cell correctly except the last one; returns that last cell.</summary>
    public static int FillAllButLast(Game game)
    {
        var empties = Enumerable.Range(0, Game.CellCount).Where(i => !game.IsGiven(i)).ToList();
        foreach (var cell in empties.SkipLast(1))
        {
            Put(game, cell, game.Sudoku.Solution[cell]);
        }

        return empties[^1];
    }
}

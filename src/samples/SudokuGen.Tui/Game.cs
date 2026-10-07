using System.Text.Json.Serialization;

namespace SudokuGen.Tui;

internal enum CellMark
{
    None,
    Right,
    Wrong,
}

/// <summary>One edit of one cell, as kept in the undo/redo history.</summary>
internal readonly record struct Change(int Cell, char Before, char After);

internal readonly record struct CheckResult(int Wrong, int Empty);

/// <summary>Everything needed to resume a game: see <see cref="Game.ToSave"/> and <see cref="Game.Restore"/>.</summary>
internal sealed record GameSave(
    [property: JsonRequired] int Version,
    [property: JsonRequired] string Id,
    [property: JsonRequired] string Cells,
    [property: JsonRequired] IReadOnlyList<Change> Undo,
    [property: JsonRequired] IReadOnlyList<Change> Redo,
    [property: JsonRequired] long ElapsedMs)
{
    public const int CurrentVersion = 1;
}

/// <summary>
/// One play-through of a sudoku. Knows the rules but nothing about the screen: selection, entries, history,
/// conflicts, check marks, the solved state and the play-time clock.
/// </summary>
internal sealed class Game
{
    public const int Size = 9;
    public const int CellCount = Size * Size;
    public const char Empty = '-';

    private readonly TimeProvider clock;
    private readonly char[] cells;
    private readonly CellMark[] marks = new CellMark[CellCount];
    private readonly List<Change> undo = [];
    private readonly List<Change> redo = [];
    private TimeSpan accumulated;
    private long? runningSince;

    public Game(Sudoku sudoku, TimeProvider clock)
    {
        Sudoku = sudoku;
        this.clock = clock;
        cells = [.. sudoku.Puzzle];
    }

    public Sudoku Sudoku { get; }

    public int Selected { get; private set; }

    public bool IsSolved { get; private set; }

    public bool CanUndo => undo.Count > 0 && !IsSolved;

    public bool CanRedo => redo.Count > 0 && !IsSolved;

    /// <summary>Number of cells the player has filled in.</summary>
    public int EntryCount => Enumerable.Range(0, CellCount).Count(i => !IsGiven(i) && cells[i] != Empty);

    public TimeSpan Elapsed => accumulated + (runningSince is { } since ? clock.GetElapsedTime(since) : TimeSpan.Zero);

    public char this[int cell] => cells[cell];

    public bool IsGiven(int cell) => Sudoku.Puzzle[cell] != Empty;

    public CellMark MarkAt(int cell) => marks[cell];

    /// <summary>True when another cell in the same row, column or box holds the same digit.</summary>
    public bool HasConflict(int cell) =>
        cells[cell] != Empty && Enumerable.Range(0, CellCount).Any(other => other != cell && cells[other] == cells[cell] && SharesUnit(cell, other));

    public static bool SharesUnit(int a, int b) =>
        a / Size == b / Size || a % Size == b % Size || (a / Size / 3 == b / Size / 3 && a % Size / 3 == b % Size / 3);

    public void Select(int cell) => Selected = Math.Clamp(cell, 0, CellCount - 1);

    public void Move(int rowDelta, int columnDelta) =>
        Selected = (Math.Clamp((Selected / Size) + rowDelta, 0, Size - 1) * Size) + Math.Clamp((Selected % Size) + columnDelta, 0, Size - 1);

    /// <summary>Puts a digit (<c>1</c>-<c>9</c>) or <see cref="Empty"/> into the selected cell; false when nothing changed.</summary>
    public bool Enter(char digit)
    {
        var valid = digit == Empty || digit is >= '1' and <= '9';
        if (!valid || IsSolved || IsGiven(Selected) || cells[Selected] == digit)
        {
            return false;
        }

        undo.Add(new Change(Selected, cells[Selected], digit));
        redo.Clear();
        Apply(Selected, digit);
        marks[Selected] = CellMark.None;
        return true;
    }

    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        var change = undo[^1];
        undo.RemoveAt(undo.Count - 1);
        redo.Add(change);
        Apply(change.Cell, change.Before);
        Select(change.Cell);
        Array.Clear(marks);
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        var change = redo[^1];
        redo.RemoveAt(redo.Count - 1);
        undo.Add(change);
        Apply(change.Cell, change.After);
        Select(change.Cell);
        Array.Clear(marks);
        return true;
    }

    /// <summary>Marks every entry as right or wrong. Marks vanish again as soon as the entry is edited.</summary>
    public CheckResult Check()
    {
        var wrong = 0;
        var empty = 0;
        for (var i = 0; i < CellCount; i++)
        {
            if (IsGiven(i))
            {
                continue;
            }

            if (cells[i] == Empty)
            {
                empty++;
                marks[i] = CellMark.None;
            }
            else if (cells[i] == Sudoku.Solution[i])
            {
                marks[i] = CellMark.Right;
            }
            else
            {
                wrong++;
                marks[i] = CellMark.Wrong;
            }
        }

        return new CheckResult(wrong, empty);
    }

    /// <summary>Back to the puzzle as handed out: no entries, no history, clock at zero.</summary>
    public void Restart()
    {
        Sudoku.Puzzle.CopyTo(0, cells, 0, CellCount);
        undo.Clear();
        redo.Clear();
        Array.Clear(marks);
        accumulated = TimeSpan.Zero;
        runningSince = null;
        IsSolved = false;
    }

    public GameSave ToSave() =>
        new(GameSave.CurrentVersion, Sudoku.Id.ToString(), new string(cells), [.. undo], [.. redo], (long)Elapsed.TotalMilliseconds);

    /// <summary>Rebuilds a game from a save; null when the save is not a consistent game.</summary>
    public static Game? Restore(GameSave save, TimeProvider clock)
    {
        if (save.Version != GameSave.CurrentVersion || !SudokuId.TryParse(save.Id, out var id) || save.ElapsedMs < 0)
        {
            return null;
        }

        var sudoku = SudokuGenerator.FromId(id);
        var consistent = save.Cells.Length == CellCount
            && Enumerable.Range(0, CellCount).All(i => sudoku.Puzzle[i] != Empty ? save.Cells[i] == sudoku.Puzzle[i] : IsDigitOrEmpty(save.Cells[i]))
            && save.Undo.Concat(save.Redo).All(c => c.Cell is >= 0 and < CellCount && sudoku.Puzzle[c.Cell] == Empty && IsDigitOrEmpty(c.Before) && IsDigitOrEmpty(c.After));
        if (!consistent)
        {
            return null;
        }

        var game = new Game(sudoku, clock) { accumulated = TimeSpan.FromMilliseconds(save.ElapsedMs) };
        save.Cells.CopyTo(0, game.cells, 0, CellCount);
        game.undo.AddRange(save.Undo);
        game.redo.AddRange(save.Redo);
        game.Evaluate();
        return game;
    }

    private static bool IsDigitOrEmpty(char c) => c == Empty || c is >= '1' and <= '9';

    private void Apply(int cell, char digit)
    {
        cells[cell] = digit;
        runningSince ??= clock.GetTimestamp();
        Evaluate();
    }

    private void Evaluate()
    {
        if (!cells.AsSpan().SequenceEqual(Sudoku.Solution))
        {
            return;
        }

        accumulated = Elapsed;
        runningSince = null;
        IsSolved = true;
    }
}

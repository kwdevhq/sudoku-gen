using SudokuGen.Tui;

namespace SudokuGen.Tests;

internal sealed class RecordingCanvas(int width = 80, int height = 24) : ICanvas
{
    private readonly char[,] chars = new char[width, height];
    private readonly Style[,] styles = new Style[width, height];

    public int Width => width;

    public int Height => height;

    public void Put(int x, int y, string text, Style style)
    {
        for (var i = 0; i < text.Length; i++)
        {
            chars[x + i, y] = text[i];
            styles[x + i, y] = style;
        }
    }

    public string Row(int y) => new(Enumerable.Range(0, width).Select(x => chars[x, y] == '\0' ? ' ' : chars[x, y]).ToArray());

    public string All() => string.Join('\n', Enumerable.Range(0, height).Select(Row));

    public Style StyleAt(int x, int y) => styles[x, y];

    public (int X, int Y) Find(string text)
    {
        for (var y = 0; y < height; y++)
        {
            var x = Row(y).IndexOf(text, StringComparison.Ordinal);
            if (x >= 0)
            {
                return (x, y);
            }
        }

        throw new InvalidOperationException($"'{text}' is not on screen:\n{All()}");
    }

    public bool Contains(string text) => All().Contains(text, StringComparison.Ordinal);
}

internal sealed class UiFixture : IDisposable
{
    private readonly TempDirectory dir = new();

    public UiFixture(bool startGame = true, string id = KnownIds.Random42Hard)
    {
        Session = new Session(dir.Path, Clock, new Random(42));
        if (startGame)
        {
            Session.TryStart(id, Difficulty.Hard, out _);
        }

        Ui = new Ui(Session, Clock);
    }

    public ManualClock Clock { get; } = new();

    public Session Session { get; }

    public Ui Ui { get; }

    public Game Game => Session.Game!;

    public RecordingCanvas Paint(int width = 80, int height = 24)
    {
        var canvas = new RecordingCanvas(width, height);
        Ui.Paint(canvas);
        return canvas;
    }

    public void Key(string key, bool ctrl = false) => Ui.HandleKey(new Input(key, ctrl));

    public void Type(string text)
    {
        foreach (var c in text)
        {
            Ui.HandleKey(Input.Char(c));
        }
    }

    public void Click(int x, int y) => Ui.HandleClick(x, y, 80, 24);

    public void ClickLabel(string label)
    {
        var (x, y) = Paint().Find(label);
        Click(x + 1, y);
    }

    /// <summary>Top-left of a cell on an 80x24 screen.</summary>
    public static (int X, int Y) CellAt(int row, int column) => (6 + (column * 5) + (column / 3), 1 + (row * 2) + (row / 3));

    public void ClickCell(int cell)
    {
        var (x, y) = CellAt(cell / 9, cell % 9);
        Click(x + 1, y);
    }

    public int FillAllButLast()
    {
        var empties = Enumerable.Range(0, Game.CellCount).Where(i => !Game.IsGiven(i)).ToList();
        foreach (var cell in empties.SkipLast(1))
        {
            Game.Select(cell);
            Session.Enter(Game.Sudoku.Solution[cell]);
        }

        return empties[^1];
    }

    public void SolveWithTheLastKey()
    {
        var last = FillAllButLast();
        Game.Select(last);
        Key(Game.Sudoku.Solution[last].ToString());
    }

    public void Dispose() => dir.Dispose();
}

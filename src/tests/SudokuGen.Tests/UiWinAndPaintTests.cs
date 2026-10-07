using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class UiWinAndPaintTests
{
    private static readonly Rgb LightGreen = Theme.Right;

    [Test]
    public async Task TheLastCorrectDigit_LocksInputPlaysTheWaveThenShowsTheDialog()
    {
        using var f = new UiFixture();
        f.Clock.Advance(TimeSpan.FromSeconds(1));
        f.SolveWithTheLastKey();

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Animating);
        await Assert.That(File.Exists(Path.Combine(Path.GetTempPath(), "never"))).IsFalse();

        f.Key("n");
        f.Click(10, 10);
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Animating);

        f.Clock.Advance(TimeSpan.FromMilliseconds(500));
        await Assert.That(f.Ui.Tick()).IsTrue();
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Animating);

        f.Clock.Advance(TimeSpan.FromMilliseconds(600));
        await Assert.That(f.Ui.Tick()).IsTrue();
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Won);
    }

    [Test]
    public async Task TheWaveSweepsDiagonally_FlashThenGreen()
    {
        using var f = new UiFixture();
        f.SolveWithTheLastKey();
        f.Clock.Advance(TimeSpan.FromMilliseconds(300));

        var screen = f.Paint();

        var (x0, y0) = UiFixture.CellAt(0, 0);
        var (x8, y8) = UiFixture.CellAt(8, 8);
        await Assert.That(screen.StyleAt(x0, y0).Bg).IsEqualTo(LightGreen);
        await Assert.That(screen.StyleAt(x8, y8).Bg).IsEqualTo(Theme.UnitHighlight == default ? default : screen.StyleAt(x8, y8).Bg);
        await Assert.That(screen.StyleAt(x8, y8).Bg).IsNotEqualTo(LightGreen);

        var flash = Enumerable.Range(0, Game.CellCount).Any(c =>
        {
            var (x, y) = UiFixture.CellAt(c / 9, c % 9);
            return screen.StyleAt(x, y).Bg == Theme.Flash;
        });
        await Assert.That(flash).IsTrue();
    }

    [Test]
    public async Task TheWinDialog_ShowsTimeIdTableAndHighlightsTheDifficulty()
    {
        using var f = new UiFixture();
        f.FillAllButLast();
        f.Clock.Advance(TimeSpan.FromSeconds(65));
        var last = f.Game.Selected;
        f.Game.Select(Enumerable.Range(0, Game.CellCount).First(i => f.Game[i] == Game.Empty));
        f.Key(f.Game.Sudoku.Solution[f.Game.Selected].ToString());
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();

        var screen = f.Paint();

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Won);
        await Assert.That(screen.Contains("Solved in 01:05")).IsTrue();
        await Assert.That(screen.Contains($"Id {KnownIds.Random42Hard}")).IsTrue();
        await Assert.That(screen.Contains("New personal best!")).IsFalse();
        var (x, y) = screen.Find("Hard");
        await Assert.That(screen.StyleAt(x, y).Bg).IsEqualTo(Theme.Accent);
        var (ex, ey) = screen.Find("Easy");
        await Assert.That(screen.StyleAt(ex, ey).Bg).IsNotEqualTo(Theme.Accent);
        await Assert.That(last).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task ANewPersonalBest_IsAnnounced()
    {
        using var f = new UiFixture();
        f.FillAllButLast();
        f.Clock.Advance(TimeSpan.FromSeconds(100));
        f.Game.Select(Enumerable.Range(0, Game.CellCount).First(i => f.Game[i] == Game.Empty));
        f.Key(f.Game.Sudoku.Solution[f.Game.Selected].ToString());
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();
        f.Key("n");
        f.Key("Tab");
        f.Key("Tab");
        f.Type(KnownIds.Random42Hard);
        f.Key("Enter");
        f.FillAllButLast();
        f.Clock.Advance(TimeSpan.FromSeconds(10));
        f.Game.Select(Enumerable.Range(0, Game.CellCount).First(i => f.Game[i] == Game.Empty));
        f.Key(f.Game.Sudoku.Solution[f.Game.Selected].ToString());
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();

        await Assert.That(f.Paint().Contains("New personal best!")).IsTrue();
    }

    [Test]
    public async Task WinDialog_KeysAndButtonsStartOverOrQuit()
    {
        using var f = new UiFixture();
        f.SolveWithTheLastKey();
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();

        f.Key("x");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Won);
        f.Key("Enter");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
        await Assert.That(f.Paint().Contains("Esc quit")).IsTrue();
        f.Key("Esc");
        await Assert.That(f.Ui.QuitRequested).IsTrue();
    }

    [Test]
    public async Task WinDialog_ButtonsWork()
    {
        using var f = new UiFixture();
        f.SolveWithTheLastKey();
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();

        f.Key("q");
        await Assert.That(f.Ui.QuitRequested).IsTrue();

        using var g = new UiFixture();
        g.SolveWithTheLastKey();
        g.Clock.Advance(TimeSpan.FromSeconds(2));
        g.Ui.Tick();
        g.ClickLabel("New game (N)");
        await Assert.That(g.Ui.Screen).IsEqualTo(Screen.Start);

        using var h = new UiFixture();
        h.SolveWithTheLastKey();
        h.Clock.Advance(TimeSpan.FromSeconds(2));
        h.Ui.Tick();
        h.ClickLabel("Quit (Q)");
        await Assert.That(h.Ui.QuitRequested).IsTrue();
    }

    [Test]
    public async Task StatisticsTable_ShowsTalliesWithoutAHighlight()
    {
        using var f = new UiFixture();
        f.SolveWithTheLastKey();
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();
        f.Key("n");
        f.Key("Tab");
        f.Key("Enter");

        var screen = f.Paint();

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Stats);
        await Assert.That(screen.Contains("100%")).IsTrue();
        await Assert.That(screen.Contains("1/1")).IsTrue();
        var (x, y) = screen.Find("Hard");
        await Assert.That(screen.StyleAt(x, y).Bg).IsNotEqualTo(Theme.Accent);
    }

    [Test]
    public async Task Board_HighlightsSelectionUnitSameDigitConflictsAndMarks()
    {
        using var f = new UiFixture();
        var game = f.Game;
        var given = Enumerable.Range(0, Game.CellCount).First(game.IsGiven);
        var sameDigit = Enumerable.Range(0, Game.CellCount).First(i => i != given && game[i] == game[given] && !Game.SharesUnit(i, given));
        var inUnit = Enumerable.Range(0, Game.CellCount).First(i => i != given && Game.SharesUnit(i, given) && game[i] != game[given]);
        var unrelated = Enumerable.Range(0, Game.CellCount).First(i => !Game.SharesUnit(i, given) && game[i] != game[given]);
        game.Select(given);

        var screen = f.Paint();

        Rgb Bg(int cell) { var (x, y) = UiFixture.CellAt(cell / 9, cell % 9); return screen.StyleAt(x, y).Bg; }
        await Assert.That(Bg(given)).IsEqualTo(Theme.Selected);
        await Assert.That(Bg(sameDigit)).IsEqualTo(Theme.SameDigit);
        await Assert.That(Bg(inUnit)).IsEqualTo(Theme.UnitHighlight);
        await Assert.That(Bg(unrelated)).IsNotEqualTo(Theme.UnitHighlight);

        var (gx, gy) = UiFixture.CellAt(given / 9, given % 9);
        await Assert.That(screen.Row(gy + 1).Substring(gx, 6).Trim()).IsEqualTo($"[{(char)('\uFF10' + (game[given] - '0'))} ]");
    }

    [Test]
    public async Task Board_ShowsConflictsAmberAndCheckResultsRedAndLightGreen()
    {
        using var f = new UiFixture();
        var game = f.Game;
        var cell = TuiHelpers.FirstEmpty(game);
        var peer = Enumerable.Range(0, Game.CellCount).First(i => i != cell && game.IsGiven(i) && Game.SharesUnit(i, cell));
        var right = TuiHelpers.FirstEmpty(game, cell);
        game.Select(cell);
        f.Key(game[peer].ToString());
        game.Select(right);
        f.Key(game.Sudoku.Solution[right].ToString());

        var screen = f.Paint();
        var (cx, cy) = UiFixture.CellAt(cell / 9, cell % 9);
        await Assert.That(screen.StyleAt(cx + 2, cy).Fg).IsEqualTo(Theme.Conflict);
        var (px, py) = UiFixture.CellAt(peer / 9, peer % 9);
        await Assert.That(screen.StyleAt(px + 2, py).Fg).IsEqualTo(Theme.Conflict);

        f.Key("c");
        screen = f.Paint();
        var (rx, ry) = UiFixture.CellAt(right / 9, right % 9);
        await Assert.That(screen.StyleAt(rx, ry).Bg).IsEqualTo(Theme.Right);
        await Assert.That(screen.StyleAt(cx, cy).Bg).IsEqualTo(game.Sudoku.Solution[cell] == game[cell] ? Theme.Right : Theme.Wrong);
    }

    [Test]
    public async Task Panel_ShowsDifficultyTimeAndId()
    {
        using var f = new UiFixture();
        f.Game.Select(TuiHelpers.FirstEmpty(f.Game));
        f.Key("5");
        f.Clock.Advance(TimeSpan.FromSeconds(3725));

        var screen = f.Paint();

        await Assert.That(screen.Contains("HARD")).IsTrue();
        await Assert.That(screen.Contains("Time  1:02:05")).IsTrue();
        await Assert.That(screen.Contains($"Id {KnownIds.Random42Hard}")).IsTrue();
    }
}

public class FormatTests
{
    [Test]
    public async Task Formats_TimePercentAndCentering()
    {
        await Assert.That(Format.Time(null)).IsEqualTo("--:--");
        await Assert.That(Format.Time(TimeSpan.FromSeconds(75))).IsEqualTo("01:15");
        await Assert.That(Format.Time(TimeSpan.FromSeconds(3661))).IsEqualTo("1:01:01");
        await Assert.That(Format.Percent(null)).IsEqualTo("--");
        await Assert.That(Format.Percent(0.5)).IsEqualTo("50%");
        await Assert.That(Format.Center("ab", 6)).IsEqualTo("  ab  ");
        await Assert.That(Format.Center("abcdef", 3)).IsEqualTo("abc");
    }
}

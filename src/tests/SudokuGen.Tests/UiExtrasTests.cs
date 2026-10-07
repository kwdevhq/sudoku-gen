using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class UiExtrasTests
{
    private static void EnterAllOf(UiFixture f, char digit)
    {
        foreach (var cell in Enumerable.Range(0, Game.CellCount).Where(i => !f.Game.IsGiven(i) && f.Game.Sudoku.Solution[i] == digit))
        {
            f.Game.Select(cell);
            f.Key(digit.ToString());
        }
    }

    [Test]
    public async Task ADigitWithAllNineCorrectlyPlaced_IsGrayOnThePad()
    {
        using var f = new UiFixture();
        await Assert.That(f.Paint().StyleAt(65, 7)).IsEqualTo(Theme.ButtonStyle);

        EnterAllOf(f, '5');

        await Assert.That(f.Game.IsDigitComplete('5')).IsTrue();
        await Assert.That(f.Paint().StyleAt(65, 7)).IsEqualTo(Theme.ButtonDone);
        await Assert.That(f.Paint().StyleAt(58, 5)).IsEqualTo(Theme.ButtonStyle);
    }

    [Test]
    public async Task ADigitInConflict_IsNotComplete()
    {
        using var f = new UiFixture();
        EnterAllOf(f, '5');
        var spare = Enumerable.Range(0, Game.CellCount).First(i => !f.Game.IsGiven(i) && f.Game[i] == Game.Empty);
        f.Game.Select(spare);

        f.Key("5");

        await Assert.That(f.Game.IsDigitComplete('5')).IsFalse();
        await Assert.That(f.Paint().StyleAt(65, 7)).IsEqualTo(Theme.ButtonStyle);
    }

    [Test]
    public async Task CopyingTheId_WorksFromKeyAndButton()
    {
        using var f = new UiFixture();

        f.Key("c", ctrl: true);
        await Assert.That(f.System.Clipboard).IsEqualTo(KnownIds.Random42Hard);
        await Assert.That(f.Paint().Contains("Id copied.")).IsTrue();

        f.System.Clipboard = null;
        f.ClickLabel("Copy");
        await Assert.That(f.System.Clipboard).IsEqualTo(KnownIds.Random42Hard);
    }

    [Test]
    public async Task CopyingTheId_WorksOnTheWinDialog()
    {
        using var f = new UiFixture();
        f.SolveWithTheLastKey();
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();

        f.Key("c");
        await Assert.That(f.System.Clipboard).IsEqualTo(KnownIds.Random42Hard);
        await Assert.That(f.Paint().Contains("Id copied.")).IsTrue();

        f.System.Clipboard = null;
        f.ClickLabel("Copy id (C)");
        await Assert.That(f.System.Clipboard).IsEqualTo(KnownIds.Random42Hard);
    }

    [Test]
    public async Task Paste_FillsTheIdFieldWithOnlyValidCharactersAndFocusesIt()
    {
        using var f = new UiFixture(startGame: false);

        f.Ui.Paste(" # dhs6-rjn0-c38 ; ");

        var screen = f.Paint();
        await Assert.That(screen.Contains("DHS6-RJN0-C38")).IsTrue();
        await Assert.That(screen.Contains("▏")).IsTrue();
    }

    [Test]
    public async Task Paste_IsLimitedToTheIdLengthAndIgnoresNullAndOtherScreens()
    {
        using var f = new UiFixture(startGame: false);
        f.Ui.Paste(new string('A', 30));
        f.Ui.Paste(null);
        await Assert.That(f.Paint().Contains(new string('A', 14) + "▏")).IsTrue();

        using var g = new UiFixture();
        g.Ui.Paste("ABC");
        await Assert.That(g.Ui.Screen).IsEqualTo(Screen.Playing);
    }

    [Test]
    public async Task CtrlV_PastesTheClipboard_AndOtherCtrlKeysDoNotType()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Tab");
        f.Key("Tab");
        f.Key("x", ctrl: true);
        await Assert.That(f.Paint().Contains("X▏")).IsFalse();

        f.Key("v");
        await Assert.That(f.Paint().Contains("V▏")).IsTrue();

        f.Key("Backspace");
        f.System.Clipboard = KnownIds.Random42Hard;
        f.Key("v", ctrl: true);
        f.Key("Enter");

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(f.Session.Game!.Sudoku.Id.ToString()).IsEqualTo(KnownIds.Random42Hard);
    }

    [Test]
    public async Task TheFooter_CreditsKwDevAndLinksToTheSiteAndTheLicense()
    {
        using var f = new UiFixture();
        var screen = f.Paint();
        var (x, y) = screen.Find("kw.dev");
        await Assert.That(screen.Contains("Created by")).IsTrue();
        await Assert.That(screen.StyleAt(x, y).Fg).IsEqualTo(Theme.Brand);
        await Assert.That(screen.StyleAt(x, y).Fg).IsNotEqualTo(Theme.Conflict);

        f.ClickLabel("kw.dev");
        f.ClickLabel("MIT License");

        await Assert.That(f.System.Opened).IsEquivalentTo(["https://kw.dev", "https://github.com/kwdevhq/sudoku-gen/blob/main/LICENSE"]);
    }

    [Test]
    public async Task TheStartScreenFooter_LinksToo_AndTitlesUseTheBrandColor()
    {
        using var f = new UiFixture(startGame: false);
        var screen = f.Paint();
        var (x, y) = screen.Find("█████ █   █ ████  █████ █   █ █   █");
        await Assert.That(screen.StyleAt(x, y).Fg).IsEqualTo(Theme.Brand);

        f.ClickLabel("kw.dev");

        await Assert.That(f.System.Opened).IsEquivalentTo(["https://kw.dev"]);
    }

    [Test]
    public async Task TheStatisticsTable_EndsWithATotalLine()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Enter");
        f.Key("n");
        f.Key("Tab");
        f.Key("Enter");

        var screen = f.Paint();

        var (x, y) = screen.Find("Total");
        await Assert.That(screen.Row(y).Substring(x, 25).Split(' ', StringSplitOptions.RemoveEmptyEntries)).IsEquivalentTo(["Total", "1", "0", "0%"]);
    }
}

public class WrapAndTotalTests
{
    [Test]
    public async Task Wrap_BreaksAtWordsAndKeepsLongWords()
    {
        await Assert.That(Format.Wrap(string.Empty, 5).ToList()).IsEmpty();
        await Assert.That(Format.Wrap("aa bb cc", 5).ToList()).IsEquivalentTo(["aa bb", "cc"]);
        await Assert.That(Format.Wrap("abcdefgh x", 5).ToList()).IsEquivalentTo(["abcdefgh", "x"]);
    }

    [Test]
    public async Task Total_AddsUpGamesAndWinsOverAllDifficulties()
    {
        var stats = Statistics.Empty
            .Started(Difficulty.Easy)
            .Started(Difficulty.Hard)
            .Won(Difficulty.Hard, TimeSpan.FromSeconds(10), out _);

        await Assert.That(stats.Total.Started).IsEqualTo(2);
        await Assert.That(stats.Total.Won).IsEqualTo(1);
        await Assert.That(stats.Total.WinRate).IsEqualTo(0.5);
        await Assert.That(Statistics.Empty.Total.WinRate).IsNull();
    }
}

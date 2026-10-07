using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class UiStartTests
{
    [Test]
    public async Task WithoutASave_TheStartScreenIsShown()
    {
        using var f = new UiFixture(startGame: false);

        var screen = f.Paint();

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
        await Assert.That(screen.Contains("S U D O K U")).IsTrue();
        await Assert.That(screen.Contains("● Medium")).IsTrue();
        await Assert.That(screen.Contains("○ Easy")).IsTrue();
        await Assert.That(screen.Contains("Esc quit")).IsTrue();
        await Assert.That(f.Ui.Tick()).IsFalse();
    }

    [Test]
    public async Task WithASave_TheGameIsShownDirectly()
    {
        using var f = new UiFixture();

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(f.Ui.Tick()).IsTrue();
    }

    [Test]
    public async Task UpAndDown_ChooseTheDifficultyWithinTheList()
    {
        using var f = new UiFixture(startGame: false);

        f.Key("Up");
        f.Key("Up");
        f.Key("Up");
        await Assert.That(f.Paint().Contains("● Easy")).IsTrue();

        for (var i = 0; i < 5; i++)
        {
            f.Key("Down");
        }

        await Assert.That(f.Paint().Contains("● Expert")).IsTrue();
    }

    [Test]
    public async Task TheIdField_TakesLettersDigitsAndHyphensUpToALimit()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Tab");
        f.Key("Tab");

        f.Type("dhs6 -!rjn0-c38xyz99");
        f.Key("Backspace");

        await Assert.That(f.Paint().Contains("DHS6-RJN0-C38X")).IsFalse();
        await Assert.That(f.Paint().Contains("DHS6-RJN0-C38")).IsTrue();
        await Assert.That(f.Paint().Contains("▏")).IsTrue();
    }

    [Test]
    public async Task Backspace_OnAnEmptyIdFieldDoesNothing()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Tab");
        f.Key("Tab");

        f.Key("Backspace");

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
    }

    [Test]
    public async Task Typing_IsIgnoredOutsideTheIdField()
    {
        using var f = new UiFixture(startGame: false);

        f.Type("abc");
        f.Key("Backspace");

        await Assert.That(f.Paint().Contains("ABC")).IsFalse();
    }

    [Test]
    public async Task Enter_StartsARandomGameOfTheChosenDifficulty()
    {
        using var f = new UiFixture(startGame: false);

        f.Key("Down");
        f.Key("Enter");

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(f.Session.Game!.Sudoku.Difficulty).IsEqualTo(Difficulty.Hard);
        await Assert.That(f.Session.Stats[Difficulty.Hard].Started).IsEqualTo(1);
    }

    [Test]
    public async Task AnIdReplaysThatSudoku_AndAnInvalidIdShowsTheError()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Tab");
        f.Key("Tab");
        f.Type("zzzz");
        f.Key("Enter");

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
        await Assert.That(f.Paint().Contains("That is not a valid Sudoku id.")).IsTrue();

        for (var i = 0; i < 4; i++)
        {
            f.Key("Backspace");
        }

        f.Type(KnownIds.Random42Hard);
        f.Key("Enter");

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(f.Session.Game!.Sudoku.Id.ToString()).IsEqualTo(KnownIds.Random42Hard);
    }

    [Test]
    public async Task Esc_WithoutAGameQuits_WithAnUnfinishedGameGoesBack()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Esc");
        await Assert.That(f.Ui.QuitRequested).IsTrue();

        using var g = new UiFixture();
        g.Key("n");
        await Assert.That(g.Ui.Screen).IsEqualTo(Screen.Start);
        await Assert.That(g.Paint().Contains("Esc back")).IsTrue();
        g.Key("Esc");
        await Assert.That(g.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(g.Ui.QuitRequested).IsFalse();
    }

    [Test]
    public async Task Statistics_OpenFromTheFocusedButtonAndAnyKeyClosesThem()
    {
        using var f = new UiFixture(startGame: false);
        f.Key("Tab");
        await Assert.That(f.Paint().Contains("Statistics")).IsTrue();

        f.Key("Enter");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Stats);
        var screen = f.Paint();
        await Assert.That(screen.Contains("Press any key")).IsTrue();
        await Assert.That(screen.Contains("Fastest")).IsTrue();
        await Assert.That(screen.Contains("--:--")).IsTrue();
        await Assert.That(f.Ui.Tick()).IsFalse();

        f.Key("x");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
    }

    [Test]
    public async Task Mouse_ChoosesDifficultyFocusesTheIdStartsAndOpensStatistics()
    {
        using var f = new UiFixture(startGame: false);

        f.ClickLabel("○ Expert");
        await Assert.That(f.Paint().Contains("● Expert")).IsTrue();

        f.ClickLabel("Statistics");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Stats);
        f.Click(5, 5);
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);

        var (_, y) = f.Paint().Find("Sudoku id");
        f.Click(30, y + 1);
        f.Type("a1");
        await Assert.That(f.Paint().Contains("A1")).IsTrue();

        f.ClickLabel("Start");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
        f.Key("Backspace");
        f.Key("Backspace");
        f.ClickLabel("Start");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
    }

    [Test]
    public async Task ATooSmallTerminalAsksToEnlarge_AndIgnoresClicks()
    {
        using var f = new UiFixture(startGame: false);

        var screen = f.Paint(60, 20);
        f.Ui.HandleClick(10, 10, 60, 20);

        await Assert.That(screen.Contains("Please enlarge the terminal to at least 70x24.")).IsTrue();
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
    }
}

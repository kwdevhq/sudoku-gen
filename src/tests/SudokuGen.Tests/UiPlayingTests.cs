using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class UiPlayingTests
{
    [Test]
    public async Task Arrows_MoveTheSelection()
    {
        using var f = new UiFixture();
        f.Game.Select(40);

        f.Key("Up");
        f.Key("Left");
        await Assert.That(f.Game.Selected).IsEqualTo(30);
        f.Key("Down");
        f.Key("Right");
        await Assert.That(f.Game.Selected).IsEqualTo(40);
    }

    [Test]
    public async Task Digits_EnterAndEraseKeysClear_AndUnknownKeysAreIgnored()
    {
        using var f = new UiFixture();
        var cell = TuiHelpers.FirstEmpty(f.Game);
        f.Game.Select(cell);

        f.Key("5");
        await Assert.That(f.Game[cell]).IsEqualTo('5');
        f.Key("0");
        await Assert.That(f.Game[cell]).IsEqualTo(Game.Empty);
        f.Key("6");
        f.Key("Backspace");
        await Assert.That(f.Game[cell]).IsEqualTo(Game.Empty);
        f.Key("7");
        f.Key("Delete");
        await Assert.That(f.Game[cell]).IsEqualTo(Game.Empty);

        f.Key("F5");
        f.Key("z");
        f.Key("y");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
    }

    [Test]
    public async Task UndoAndRedo_AreOnCtrlZAndCtrlY()
    {
        using var f = new UiFixture();
        var cell = TuiHelpers.FirstEmpty(f.Game);
        f.Game.Select(cell);
        f.Key("5");

        f.Key("z", ctrl: true);
        await Assert.That(f.Game[cell]).IsEqualTo(Game.Empty);
        f.Key("y", ctrl: true);
        await Assert.That(f.Game[cell]).IsEqualTo('5');
    }

    [Test]
    public async Task Check_ShowsTheCountsAndMarksButNothingHappensAutomatically()
    {
        using var f = new UiFixture();
        var cell = TuiHelpers.FirstEmpty(f.Game);
        f.Game.Select(cell);
        f.Key(TuiHelpers.Wrong(f.Game, cell).ToString());
        await Assert.That(f.Paint().Contains(" wrong,")).IsFalse();
        await Assert.That(f.Game.MarkAt(cell)).IsEqualTo(CellMark.None);

        f.Key("c");

        var empty = Enumerable.Range(0, Game.CellCount).Count(i => !f.Game.IsGiven(i)) - 1;
        await Assert.That(f.Paint().Contains($"1 wrong, {empty} empty")).IsTrue();
        await Assert.That(f.Game.MarkAt(cell)).IsEqualTo(CellMark.Wrong);

        f.Key("0");
        await Assert.That(f.Paint().Contains(" wrong,")).IsFalse();
    }

    [Test]
    public async Task Reset_WithoutEntriesOnlySaysSo_WithEntriesAsksFirst()
    {
        using var f = new UiFixture();
        f.Key("r");
        await Assert.That(f.Paint().Contains("Nothing to reset yet.")).IsTrue();
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);

        f.Game.Select(TuiHelpers.FirstEmpty(f.Game));
        f.Key("5");
        f.Key("r");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Confirm);
        var screen = f.Paint();
        await Assert.That(screen.Contains("Reset this puzzle?")).IsTrue();
        await Assert.That(screen.Contains("counts as a new game")).IsTrue();

        f.Key("n");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(f.Game.EntryCount).IsEqualTo(1);

        f.Key("r");
        f.Key("y");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
        await Assert.That(f.Game.EntryCount).IsEqualTo(0);
        await Assert.That(f.Session.Stats[Difficulty.Hard].Started).IsEqualTo(2);
    }

    [Test]
    public async Task NewGame_GoesStraightToTheStartScreenWithoutEntries_ElseConfirms()
    {
        using var f = new UiFixture();
        f.Game.Select(TuiHelpers.FirstEmpty(f.Game));
        f.Key("5");

        f.Key("n");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Confirm);
        await Assert.That(f.Paint().Contains("Start a new game?")).IsTrue();

        f.Key("Esc");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);

        f.Key("n");
        f.Key("Enter");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
        await Assert.That(f.Game.EntryCount).IsEqualTo(1);
    }

    [Test]
    public async Task Confirm_IgnoresOtherKeys()
    {
        using var f = new UiFixture();
        f.Game.Select(TuiHelpers.FirstEmpty(f.Game));
        f.Key("5");
        f.Key("r");

        f.Key("x");

        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Confirm);
    }

    [Test]
    [Arguments("q")]
    public async Task Quit_SavesAndRequestsTheEnd(string key)
    {
        using var f = new UiFixture();
        f.Game.Select(TuiHelpers.FirstEmpty(f.Game));
        f.Key("5");

        f.Key(key);

        await Assert.That(f.Ui.QuitRequested).IsTrue();
    }

    [Test]
    public async Task Clicks_SelectCellsAndPressTheButtons()
    {
        using var f = new UiFixture();
        var cell = TuiHelpers.FirstEmpty(f.Game);
        f.ClickCell(cell);
        await Assert.That(f.Game.Selected).IsEqualTo(cell);

        f.Click(66, 7);
        await Assert.That(f.Game[cell]).IsEqualTo('5');

        f.ClickLabel("Undo");
        await Assert.That(f.Game[cell]).IsEqualTo(Game.Empty);
        f.ClickLabel("Redo");
        await Assert.That(f.Game[cell]).IsEqualTo('5');
        f.ClickLabel("Erase");
        await Assert.That(f.Game[cell]).IsEqualTo(Game.Empty);

        f.ClickLabel("Undo");
        f.Click(79, 29);
        await Assert.That(f.Game[cell]).IsEqualTo('5');

        f.ClickLabel("Check");
        await Assert.That(f.Paint().Contains(" wrong, ")).IsTrue();
        f.ClickLabel("Reset");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Confirm);
        f.ClickLabel("No (N)");
        f.ClickLabel("New");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Confirm);
        f.ClickLabel("Yes (Y)");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);
    }

    [Test]
    public async Task ClickingResetConfirmedRestartsThePuzzle_AndQuitQuits()
    {
        using var f = new UiFixture();
        f.Game.Select(TuiHelpers.FirstEmpty(f.Game));
        f.Key("5");
        f.ClickLabel("Reset");
        f.ClickLabel("Yes (Y)");
        await Assert.That(f.Game.EntryCount).IsEqualTo(0);

        f.ClickLabel("Quit");
        await Assert.That(f.Ui.QuitRequested).IsTrue();
    }

    [Test]
    public async Task DisabledButtons_DoNothingWhenClicked()
    {
        using var f = new UiFixture();

        f.ClickLabel("Undo");
        f.ClickLabel("Redo");

        await Assert.That(f.Game.CanUndo).IsFalse();
    }

    [Test]
    public async Task ClickingEmptySpace_DoesNothing()
    {
        using var f = new UiFixture();
        var before = f.Game.Selected;

        f.Click(56, 10);
        f.Click(79, 29);
        f.Click(57, 0);

        await Assert.That(f.Game.Selected).IsEqualTo(before);
    }

    [Test]
    public async Task OnATooSmallTerminal_ClicksDoNothing_AndTheConfirmScreenNeedsNoRedraw()
    {
        using var f = new UiFixture();
        var cell = TuiHelpers.FirstEmpty(f.Game);
        f.Game.Select(cell);

        f.Ui.HandleClick(66, 7, 60, 20);
        await Assert.That(f.Ui.Tick()).IsTrue();

        f.Key("5");
        f.Key("r");
        await Assert.That(f.Ui.Tick()).IsFalse();
    }
}

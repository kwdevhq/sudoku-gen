using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class GameTests
{
    [Test]
    public async Task Selection_MovesAndStaysOnTheBoard()
    {
        var game = TuiHelpers.NewGame();

        game.Move(-1, -1);
        await Assert.That(game.Selected).IsEqualTo(0);

        game.Move(1, 2);
        await Assert.That(game.Selected).IsEqualTo(9 + 2);

        game.Select(1000);
        await Assert.That(game.Selected).IsEqualTo(80);

        game.Move(1, 1);
        await Assert.That(game.Selected).IsEqualTo(80);
    }

    [Test]
    public async Task Givens_CannotBeChanged()
    {
        var game = TuiHelpers.NewGame();
        var given = Enumerable.Range(0, Game.CellCount).First(game.IsGiven);
        game.Select(given);

        await Assert.That(game.Enter(TuiHelpers.Wrong(game, given))).IsFalse();
        await Assert.That(game.Enter(Game.Empty)).IsFalse();
        await Assert.That(game[given]).IsEqualTo(game.Sudoku.Puzzle[given]);
    }

    [Test]
    [Arguments('0')]
    [Arguments('a')]
    public async Task InvalidCharacters_AreRejected(char digit)
    {
        var game = TuiHelpers.NewGame();
        game.Select(TuiHelpers.FirstEmpty(game));

        await Assert.That(game.Enter(digit)).IsFalse();
    }

    [Test]
    public async Task Enter_PutsClearsAndIgnoresNoChange()
    {
        var game = TuiHelpers.NewGame();
        var cell = TuiHelpers.FirstEmpty(game);
        game.Select(cell);

        await Assert.That(game.Enter('5')).IsTrue();
        await Assert.That(game[cell]).IsEqualTo('5');
        await Assert.That(game.EntryCount).IsEqualTo(1);
        await Assert.That(game.Enter('5')).IsTrue();
        await Assert.That(game[cell]).IsEqualTo(Game.Empty);
        await Assert.That(game.EntryCount).IsEqualTo(0);
        await Assert.That(game.Enter(Game.Empty)).IsFalse();
    }

    [Test]
    public async Task MovingSideways_RunsOnIntoTheNextAndPreviousRow()
    {
        var game = TuiHelpers.NewGame();
        game.Select(8);
        game.Move(0, 1);
        await Assert.That(game.Selected).IsEqualTo(9);
        game.Move(0, -1);
        await Assert.That(game.Selected).IsEqualTo(8);
        game.Select(80);
        game.Move(0, 1);
        await Assert.That(game.Selected).IsEqualTo(80);
        game.Select(0);
        game.Move(0, -1);
        await Assert.That(game.Selected).IsEqualTo(0);
        game.Move(-1, 0);
        await Assert.That(game.Selected).IsEqualTo(0);
    }

    [Test]
    public async Task Conflicts_AreFoundInRowColumnAndBoxButNotElsewhere()
    {
        var game = TuiHelpers.NewGame();
        var cell = TuiHelpers.FirstEmpty(game);
        var digit = Enumerable.Range(0, Game.CellCount).First(i => i != cell && game.IsGiven(i) && Game.SharesUnit(i, cell));
        var other = Enumerable.Range(0, Game.CellCount).First(i => game.IsGiven(i) && !Game.SharesUnit(i, cell) && game[i] == game[digit]);

        TuiHelpers.Put(game, cell, game[digit]);

        await Assert.That(game.HasConflict(cell)).IsTrue();
        await Assert.That(game.HasConflict(digit)).IsTrue();
        await Assert.That(game.HasConflict(other)).IsFalse();
        await Assert.That(game.HasConflict(TuiHelpers.FirstEmpty(game, cell))).IsFalse();
    }

    [Test]
    public async Task SharesUnit_CoversRowColumnBoxAndRejectsOthers()
    {
        await Assert.That(Game.SharesUnit(0, 8)).IsTrue();
        await Assert.That(Game.SharesUnit(0, 72)).IsTrue();
        await Assert.That(Game.SharesUnit(0, 20)).IsTrue();
        await Assert.That(Game.SharesUnit(0, 3 * 9 + 3)).IsFalse();
        await Assert.That(Game.SharesUnit(0, 2 * 9 + 3)).IsFalse();
    }

    [Test]
    public async Task UndoRedo_WalkTheHistoryAndSelectTheCell()
    {
        var game = TuiHelpers.NewGame();
        var first = TuiHelpers.FirstEmpty(game);
        var second = TuiHelpers.FirstEmpty(game, first);
        TuiHelpers.Put(game, first, '4');
        TuiHelpers.Put(game, second, '6');

        await Assert.That(game.Undo()).IsTrue();
        await Assert.That(game[second]).IsEqualTo(Game.Empty);
        await Assert.That(game.CanRedo).IsTrue();
        await Assert.That(game.Redo()).IsTrue();
        await Assert.That(game[second]).IsEqualTo('6');

        game.Undo();
        game.Undo();
        await Assert.That(game.Selected).IsEqualTo(first);
        await Assert.That(game.CanUndo).IsFalse();
        await Assert.That(game.Undo()).IsFalse();

        game.Redo();
        game.Redo();
        await Assert.That(game.Redo()).IsFalse();
    }

    [Test]
    public async Task NewEntry_DropsTheRedoHistory()
    {
        var game = TuiHelpers.NewGame();
        var cell = TuiHelpers.FirstEmpty(game);
        TuiHelpers.Put(game, cell, '4');
        game.Undo();

        game.Enter('5');

        await Assert.That(game.CanRedo).IsFalse();
    }

    [Test]
    public async Task Check_MarksRightAndWrongAndCountsEmpties_WithoutMarkingGivensOrEmpties()
    {
        var game = TuiHelpers.NewGame();
        var right = TuiHelpers.FirstEmpty(game);
        var wrong = TuiHelpers.FirstEmpty(game, right);
        var empty = TuiHelpers.FirstEmpty(game, wrong);
        var emptyCount = Enumerable.Range(0, Game.CellCount).Count(i => !game.IsGiven(i));
        TuiHelpers.Put(game, right, game.Sudoku.Solution[right]);
        TuiHelpers.Put(game, wrong, TuiHelpers.Wrong(game, wrong));

        var result = game.Check();

        await Assert.That(result).IsEqualTo(new CheckResult(1, emptyCount - 2));
        await Assert.That(game.MarkAt(right)).IsEqualTo(CellMark.Right);
        await Assert.That(game.MarkAt(wrong)).IsEqualTo(CellMark.Wrong);
        await Assert.That(game.MarkAt(empty)).IsEqualTo(CellMark.None);
        await Assert.That(game.MarkAt(Enumerable.Range(0, Game.CellCount).First(game.IsGiven))).IsEqualTo(CellMark.None);
    }

    [Test]
    public async Task Marks_VanishOnAnyEditAndOnUndoRedo()
    {
        var game = TuiHelpers.NewGame();
        var a = TuiHelpers.FirstEmpty(game);
        var b = TuiHelpers.FirstEmpty(game, a);
        TuiHelpers.Put(game, a, TuiHelpers.Wrong(game, a));
        TuiHelpers.Put(game, b, TuiHelpers.Wrong(game, b));
        game.Check();

        TuiHelpers.Put(game, a, Game.Empty);
        await Assert.That(game.MarkAt(a)).IsEqualTo(CellMark.None);
        await Assert.That(game.MarkAt(b)).IsEqualTo(CellMark.None);

        game.Check();
        game.Undo();
        await Assert.That(game.MarkAt(b)).IsEqualTo(CellMark.None);

        game.Check();
        game.Redo();
        await Assert.That(game.MarkAt(b)).IsEqualTo(CellMark.None);
    }

    [Test]
    public async Task LastCorrectEntry_SolvesTheGameAndLocksIt()
    {
        var clock = new ManualClock();
        var game = TuiHelpers.NewGame(clock);
        var last = TuiHelpers.FillAllButLast(game);
        await Assert.That(game.IsSolved).IsFalse();

        clock.Advance(TimeSpan.FromSeconds(90));
        TuiHelpers.Put(game, last, game.Sudoku.Solution[last]);
        clock.Advance(TimeSpan.FromSeconds(30));

        await Assert.That(game.IsSolved).IsTrue();
        await Assert.That(game.Elapsed).IsEqualTo(TimeSpan.FromSeconds(90));
        await Assert.That(game.Enter(Game.Empty)).IsFalse();
        await Assert.That(game.Undo()).IsFalse();
        await Assert.That(game.CanRedo).IsFalse();
    }

    [Test]
    public async Task Redo_CanSolveTheGame()
    {
        var game = TuiHelpers.NewGame();
        var last = TuiHelpers.FillAllButLast(game);
        TuiHelpers.Put(game, last, game.Sudoku.Solution[last]);
        var save = game.ToSave();

        var restored = Game.Restore(save with { Cells = save.Cells.Remove(last, 1).Insert(last, "-"), Undo = save.Undo.SkipLast(1).ToList(), Redo = [save.Undo[^1]] }, new ManualClock());

        await Assert.That(restored!.IsSolved).IsFalse();
        await Assert.That(restored.Redo()).IsTrue();
        await Assert.That(restored.IsSolved).IsTrue();
    }

    [Test]
    public async Task Clock_StartsAtFirstInputAndKeepsRunning()
    {
        var clock = new ManualClock();
        var game = TuiHelpers.NewGame(clock);
        clock.Advance(TimeSpan.FromSeconds(10));
        await Assert.That(game.Elapsed).IsEqualTo(TimeSpan.Zero);

        TuiHelpers.Put(game, TuiHelpers.FirstEmpty(game), '5');
        clock.Advance(TimeSpan.FromSeconds(5));
        TuiHelpers.Put(game, TuiHelpers.FirstEmpty(game), '6');
        clock.Advance(TimeSpan.FromSeconds(2));

        await Assert.That(game.Elapsed).IsEqualTo(TimeSpan.FromSeconds(7));
    }

    [Test]
    public async Task Restart_RestoresThePuzzleAndStopsTheClock()
    {
        var clock = new ManualClock();
        var game = TuiHelpers.NewGame(clock);
        TuiHelpers.Put(game, TuiHelpers.FirstEmpty(game), '5');
        clock.Advance(TimeSpan.FromSeconds(5));
        game.Check();

        game.Restart();
        clock.Advance(TimeSpan.FromSeconds(5));

        await Assert.That(game.EntryCount).IsEqualTo(0);
        await Assert.That(game.CanUndo).IsFalse();
        await Assert.That(game.Elapsed).IsEqualTo(TimeSpan.Zero);
        await Assert.That(new string(Enumerable.Range(0, Game.CellCount).Select(i => game[i]).ToArray())).IsEqualTo(game.Sudoku.Puzzle);
    }

    [Test]
    public async Task Restart_ReopensASolvedGame()
    {
        var game = TuiHelpers.NewGame();
        var last = TuiHelpers.FillAllButLast(game);
        TuiHelpers.Put(game, last, game.Sudoku.Solution[last]);

        game.Restart();

        await Assert.That(game.IsSolved).IsFalse();
    }

    [Test]
    public async Task Save_RoundTripsEntriesHistoryAndTime()
    {
        var clock = new ManualClock();
        var game = TuiHelpers.NewGame(clock);
        var a = TuiHelpers.FirstEmpty(game);
        var b = TuiHelpers.FirstEmpty(game, a);
        TuiHelpers.Put(game, a, '4');
        TuiHelpers.Put(game, b, '6');
        game.Undo();
        clock.Advance(TimeSpan.FromSeconds(42));

        var restored = Game.Restore(game.ToSave(), new ManualClock())!;

        await Assert.That(restored.Sudoku.Id).IsEqualTo(game.Sudoku.Id);
        await Assert.That(restored[a]).IsEqualTo('4');
        await Assert.That(restored.CanUndo).IsTrue();
        await Assert.That(restored.CanRedo).IsTrue();
        await Assert.That(restored.Elapsed).IsEqualTo(TimeSpan.FromSeconds(42));
    }

    [Test]
    public async Task Restore_RejectsInconsistentSaves()
    {
        var game = TuiHelpers.NewGame();
        var empty = TuiHelpers.FirstEmpty(game);
        var given = Enumerable.Range(0, Game.CellCount).First(game.IsGiven);
        TuiHelpers.Put(game, empty, '4');
        var good = game.ToSave();
        var clock = new ManualClock();

        var bad = new[]
        {
            good with { Version = 99 },
            good with { Id = "nope" },
            good with { ElapsedMs = -1 },
            good with { Cells = good.Cells[..80] },
            good with { Cells = good.Cells.Remove(empty, 1).Insert(empty, "x") },
            good with { Cells = good.Cells.Remove(empty, 1).Insert(empty, "0") },
            good with { Cells = good.Cells.Remove(given, 1).Insert(given, game.Sudoku.Puzzle[given] == '1' ? "2" : "1") },
            good with { Undo = [new Change(81, '-', '4')] },
            good with { Redo = [new Change(given, '-', '4')] },
            good with { Undo = [new Change(empty, 'x', '4')] },
            good with { Undo = [new Change(empty, '-', 'x')] },
        };

        foreach (var save in bad)
        {
            await Assert.That(Game.Restore(save, clock)).IsNull();
        }

        await Assert.That(Game.Restore(good, clock)).IsNotNull();
    }
}

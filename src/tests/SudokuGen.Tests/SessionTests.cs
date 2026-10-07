using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class SessionTests
{
    private static Session Open(TempDirectory dir, ManualClock? clock = null) =>
        new(dir.Path, clock ?? new ManualClock(), new Random(42));

    private static void Put(Session session, int cell, char digit)
    {
        session.Game!.Select(cell);
        session.Enter(digit);
    }

    private static int PlayAllButLast(Session session)
    {
        var game = session.Game!;
        var empties = Enumerable.Range(0, Game.CellCount).Where(i => !game.IsGiven(i)).ToList();
        foreach (var cell in empties.SkipLast(1))
        {
            Put(session, cell, game.Sudoku.Solution[cell]);
        }

        return empties[^1];
    }

    [Test]
    public async Task FreshStart_HasNoGameAndEmptyStatistics()
    {
        using var dir = new TempDirectory();

        var session = Open(dir);

        await Assert.That(session.Game).IsNull();
        await Assert.That(session.NeedsConfirmation).IsFalse();
        await Assert.That(session.Stats).IsEqualTo(Statistics.Empty);
    }

    [Test]
    public async Task TryStart_BlankIdPlaysARandomGameOfTheDifficulty()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);

        var started = session.TryStart("  ", Difficulty.Hard, out var error);

        await Assert.That(started).IsTrue();
        await Assert.That(error).IsNull();
        await Assert.That(session.Game!.Sudoku.Id.ToString()).IsEqualTo(KnownIds.Random42Hard);
        await Assert.That(session.Stats[Difficulty.Hard].Started).IsEqualTo(1);
        await Assert.That(File.Exists(dir.SaveFile)).IsTrue();
    }

    [Test]
    public async Task TryStart_WithAnIdReplaysThatSudokuIgnoringTheDifficulty()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);

        var started = session.TryStart("2zkgvh7j46x", Difficulty.Easy, out _);

        await Assert.That(started).IsTrue();
        await Assert.That(session.Game!.Sudoku.Id.ToString()).IsEqualTo(KnownIds.Random42Hard);
        await Assert.That(session.Stats[Difficulty.Hard].Started).IsEqualTo(1);
        await Assert.That(session.Stats[Difficulty.Easy].Started).IsEqualTo(0);
    }

    [Test]
    public async Task TryStart_RejectsAnInvalidIdAndKeepsTheCurrentGame()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);
        session.TryStart(null, Difficulty.Easy, out _);
        var game = session.Game;

        var started = session.TryStart("not an id", Difficulty.Hard, out var error);

        await Assert.That(started).IsFalse();
        await Assert.That(error).IsEqualTo("That is not a valid Sudoku id.");
        await Assert.That(session.Game).IsSameReferenceAs(game);
    }

    [Test]
    public async Task Entries_AreSavedAndResumedOnTheNextStart()
    {
        using var dir = new TempDirectory();
        var clock = new ManualClock();
        var session = Open(dir, clock);
        session.TryStart(null, Difficulty.Hard, out _);
        var cell = TuiHelpers.FirstEmpty(session.Game!);
        Put(session, cell, '5');
        clock.Advance(TimeSpan.FromSeconds(30));
        session.SaveOnExit();

        var resumed = Open(dir, new ManualClock());

        await Assert.That(resumed.Game!.Sudoku.Id.ToString()).IsEqualTo(KnownIds.Random42Hard);
        await Assert.That(resumed.Game[cell]).IsEqualTo('5');
        await Assert.That(resumed.Game.Elapsed).IsEqualTo(TimeSpan.FromSeconds(30));
        await Assert.That(resumed.Stats[Difficulty.Hard].Started).IsEqualTo(1);
    }

    [Test]
    public async Task SaveOnExit_DoesNothingWithoutAGameOrAfterAWin()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);

        session.SaveOnExit();
        await Assert.That(File.Exists(dir.SaveFile)).IsFalse();

        session.TryStart(null, Difficulty.Hard, out _);
        var last = PlayAllButLast(session);
        Put(session, last, session.Game!.Sudoku.Solution[last]);
        session.SaveOnExit();
        await Assert.That(File.Exists(dir.SaveFile)).IsFalse();
    }

    [Test]
    public async Task UndoRedo_ArePersisted()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);
        session.TryStart(null, Difficulty.Hard, out _);
        var cell = TuiHelpers.FirstEmpty(session.Game!);
        Put(session, cell, '5');

        await Assert.That(session.Undo()).IsTrue();
        await Assert.That(Open(dir).Game![cell]).IsEqualTo(Game.Empty);
        await Assert.That(session.Redo()).IsTrue();
        await Assert.That(Open(dir).Game![cell]).IsEqualTo('5');
        await Assert.That(session.Redo()).IsFalse();
    }

    [Test]
    public async Task Solving_DeletesTheSaveRecordsTheWinAndKeepsTheGameLocked()
    {
        using var dir = new TempDirectory();
        var clock = new ManualClock();
        var session = Open(dir, clock);
        session.TryStart(null, Difficulty.Hard, out _);
        var last = PlayAllButLast(session);
        clock.Advance(TimeSpan.FromSeconds(75));

        Put(session, last, session.Game!.Sudoku.Solution[last]);

        await Assert.That(File.Exists(dir.SaveFile)).IsFalse();
        await Assert.That(session.Win!.Time).IsEqualTo(TimeSpan.FromSeconds(75));
        await Assert.That(session.Win.NewFastest).IsFalse();
        await Assert.That(session.Stats[Difficulty.Hard].Won).IsEqualTo(1);
        await Assert.That(Open(dir).Stats[Difficulty.Hard].Won).IsEqualTo(1);
        await Assert.That(Open(dir).Game).IsNull();
        await Assert.That(session.NeedsConfirmation).IsFalse();
    }

    [Test]
    public async Task NeedsConfirmation_OnlyForUnfinishedGamesWithEntries()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);
        session.TryStart(null, Difficulty.Hard, out _);
        await Assert.That(session.NeedsConfirmation).IsFalse();

        Put(session, TuiHelpers.FirstEmpty(session.Game!), '5');
        await Assert.That(session.NeedsConfirmation).IsTrue();
    }

    [Test]
    public async Task Reset_WithoutEntriesDoesNothing()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);

        await Assert.That(session.Reset()).IsFalse();

        session.TryStart(null, Difficulty.Hard, out _);
        await Assert.That(session.Reset()).IsFalse();
        await Assert.That(session.Stats[Difficulty.Hard].Started).IsEqualTo(1);
    }

    [Test]
    public async Task Reset_RestartsCountsAsANewGameAndBreaksTheStreak()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);
        session.TryStart(null, Difficulty.Hard, out _);
        var last = PlayAllButLast(session);
        Put(session, last, session.Game!.Sudoku.Solution[last]);
        session.TryStart(null, Difficulty.Hard, out _);
        Put(session, TuiHelpers.FirstEmpty(session.Game!), '5');

        await Assert.That(session.Reset()).IsTrue();

        var hard = session.Stats[Difficulty.Hard];
        await Assert.That(session.Game!.EntryCount).IsEqualTo(0);
        await Assert.That(hard.Started).IsEqualTo(3);
        await Assert.That(hard.Streak).IsEqualTo(0);
        await Assert.That(hard.BestStreak).IsEqualTo(1);
        await Assert.That(Open(dir).Game!.EntryCount).IsEqualTo(0);
    }

    [Test]
    public async Task StartingOverAnUnfinishedGame_AbandonsIt()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);
        session.TryStart(null, Difficulty.Hard, out _);
        var last = PlayAllButLast(session);
        Put(session, last, session.Game!.Sudoku.Solution[last]);
        session.TryStart(null, Difficulty.Hard, out _);
        Put(session, TuiHelpers.FirstEmpty(session.Game!), '5');

        session.TryStart(null, Difficulty.Easy, out _);

        await Assert.That(session.Win).IsNull();
        await Assert.That(session.Stats[Difficulty.Hard].Streak).IsEqualTo(0);
        await Assert.That(session.Stats[Difficulty.Hard].Started).IsEqualTo(2);
        await Assert.That(session.Stats[Difficulty.Easy].Started).IsEqualTo(1);
    }

    [Test]
    public async Task PersonalBest_IsFlaggedOnTheWinThatBeatsTheFastest()
    {
        using var dir = new TempDirectory();
        var clock = new ManualClock();
        var session = Open(dir, clock);
        foreach (var seconds in new[] { 100, 40 })
        {
            session.TryStart(null, Difficulty.Hard, out _);
            var last = PlayAllButLast(session);
            clock.Advance(TimeSpan.FromSeconds(seconds));
            Put(session, last, session.Game!.Sudoku.Solution[last]);
        }

        await Assert.That(session.Win!.NewFastest).IsTrue();
        await Assert.That(session.Stats[Difficulty.Hard].Fastest).IsEqualTo(TimeSpan.FromSeconds(40));
    }

    [Test]
    [Arguments("{ not json")]
    [Arguments("null")]
    [Arguments("{}")]
    [Arguments("""{"Version":1,"Id":null,"Cells":"","Undo":[],"Redo":[],"ElapsedMs":0}""")]
    [Arguments("""{"Version":7,"Id":"x","Cells":"","Undo":[],"Redo":[],"ElapsedMs":0}""")]
    public async Task DamagedFiles_AreIgnoredAndOverwrittenOnTheNextSave(string content)
    {
        using var dir = new TempDirectory();
        Directory.CreateDirectory(dir.Path);
        File.WriteAllText(dir.SaveFile, content);
        File.WriteAllText(dir.StatsFile, content);

        var session = Open(dir);
        await Assert.That(session.Game).IsNull();
        await Assert.That(session.Stats).IsEqualTo(Statistics.Empty);

        session.TryStart(null, Difficulty.Hard, out _);
        await Assert.That(Open(dir).Game).IsNotNull();
        await Assert.That(Open(dir).Stats[Difficulty.Hard].Started).IsEqualTo(1);
    }

    [Test]
    public async Task StatisticsWithAnotherVersion_AreTreatedAsEmpty()
    {
        using var dir = new TempDirectory();
        Directory.CreateDirectory(dir.Path);
        File.WriteAllText(dir.StatsFile, """{"Version":9,"ByDifficulty":[]}""");

        await Assert.That(Open(dir).Stats).IsEqualTo(Statistics.Empty);
    }

    [Test]
    public async Task UnwritableStorage_NeverStopsTheGameOrTheWin()
    {
        using var dir = new TempDirectory();
        Directory.CreateDirectory(dir.SaveFile);
        Directory.CreateDirectory(dir.StatsFile);
        var session = Open(dir);

        session.TryStart(null, Difficulty.Hard, out _);
        var last = PlayAllButLast(session);
        Put(session, last, session.Game!.Sudoku.Solution[last]);

        await Assert.That(session.Win).IsNotNull();
        await Assert.That(session.Stats[Difficulty.Hard].Won).IsEqualTo(1);
    }

    [Test]
    public async Task Check_NeverTouchesTheSave()
    {
        using var dir = new TempDirectory();
        var session = Open(dir);
        session.TryStart(null, Difficulty.Hard, out _);
        var cell = TuiHelpers.FirstEmpty(session.Game!);
        Put(session, cell, TuiHelpers.Wrong(session.Game!, cell));
        session.Game!.Check();

        await Assert.That(Open(dir).Game!.MarkAt(cell)).IsEqualTo(CellMark.None);
    }
}

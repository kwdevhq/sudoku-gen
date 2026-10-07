namespace SudokuGen.Tui;

internal sealed record ThemeSetting(bool Light);

internal sealed record WinResult(Sudoku Sudoku, TimeSpan Time, bool NewFastest);

/// <summary>
/// Ties a <see cref="Game"/> to the save file and the statistics: starts, resumes and resets games, persists every change
/// and, when the game is solved, removes the save and records the win. Contains no screen code.
/// </summary>
internal sealed class Session
{
    private readonly TimeProvider clock;
    private readonly Random? random;
    private readonly JsonFile saveFile;
    private readonly JsonFile statsFile;
    private readonly JsonFile themeFile;

    public Session(string directory, TimeProvider clock, Random? random = null)
    {
        this.clock = clock;
        this.random = random;
        saveFile = new JsonFile(Path.Combine(directory, "tui-save.json"));
        statsFile = new JsonFile(Path.Combine(directory, "tui-stats.json"));

        themeFile = new JsonFile(Path.Combine(directory, "tui-theme.json"));
        LightTheme = themeFile.Read<ThemeSetting>()?.Light ?? false;
        Stats = statsFile.Read<Statistics>() is { IsValid: true } stored ? stored : Statistics.Empty;
        Game = saveFile.Read<GameSave>() is { } save ? Game.Restore(save, clock) : null;
    }

    public Statistics Stats { get; private set; }

    public bool LightTheme { get; private set; }

    /// <summary>The game being played, or null before one is started. Operations below require one.</summary>
    public Game? Game { get; private set; }

    /// <summary>Set once the current game is solved.</summary>
    public WinResult? Win { get; private set; }

    /// <summary>True when leaving or restarting would throw away entries.</summary>
    public bool NeedsConfirmation => Game is { IsSolved: false, EntryCount: > 0 };

    /// <summary>Starts a game from an id text, or a random one of <paramref name="difficulty"/> when the text is blank.</summary>
    public bool TryStart(string? idText, Difficulty difficulty, out string? error)
    {
        SudokuId id = default;
        if (!string.IsNullOrWhiteSpace(idText) && !SudokuId.TryParse(idText, out id))
        {
            error = "That is not a valid Sudoku id.";
            return false;
        }

        var sudoku = string.IsNullOrWhiteSpace(idText) ? SudokuGenerator.Generate(difficulty, random) : SudokuGenerator.FromId(id);
        AbandonCurrent();
        Game = new Game(sudoku, clock);
        Win = null;
        Record(Stats.Started(sudoku.Difficulty));
        saveFile.Write(Game.ToSave());
        error = null;
        return true;
    }

    public bool Enter(char digit) => Changed(Game!.Enter(digit));

    public bool Undo() => Changed(Game!.Undo());

    public bool Redo() => Changed(Game!.Redo());

    /// <summary>Starts the same puzzle over; counts as a new game. Does nothing without entries.</summary>
    public bool Reset()
    {
        if (Game is not { IsSolved: false, EntryCount: > 0 })
        {
            return false;
        }

        AbandonCurrent();
        Record(Stats.Started(Game.Sudoku.Difficulty));
        Game.Restart();
        saveFile.Write(Game.ToSave());
        return true;
    }

    /// <summary>Stores the current state, including the elapsed time. Call when leaving.</summary>
    public void SaveOnExit()
    {
        if (Game is { IsSolved: false })
        {
            saveFile.Write(Game.ToSave());
        }
    }

    public void ToggleTheme()
    {
        LightTheme = !LightTheme;
        themeFile.Write(new ThemeSetting(LightTheme));
    }

    private bool Changed(bool changed)
    {
        if (!changed)
        {
            return false;
        }

        if (Game!.IsSolved)
        {
            saveFile.Delete();
            Record(Stats.Won(Game.Sudoku.Difficulty, Game.Elapsed, out var newFastest));
            Win = new WinResult(Game.Sudoku, Game.Elapsed, newFastest);
        }
        else
        {
            saveFile.Write(Game.ToSave());
        }

        return true;
    }

    private void AbandonCurrent()
    {
        if (NeedsConfirmation)
        {
            Record(Stats.Abandoned(Game!.Sudoku.Difficulty));
        }
    }

    private void Record(Statistics stats)
    {
        Stats = stats;
        statsFile.Write(stats);
    }
}

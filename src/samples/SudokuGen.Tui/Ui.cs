namespace SudokuGen.Tui;

internal enum Screen
{
    Start,
    Playing,
    Confirm,
    Animating,
    Won,
    Stats,
}

internal enum StartFocus
{
    Id,
    Start,
    Statistics,
}

internal enum PendingAction
{
    Reset,
    NewGame,
}

/// <summary>
/// The whole user interface minus the terminal: which screen is shown, what keys and clicks do, and what gets painted.
/// The Terminal.Gui view only forwards input and supplies an <see cref="ICanvas"/>.
/// </summary>
internal sealed partial class Ui(Session session, TimeProvider clock, ISystem system)
{
    public const int MinWidth = 80;
    public const int MinHeight = 30;
    private const int MaxIdLength = 14;

    private static readonly TimeSpan WaveDuration = TimeSpan.FromSeconds(1);
    private static readonly Difficulty[] Difficulties = Enum.GetValues<Difficulty>();

    private int width = MinWidth;
    private int height = MinHeight;
    private long waveStart;
    private PendingAction pending;
    private Difficulty chosen = Difficulty.Medium;
    private StartFocus focus = StartFocus.Start;
    private string idText = string.Empty;
    private string? startError;
    private string? status;

    public Screen Screen { get; private set; } = session.Game is null ? Screen.Start : Screen.Playing;

    private static readonly Screen[] LiveScreens = [Screen.Playing, Screen.Animating, Screen.Won];

    public bool QuitRequested { get; private set; }

    /// <summary>Called regularly: advances the win animation. Returns true when something on screen changed by itself.</summary>
    public bool Tick()
    {
        if (Screen == Screen.Animating && clock.GetElapsedTime(waveStart) >= WaveDuration)
        {
            Screen = Screen.Won;
        }

        return LiveScreens.Contains(Screen);
    }

    public void HandleKey(Input input)
    {
        if (input is { Key: "t", Ctrl: true })
        {
            ToggleTheme();
            return;
        }

        if (Screen == Screen.Start)
        {
            StartKey(input);
        }
        else if (Screen == Screen.Playing)
        {
            PlayingKey(input);
        }
        else if (Screen == Screen.Confirm)
        {
            ConfirmKey(input);
        }
        else if (Screen == Screen.Won)
        {
            WonKey(input);
        }
        else if (Screen == Screen.Stats)
        {
            Screen = Screen.Start;
        }
    }
    public void HandleClick(int x, int y, int screenWidth, int screenHeight)
    {
        width = screenWidth;
        height = screenHeight;
        if (Screen == Screen.Stats)
        {
            Screen = Screen.Start;
            return;
        }

        var button = Buttons().FirstOrDefault(b => y == b.Y && x >= b.X && x < b.X + b.Width);
        if (button is not null)
        {
            button.Click?.Invoke();
        }
        else if (Screen == Screen.Playing && CellAt(x, y) is { } cell)
        {
            session.Game!.Select(cell);
        }
    }

    private void PlayingKey(Input input)
    {
        var game = session.Game!;
        switch (input)
        {
            case { Key: "Up" }:
                game.Move(-1, 0);
                break;
            case { Key: "Down" }:
                game.Move(1, 0);
                break;
            case { Key: "Left" }:
                game.Move(0, -1);
                break;
            case { Key: "Right" }:
                game.Move(0, 1);
                break;
            case { Key: [>= '1' and <= '9' and var digit] }:
                Enter(digit);
                break;
            case { Key: "0" or "Backspace" or "Delete" }:
                Enter(Game.Empty);
                break;
            case { Key: "z", Ctrl: true }:
                Edited(session.Undo());
                break;
            case { Key: "y", Ctrl: true }:
                Edited(session.Redo());
                break;
            case { Key: "c", Ctrl: true }:
                CopyId();
                break;
            case { Key: "c" }:
                Check();
                break;
            case { Key: "t" }:
                ToggleTheme();
                break;
            case { Key: "r" }:
                RequestReset();
                break;
            case { Key: "n" }:
                RequestNewGame();
                break;
            case { Key: "q" or "Esc" }:
                Quit();
                break;
        }
    }

    private void ToggleTheme()
    {
        session.ToggleTheme();
        Theme.Light = session.LightTheme;
    }

    private void Enter(char digit)
    {
        Edited(session.Enter(digit));
        if (session.Win is not null)
        {
            Screen = Screen.Animating;
            waveStart = clock.GetTimestamp();
        }
    }

    private void Edited(bool changed)
    {
        if (changed)
        {
            status = null;
        }
    }

    private void CopyId()
    {
        system.WriteClipboard(session.Game!.Sudoku.Id.ToString());
        status = "Id copied.";
    }

    private void Check()
    {
        var result = session.Game!.Check();
        status = $"{result.Wrong} wrong, {result.Empty} empty";
    }

    private void RequestReset()
    {
        if (session.NeedsConfirmation)
        {
            Ask(PendingAction.Reset);
        }
        else
        {
            status = "Nothing to reset yet.";
        }
    }

    private void RequestNewGame()
    {
        if (session.NeedsConfirmation)
        {
            Ask(PendingAction.NewGame);
        }
        else
        {
            GoToStart();
        }
    }

    private void Ask(PendingAction action)
    {
        pending = action;
        Screen = Screen.Confirm;
    }

    private void GoToStart()
    {
        startError = null;
        Screen = Screen.Start;
    }

    private void Quit()
    {
        session.SaveOnExit();
        QuitRequested = true;
    }

    private void ConfirmKey(Input input)
    {
        switch (input.Key)
        {
            case "y" or "Enter":
                Confirmed();
                break;
            case "n" or "Esc":
                Screen = Screen.Playing;
                break;
        }
    }

    private void Confirmed()
    {
        if (pending == PendingAction.Reset)
        {
            session.Reset();
            status = null;
            Screen = Screen.Playing;
        }
        else
        {
            GoToStart();
        }
    }

    private void WonKey(Input input)
    {
        switch (input.Key)
        {
            case "n" or "Enter":
                GoToStart();
                break;
            case "c":
                CopyId();
                break;
            case "t":
                ToggleTheme();
                break;
            case "q" or "Esc":
                Quit();
                break;
        }
    }

    private void StartKey(Input input)
    {
        switch (input.Key)
        {
            case "Up":
                chosen = Difficulties[Math.Max(0, Array.IndexOf(Difficulties, chosen) - 1)];
                break;
            case "Down":
                chosen = Difficulties[Math.Min(Difficulties.Length - 1, Array.IndexOf(Difficulties, chosen) + 1)];
                break;
            case "Tab":
                focus = (StartFocus)(((int)focus + 1) % Enum.GetValues<StartFocus>().Length);
                break;
            case "Enter":
                if (focus == StartFocus.Statistics)
                {
                    Screen = Screen.Stats;
                }
                else
                {
                    StartGame();
                }

                break;
            case "Esc":
                LeaveStart();
                break;
            case "v" when input.Ctrl:
                Paste(system.ReadClipboard());
                break;
            case "Backspace" when focus == StartFocus.Id:
                idText = idText.Length > 0 ? idText[..^1] : idText;
                break;
            case [var c] when !input.Ctrl && IsIdChar(c):
                focus = StartFocus.Id;
                idText = idText.Length < MaxIdLength ? idText + char.ToUpperInvariant(c) : idText;
                break;
        }
    }

    /// <summary>Pasted text goes into the id field of the start screen; anything that cannot be part of an id is dropped.</summary>
    public void Paste(string? text)
    {
        if (Screen != Screen.Start)
        {
            return;
        }

        focus = StartFocus.Id;
        var clean = new string((text ?? string.Empty).Where(IsIdChar).Select(char.ToUpperInvariant).ToArray());
        idText = (idText + clean)[..Math.Min(MaxIdLength, idText.Length + clean.Length)];
    }

    private static bool IsIdChar(char c) => char.IsAsciiLetterOrDigit(c) || c == '-';

    private void LeaveStart()
    {
        if (session.Game is { IsSolved: false })
        {
            Screen = Screen.Playing;
        }
        else
        {
            Quit();
        }
    }

    private void StartGame()
    {
        if (session.TryStart(idText, chosen, out startError))
        {
            idText = string.Empty;
            status = null;
            Screen = Screen.Playing;
        }
    }
}

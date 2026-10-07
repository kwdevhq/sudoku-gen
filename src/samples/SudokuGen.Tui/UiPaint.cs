namespace SudokuGen.Tui;

internal sealed partial class Ui
{
    private const int BoardWidth = 47;
    private const int BoardHeight = 20;
    private const int PanelGap = 3;
    private const int PanelWidth = 17;
    private const int TotalWidth = BoardWidth + PanelGap + PanelWidth;
    private const int CellWidth = 5;
    private const int CellHeight = 2;
    private const int ConfirmWidth = 52;
    private const int ConfirmHeight = 7;
    private const int WonWidth = 56;
    private const int WonHeight = 15;
    private const int StatsWidth = 56;
    private const int StatsHeight = 10;
    private const int StartWidth = 40;
    private const int StartHeight = 16;

    private bool Fits => width >= MinWidth && height >= MinHeight;

    private int OriginX => (width - TotalWidth) / 2;

    private int OriginY => (height - MinHeight) / 2;

    public void Paint(ICanvas canvas)
    {
        width = canvas.Width;
        height = canvas.Height;
        for (var y = 0; y < height; y++)
        {
            canvas.Put(0, y, new string(' ', width), Theme.Normal);
        }

        if (!Fits)
        {
            Text(canvas, 0, 0, $"Please enlarge the terminal to at least {MinWidth}x{MinHeight}.", Theme.Normal);
            return;
        }

        switch (Screen)
        {
            case Screen.Start:
                PaintStart(canvas);
                break;
            case Screen.Stats:
                PaintStart(canvas);
                PaintStatsDialog(canvas);
                break;
            default:
                PaintPlaying(canvas);
                if (Screen == Screen.Confirm)
                {
                    PaintConfirm(canvas);
                }
                else if (Screen == Screen.Won)
                {
                    PaintWon(canvas);
                }

                break;
        }

    }

    private static void Text(ICanvas canvas, int x, int y, string text, Style style) => canvas.Put(x, y, text, style);

    private (int X, int Y) CellOrigin(int cell)
    {
        var row = cell / Game.Size;
        var column = cell % Game.Size;
        return (OriginX + (column * CellWidth) + (column / 3), OriginY + 1 + (row * CellHeight) + (row / 3));
    }

    private int? CellAt(int x, int y)
    {
        for (var cell = 0; cell < Game.CellCount; cell++)
        {
            var (cx, cy) = CellOrigin(cell);
            if (x >= cx && x < cx + CellWidth && y >= cy && y < cy + CellHeight)
            {
                return cell;
            }
        }

        return null;
    }

    private List<Button> Buttons()
    {
        if (!Fits)
        {
            return [];
        }

        if (Screen == Screen.Playing)
        {
            return PlayingButtons();
        }

        if (Screen == Screen.Start)
        {
            return StartButtons();
        }

        if (Screen == Screen.Confirm)
        {
            return ConfirmButtons();
        }

        return Screen == Screen.Won ? WonButtons() : [];
    }

    private List<Button> PlayingButtons()
    {
        var game = session.Game!;
        var px = OriginX + BoardWidth + PanelGap;
        var py = OriginY + 1;
        var buttons = new List<Button>();
        for (var digit = 1; digit <= 9; digit++)
        {
            var d = (char)('0' + digit);
            buttons.Add(new Button(px + (((digit - 1) % 3) * 6), py + 4 + (((digit - 1) / 3) * 2), CellWidth, Format.Center(d.ToString(), CellWidth), Theme.ButtonStyle, () => Enter(d)));
        }

        buttons.Add(new Button(px, py + 10, PanelWidth, Format.Center("Erase", PanelWidth), Theme.ButtonStyle, () => Enter(Game.Empty)));
        buttons.Add(Small(px, py + 12, "Check", Check));
        buttons.Add(Small(px + 9, py + 12, "Undo", game.CanUndo ? () => Edited(session.Undo()) : null));
        buttons.Add(Small(px, py + 14, "Redo", game.CanRedo ? () => Edited(session.Redo()) : null));
        buttons.Add(Small(px + 9, py + 14, "Reset", RequestReset));
        buttons.Add(Small(px, py + 16, "New", RequestNewGame));
        buttons.Add(Small(px + 9, py + 16, "Quit", Quit));
        return buttons;
    }

    private static Button Small(int x, int y, string label, Action? click) =>
        new(x, y, 8, Format.Center(label, 8), click is null ? Theme.ButtonOff : Theme.ButtonStyle, click);

    private List<Button> StartButtons()
    {
        var sx = (width - StartWidth) / 2;
        var sy = (height - StartHeight) / 2;
        var buttons = new List<Button>();
        foreach (var difficulty in Difficulties)
        {
            var selected = difficulty == chosen;
            var label = $"{(selected ? '●' : '○')} {difficulty}";
            buttons.Add(new Button(sx + 10, sy + 3 + (int)difficulty, 20, " " + label, selected ? Theme.ButtonOn : Theme.ButtonStyle, () => chosen = difficulty));
        }

        var idStyle = focus == StartFocus.Id ? new Style(Theme.Dark, Theme.Text) : Theme.ButtonStyle;
        buttons.Add(new Button(sx + 8, sy + 9, 24, " " + idText + (focus == StartFocus.Id ? "▏" : string.Empty), idStyle, () => focus = StartFocus.Id));
        buttons.Add(new Button(sx + 4, sy + 13, 14, Format.Center("Start", 14), focus == StartFocus.Start ? Theme.ButtonOn : Theme.ButtonStyle, StartGame));
        buttons.Add(new Button(sx + 22, sy + 13, 14, Format.Center("Statistics", 14), focus == StartFocus.Statistics ? Theme.ButtonOn : Theme.ButtonStyle, () => Screen = Screen.Stats));
        return buttons;
    }

    private List<Button> ConfirmButtons()
    {
        var (dx, dy) = Dialog(ConfirmWidth, ConfirmHeight);
        return
        [
            new Button(dx + (ConfirmWidth / 2) - 12, dy + 5, 10, Format.Center("Yes (Y)", 10), Theme.ButtonOn, Confirmed),
            new Button(dx + (ConfirmWidth / 2) + 2, dy + 5, 10, Format.Center("No (N)", 10), Theme.ButtonStyle, () => Screen = Screen.Playing),
        ];
    }

    private List<Button> WonButtons()
    {
        var (dx, dy) = Dialog(WonWidth, WonHeight);
        return
        [
            new Button(dx + 8, dy + 13, 16, Format.Center("New game (N)", 16), Theme.ButtonOn, GoToStart),
            new Button(dx + WonWidth - 8 - 12, dy + 13, 12, Format.Center("Quit (Q)", 12), Theme.ButtonStyle, Quit),
        ];
    }

    private (int X, int Y) Dialog(int dialogWidth, int dialogHeight) => ((width - dialogWidth) / 2, (height - dialogHeight) / 2);

    private void PaintPlaying(ICanvas canvas)
    {
        var game = session.Game!;
        var px = OriginX + BoardWidth + PanelGap;
        var py = OriginY + 1;
        Text(canvas, OriginX, OriginY, "S U D O K U", Theme.Title);

        var progress = Screen switch
        {
            Screen.Animating => Math.Min(1.0, clock.GetElapsedTime(waveStart) / WaveDuration),
            Screen.Won => 1.0,
            _ => 0.0,
        };
        for (var cell = 0; cell < Game.CellCount; cell++)
        {
            var (cx, cy) = CellOrigin(cell);
            var digit = game[cell] == Game.Empty ? " " : game[cell].ToString();
            var shown = cell == game.Selected ? $"[{digit}]" : digit;
            Text(canvas, cx, cy, Format.Center(shown, CellWidth), CellStyle(game, cell, progress));
            Text(canvas, cx, cy + 1, new string(' ', CellWidth), CellStyle(game, cell, progress));
        }

        Text(canvas, px, py, game.Sudoku.Difficulty.ToString().ToUpperInvariant(), Theme.Title);
        Text(canvas, px, py + 1, "Time  " + Format.Time(game.Elapsed), Theme.Normal);
        Text(canvas, px, py + 2, "Id " + game.Sudoku.Id, Theme.Muted);
        Text(canvas, OriginX, OriginY + 22, status ?? string.Empty, Theme.Normal);
        PaintButtons(canvas, PlayingButtons());
        Text(canvas, OriginX, OriginY + 23, "Arrows/click select  1-9 enter  0/Del erase  Ctrl+Z/Y undo/redo  C check", Theme.Muted);
    }

    private static Style CellStyle(Game game, int cell, double progress)
    {
        var row = cell / Game.Size;
        var column = cell % Game.Size;
        var selected = game.Selected;
        var background = (row + column) % 2 == 0 ? Theme.CellLight : Theme.CellDark;
        var foreground = game.IsGiven(cell) ? Theme.Given : Theme.Entry;

        if (cell == selected)
        {
            background = Theme.Selected;
        }
        else if (game[selected] != Game.Empty && game[cell] == game[selected])
        {
            background = Theme.SameDigit;
        }
        else if (Game.SharesUnit(selected, cell))
        {
            background = Theme.UnitHighlight;
        }

        if (game.HasConflict(cell))
        {
            foreground = Theme.Conflict;
        }

        switch (game.MarkAt(cell))
        {
            case CellMark.Right:
                (foreground, background) = (Theme.Dark, Theme.Right);
                break;
            case CellMark.Wrong:
                (foreground, background) = (Theme.Given, Theme.Wrong);
                break;
        }

        var distance = row + column - (progress * (2 * (Game.Size - 1) + 3));
        if (progress > 0 && distance <= 0)
        {
            (foreground, background) = (Theme.Dark, Theme.Right);
        }
        else if (progress > 0 && distance < 2)
        {
            (foreground, background) = (Theme.Dark, Theme.Flash);
        }

        return new Style(foreground, background);
    }

    private void PaintStart(ICanvas canvas)
    {
        var sx = (width - StartWidth) / 2;
        var sy = (height - StartHeight) / 2;
        Text(canvas, sx, sy, Format.Center("S U D O K U", StartWidth), Theme.Title);
        Text(canvas, sx, sy + 1, Format.Center("Choose a difficulty", StartWidth), Theme.Muted);
        Text(canvas, sx, sy + 8, Format.Center("Sudoku id (optional, to replay one)", StartWidth), Theme.Muted);
        Text(canvas, sx, sy + 11, Format.Center(startError ?? string.Empty, StartWidth), Theme.Error);
        var escape = session.Game is { IsSolved: false } ? "Esc back" : "Esc quit";
        Text(canvas, sx - 6, sy + 15, Format.Center($"Up/Down level  Tab focus  Enter start  {escape}", StartWidth + 12), Theme.Muted);
        PaintButtons(canvas, StartButtons());
    }

    private void PaintDialog(ICanvas canvas, int dialogWidth, int dialogHeight)
    {
        var (dx, dy) = Dialog(dialogWidth, dialogHeight);
        for (var y = 0; y < dialogHeight; y++)
        {
            var edge = y == 0 || y == dialogHeight - 1;
            var left = y == 0 ? '╭' : edge ? '╰' : '│';
            var right = y == 0 ? '╮' : edge ? '╯' : '│';
            var middle = edge ? '─' : ' ';
            Text(canvas, dx, dy + y, left + new string(middle, dialogWidth - 2) + right, Theme.Dialog);
        }
    }

    private void PaintConfirm(ICanvas canvas)
    {
        PaintDialog(canvas, ConfirmWidth, ConfirmHeight);
        var (dx, dy) = Dialog(ConfirmWidth, ConfirmHeight);
        var (title, detail) = pending == PendingAction.Reset
            ? ("Reset this puzzle?", "Your entries are cleared. It counts as a new game.")
            : ("Start a new game?", "The current game is given up and not counted as won.");
        Text(canvas, dx + 1, dy + 1, Format.Center(title, ConfirmWidth - 2), Theme.DialogTitle);
        Text(canvas, dx + 1, dy + 3, Format.Center(detail, ConfirmWidth - 2), Theme.Dialog);
        PaintButtons(canvas, ConfirmButtons());
    }

    private void PaintWon(ICanvas canvas)
    {
        PaintDialog(canvas, WonWidth, WonHeight);
        var (dx, dy) = Dialog(WonWidth, WonHeight);
        var win = session.Win!;
        Text(canvas, dx + 1, dy + 1, Format.Center("Solved!", WonWidth - 2), Theme.DialogTitle);
        Text(canvas, dx + 1, dy + 3, Format.Center($"Solved in {Format.Time(win.Time)}", WonWidth - 2), Theme.Dialog);
        Text(canvas, dx + 1, dy + 4, Format.Center(win.NewFastest ? "New personal best!" : string.Empty, WonWidth - 2), Theme.DialogTitle);
        Text(canvas, dx + 1, dy + 5, Format.Center($"Id {win.Sudoku.Id}", WonWidth - 2), Theme.DialogMuted);
        PaintTable(canvas, dx + 3, dy + 7, win.Sudoku.Difficulty);
        PaintButtons(canvas, WonButtons());
    }

    private void PaintStatsDialog(ICanvas canvas)
    {
        PaintDialog(canvas, StatsWidth, StatsHeight);
        var (dx, dy) = Dialog(StatsWidth, StatsHeight);
        Text(canvas, dx + 1, dy + 1, Format.Center("Statistics", StatsWidth - 2), Theme.DialogTitle);
        PaintTable(canvas, dx + 3, dy + 3, null);
        Text(canvas, dx + 1, dy + 8, Format.Center("Press any key", StatsWidth - 2), Theme.DialogMuted);
    }

    private static void PaintButtons(ICanvas canvas, List<Button> buttons)
    {
        foreach (var button in buttons)
        {
            Text(canvas, button.X, button.Y, button.Label.PadRight(button.Width)[..button.Width], button.Style);
        }
    }

    private void PaintTable(ICanvas canvas, int x, int y, Difficulty? highlight)
    {
        Text(canvas, x, y, $"{string.Empty,-8}{"Games",5}{"Won",6}{"Win%",6}{"Fastest",9}{"Average",9}{"Streak",7}", Theme.DialogMuted);
        foreach (var difficulty in Difficulties)
        {
            var s = session.Stats[difficulty];
            var row = $"{difficulty,-8}{s.Started,5}{s.Won,6}{Format.Percent(s.WinRate),6}{Format.Time(s.Fastest),9}{Format.Time(s.Average),9}{$"{s.Streak}/{s.BestStreak}",7}";
            Text(canvas, x, y + 1 + (int)difficulty, row, difficulty == highlight ? new Style(Theme.Dark, Theme.Accent) : Theme.Dialog);
        }
    }
}

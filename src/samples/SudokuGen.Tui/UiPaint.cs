namespace SudokuGen.Tui;

internal sealed partial class Ui
{
    private const int PadWidth = 6;
    private const int CellWidth = 6;
    private const int CellHeight = 3;
    private const int LayoutHeight = MinHeight;
    private const int TitleRows = 5;
    private const int BoardWidth = (Game.Size * CellWidth) + 2;
    private const int TotalWidth = BoardWidth + PanelGap + PanelWidth;
    private const int PanelGap = 2;
    private const int PanelWidth = 22;
    private const string LicenseUrl = "https://github.com/kwdevhq/sudoku-gen/blob/main/LICENSE";
    private const string CompanyUrl = "https://kw.dev";
    private const int ConfirmWidth = 52;
    private const int ConfirmHeight = 7;
    private const int WonWidth = 56;
    private const int WonHeight = 16;
    private const int StatsWidth = 56;
    private const int StatsHeight = 11;
    private const int StartWidth = 40;
    private const int StartHeight = 23;
    private const int MenuWidth = 40;
    private const int MenuHeight = 9;
    private const int FooterWidth = 39;

    private bool Fits => width >= MinWidth && height >= MinHeight;

    private int OriginX => (width - TotalWidth) / 2;

    private int OriginY => (height - LayoutHeight) / 2;

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
                else if (Screen == Screen.Menu)
                {
                    PaintMenu(canvas);
                }

                break;
        }

    }

    private static void Text(ICanvas canvas, int x, int y, string text, Style style) => canvas.Put(x, y, text, style);

    private (int X, int Y) CellOrigin(int cell)
    {
        var row = cell / Game.Size;
        var column = cell % Game.Size;
        return (OriginX + (column * CellWidth) + (column / 3), OriginY + (row * CellHeight) + (row / 3));
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

        if (Screen == Screen.Menu)
        {
            return MenuButtons();
        }

        return Screen == Screen.Won ? WonButtons() : [];
    }

    private int PanelX => OriginX + BoardWidth + PanelGap;

    private List<Button> PlayingButtons()
    {
        var game = session.Game!;
        var px = PanelX;
        var py = OriginY;
        var buttons = new List<Button>();
        for (var digit = 1; digit <= 9; digit++)
        {
            var d = (char)('0' + digit);
            var style = game.IsDigitComplete(d) ? Theme.ButtonDone : Theme.ButtonStyle;
            buttons.Add(new Button(px + (((digit - 1) % 3) * 7), py + 5 + (((digit - 1) / 3) * 2), PadWidth, Format.Center(d.ToString(), PadWidth), style, () => Enter(d)));
        }

        buttons.Add(new Button(px + 17, py + 3, 5, "Copy", Theme.ButtonStyle, CopyId));
        buttons.Add(new Button(px, py + 11, PanelWidth, Format.Center("Erase", PanelWidth), Theme.ButtonStyle, () => Enter(Game.Empty)));
        buttons.Add(Small(px, py + 13, "Check", Check));
        buttons.Add(Small(px + 12, py + 13, "Undo", game.CanUndo ? () => Edited(session.Undo()) : null));
        buttons.Add(Small(px, py + 15, "Redo", game.CanRedo ? () => Edited(session.Redo()) : null));
        buttons.Add(Small(px + 12, py + 15, "Reset", RequestReset));
        buttons.Add(Small(px, py + 17, "New", RequestNewGame));
        buttons.Add(Small(px + 12, py + 17, "Quit", Quit));
        buttons.AddRange(FooterButtons(OriginX));
        return buttons;
    }

    private static Button Small(int x, int y, string label, Action? click) =>
        new(x, y, 10, Format.Center(label, 10), click is null ? Theme.ButtonOff : Theme.ButtonStyle, click);

    private List<Button> FooterButtons(int x)
    {
        var y = OriginY + LayoutHeight - 1;
        return
        [
            new Button(x + 11, y, 6, "kw.dev", Theme.Company, () => system.OpenUrl(CompanyUrl)),
            new Button(x + 20, y, 11, "MIT License", Theme.Muted, () => system.OpenUrl(LicenseUrl)),
            new Button(x + 34, y, 5, "Theme", Theme.Muted, ToggleTheme),
        ];
    }

    private void PaintFooter(ICanvas canvas, int x)
    {
        var y = OriginY + LayoutHeight - 1;
        Text(canvas, x, y, "Created by", Theme.Muted);
        Text(canvas, x + 17, y, " · ", Theme.Muted);
        Text(canvas, x + 31, y, " · ", Theme.Muted);
        PaintButtons(canvas, FooterButtons(x));
    }
    private List<Button> StartButtons()
    {
        var sx = (width - StartWidth) / 2;
        var sy = ((height - StartHeight) / 2) + TitleRows;
        var buttons = new List<Button>();
        foreach (var difficulty in Difficulties)
        {
            var selected = difficulty == chosen;
            var label = $"{(selected ? '●' : '○')} {difficulty}";
            buttons.Add(new Button(sx + 10, sy + 3 + (int)difficulty, 20, " " + label, selected ? Theme.ButtonOn : Theme.ButtonStyle, () => chosen = difficulty));
        }

        var idStyle = focus == StartFocus.Id ? new Style(Theme.Background, Theme.Text) : Theme.ButtonStyle;
        buttons.Add(new Button(sx + 8, sy + 9, 24, " " + idText + (focus == StartFocus.Id ? "▏" : string.Empty), idStyle, () => focus = StartFocus.Id));
        buttons.Add(new Button(sx + 4, sy + 13, 14, Format.Center("Start", 14), focus == StartFocus.Start ? Theme.ButtonOn : Theme.ButtonStyle, StartGame));
        buttons.Add(new Button(sx + 22, sy + 13, 14, Format.Center("Statistics", 14), focus == StartFocus.Statistics ? Theme.ButtonOn : Theme.ButtonStyle, () => Screen = Screen.Stats));
        buttons.Add(new Button(sx + 13, sy + 15, 14, Format.Center("Quit", 14), focus == StartFocus.Quit ? Theme.ButtonOn : Theme.ButtonStyle, Quit));
        buttons.AddRange(FooterButtons(StartFooterX));
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

    private List<Button> MenuButtons()
    {
        var (dx, dy) = Dialog(MenuWidth, MenuHeight);
        var x = dx + ((MenuWidth - 20) / 2);
        return
        [
            new Button(x, dy + 3, 20, Format.Center("Resume", 20), Theme.ButtonOn, () => Screen = Screen.Playing),
            new Button(x, dy + 5, 20, Format.Center("Main menu", 20), Theme.ButtonStyle, GoToStart),
            new Button(x, dy + 7, 20, Format.Center("Quit game", 20), Theme.ButtonStyle, Quit),
        ];
    }

    private List<Button> WonButtons()
    {
        var (dx, dy) = Dialog(WonWidth, WonHeight);
        return
        [
            new Button(dx + 4, dy + 14, 16, Format.Center("New game (N)", 16), Theme.ButtonOn, GoToStart),
            new Button(dx + 22, dy + 14, 13, Format.Center("Copy id (C)", 13), Theme.ButtonStyle, CopyId),
            new Button(dx + WonWidth - 4 - 12, dy + 14, 12, Format.Center("Quit (Q)", 12), Theme.ButtonStyle, Quit),
        ];
    }
    private int StartFooterX => (width - FooterWidth) / 2;

    private (int X, int Y) Dialog(int dialogWidth, int dialogHeight) => ((width - dialogWidth) / 2, (height - dialogHeight) / 2);

    private void PaintPlaying(ICanvas canvas)
    {
        var game = session.Game!;
        var px = PanelX;
        var py = OriginY;

        var progress = Screen switch
        {
            Screen.Animating => Math.Min(1.0, clock.GetElapsedTime(waveStart) / WaveDuration),
            Screen.Won => 1.0,
            _ => 0.0,
        };
        for (var cell = 0; cell < Game.CellCount; cell++)
        {
            PaintCell(canvas, game, cell, CellStyle(game, cell, progress));
        }

        Text(canvas, px, py, "S U D O K U", Theme.Title);
        Text(canvas, px, py + 1, game.Sudoku.Difficulty.ToString().ToUpperInvariant(), Theme.Title);
        Text(canvas, px, py + 2, "Time  " + Format.Time(game.Elapsed), Theme.Normal);
        Text(canvas, px, py + 3, "Id " + game.Sudoku.Id, Theme.Muted);
        var line = 0;
        foreach (var text in Format.Wrap(status ?? string.Empty, PanelWidth))
        {
            Text(canvas, px, py + 19 + line++, text, Theme.Normal);
        }

        line = 0;
        foreach (var (key, meaning) in Help)
        {
            Text(canvas, px, py + 22 + line, key, Theme.Normal);
            Text(canvas, px + 9, py + 22 + line++, meaning, Theme.Muted);
        }

        PaintButtons(canvas, PlayingButtons());
        PaintFooter(canvas, OriginX);
    }

    private static readonly (string Key, string Meaning)[] Help =
    [
        ("Arrows", "select"),
        ("1-9", "enter digit"),
        ("0 Del", "erase"),
        ("C", "check"),
        ("Ctrl+Z/Y", "undo / redo"),
        ("T", "theme"),
        ("Esc", "menu"),
    ];
    private void PaintCell(ICanvas canvas, Game game, int cell, Style style)
    {
        var (cx, cy) = CellOrigin(cell);
        var selected = cell == game.Selected;
        var digit = game[cell] == Game.Empty ? "  " : ((char)('\uFF10' + (game[cell] - '0'))).ToString();
        var left = cx + ((CellWidth - 2) / 2);
        for (var y = 0; y < CellHeight; y++)
        {
            Text(canvas, cx, cy + y, new string(' ', CellWidth), style);
        }

        var middle = cy + ((CellHeight - 1) / 2);
        Text(canvas, left, middle, digit, style);
        if (selected)
        {
            Text(canvas, left - 1, middle, "[", style);
            Text(canvas, left + 2, middle, "]", style);
        }
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
            background = (row + column) % 2 == 0 ? Theme.UnitHighlight : Theme.UnitHighlightDark;
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
                (foreground, background) = (Theme.OnWrong, Theme.Wrong);
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

    private static readonly string[][] Letters =
    [
        ["█████", "█    ", "█████", "    █", "█████"],
        ["█   █", "█   █", "█   █", "█   █", "█████"],
        ["████ ", "█   █", "█   █", "█   █", "████ "],
        ["█████", "█   █", "█   █", "█   █", "█████"],
        ["█   █", "█  █ ", "███  ", "█  █ ", "█   █"],
        ["█   █", "█   █", "█   █", "█   █", "█████"],
    ];

    private static string BigTitle(int row) => string.Join(' ', Letters.Select(letter => letter[row]));

    private void PaintStart(ICanvas canvas)
    {
        var sx = (width - StartWidth) / 2;
        var sy = ((height - StartHeight) / 2) + TitleRows;
        for (var row = 0; row < TitleRows; row++)
        {
            Text(canvas, sx, sy - TitleRows + row, Format.Center(BigTitle(row), StartWidth), Theme.Title);
        }

        Text(canvas, sx, sy + 1, Format.Center("Choose a difficulty", StartWidth), Theme.Muted);
        Text(canvas, sx, sy + 8, Format.Center("Sudoku id (optional, Ctrl+V pastes)", StartWidth), Theme.Muted);
        Text(canvas, sx, sy + 11, Format.Center(startError ?? string.Empty, StartWidth), Theme.Error);
        var escape = session.Game is { IsSolved: false } ? "Esc back" : "Esc quit";
        Text(canvas, sx - 6, sy + 17, Format.Center($"Up/Down level  Tab focus  Enter start  {escape}", StartWidth + 12), Theme.Muted);
        PaintButtons(canvas, StartButtons());
        PaintFooter(canvas, StartFooterX);
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

    private void PaintMenu(ICanvas canvas)
    {
        PaintDialog(canvas, MenuWidth, MenuHeight);
        var (dx, dy) = Dialog(MenuWidth, MenuHeight);
        Text(canvas, dx + 1, dy + 1, Format.Center("Game menu", MenuWidth - 2), Theme.DialogTitle);
        PaintButtons(canvas, MenuButtons());
        var (hx, hy) = (dx + ((MenuWidth - 20) / 2) + 22, dy + 3);
        Text(canvas, hx, hy, "Esc", Theme.Dialog);
        Text(canvas, hx, hy + 2, "M", Theme.Dialog);
        Text(canvas, hx, hy + 4, "Q", Theme.Dialog);
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
        Text(canvas, dx + 1, dy + 6, Format.Center(status ?? string.Empty, WonWidth - 2), Theme.DialogMuted);
        PaintTable(canvas, dx + 3, dy + 8, win.Sudoku.Difficulty);
        PaintButtons(canvas, WonButtons());
    }

    private void PaintStatsDialog(ICanvas canvas)
    {
        PaintDialog(canvas, StatsWidth, StatsHeight);
        var (dx, dy) = Dialog(StatsWidth, StatsHeight);
        Text(canvas, dx + 1, dy + 1, Format.Center("Statistics", StatsWidth - 2), Theme.DialogTitle);
        PaintTable(canvas, dx + 3, dy + 3, null);
        Text(canvas, dx + 1, dy + 9, Format.Center("Press any key", StatsWidth - 2), Theme.DialogMuted);
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

        var total = session.Stats.Total;
        Text(canvas, x, y + 1 + Difficulties.Length, $"{"Total",-8}{total.Started,5}{total.Won,6}{Format.Percent(total.WinRate),6}", Theme.Dialog);
    }
}

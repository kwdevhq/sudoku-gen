namespace SudokuGen.Tui;

internal readonly record struct Rgb(byte R, byte G, byte B);

internal readonly record struct Style(Rgb Fg, Rgb Bg);

/// <summary>What the screen logic paints on. The Terminal.Gui view implements it; tests record it.</summary>
internal interface ICanvas
{
    int Width { get; }

    int Height { get; }

    void Put(int x, int y, string text, Style style);
}

/// <summary>What the UI needs from the machine it runs on.</summary>
internal interface ISystem
{
    string? ReadClipboard();

    void WriteClipboard(string text);

    void OpenUrl(string url);
}

/// <summary>One key press. <see cref="Key"/> is a single character or a name such as Up, Enter, Esc, Tab, Backspace, Delete.</summary>
internal readonly record struct Input(string Key, bool Ctrl = false)
{
    public static Input Char(char c) => new(c.ToString());
}

/// <summary>A clickable area. A null <see cref="Click"/> means it is shown but disabled.</summary>
internal sealed record Button(int X, int Y, int Width, string Label, Style Style, Action? Click);

internal static class Theme
{
    public static readonly Rgb Background = new(18, 20, 28);
    public static readonly Rgb Text = new(220, 224, 235);
    public static readonly Rgb Dim = new(120, 128, 150);
    public static readonly Rgb Accent = new(255, 200, 80);
    public static readonly Rgb Brand = new(0xd0, 0x69, 0x00);
    public static readonly Rgb CellLight = new(40, 45, 62);
    public static readonly Rgb CellDark = new(32, 36, 50);
    public static readonly Rgb UnitHighlight = new(58, 70, 102);
    public static readonly Rgb UnitHighlightDark = new(50, 61, 90);
    public static readonly Rgb SameDigit = new(100, 78, 156);
    public static readonly Rgb Selected = new(70, 110, 190);
    public static readonly Rgb Given = new(240, 240, 250);
    public static readonly Rgb Entry = new(110, 210, 255);
    public static readonly Rgb Conflict = new(255, 165, 0);
    public static readonly Rgb Right = new(144, 238, 144);
    public static readonly Rgb Wrong = new(205, 55, 55);
    public static readonly Rgb Flash = new(255, 255, 255);
    public static readonly Rgb Dark = new(15, 15, 20);
    public static readonly Rgb ButtonBackground = new(55, 62, 88);
    public static readonly Rgb ButtonDisabled = new(34, 38, 52);
    public static readonly Rgb DialogBackground = new(28, 32, 46);

    public static Style Normal { get; } = new(Text, Background);

    public static Style Muted { get; } = new(Dim, Background);

    public static Style Title { get; } = new(Brand, Background);

    public static Style Company { get; } = new(Brand, Background);

    public static Style Error { get; } = new(new Rgb(255, 110, 110), Background);

    public static Style Dialog { get; } = new(Text, DialogBackground);

    public static Style DialogMuted { get; } = new(Dim, DialogBackground);

    public static Style DialogTitle { get; } = new(Brand, DialogBackground);

    public static Style ButtonStyle { get; } = new(Text, ButtonBackground);

    public static Style ButtonDone { get; } = new(Dim, ButtonDisabled);

    public static Style ButtonOff { get; } = new(Dim, ButtonDisabled);

    public static Style ButtonOn { get; } = new(Dark, Accent);
}

internal static class Format
{
    public static string Time(TimeSpan? time) => time is { } t
        ? (t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes:00}:{t.Seconds:00}")
        : "--:--";

    public static string Percent(double? rate) => rate is { } r ? $"{Math.Round(r * 100):0}%" : "--";

    public static IEnumerable<string> Wrap(string text, int width)
    {
        var line = string.Empty;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length > 0 && line.Length + 1 + word.Length > width)
            {
                yield return line;
                line = word;
            }
            else
            {
                line = line.Length == 0 ? word : line + ' ' + word;
            }
        }

        if (line.Length > 0)
        {
            yield return line;
        }
    }

    public static string Center(string text, int width) =>
        text.Length >= width ? text[..width] : text.PadLeft(((width - text.Length) / 2) + text.Length).PadRight(width);
}

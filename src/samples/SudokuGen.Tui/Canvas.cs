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
    /// <summary>Selects the light palette. Set once at start-up and when the player toggles the theme.</summary>
    public static bool Light { get; set; }

    public static Rgb Background => Pick(new(18, 20, 28), new(246, 247, 251));

    public static Rgb Text => Pick(new(220, 224, 235), new(28, 32, 46));

    public static Rgb Dim => Pick(new(120, 128, 150), new(104, 110, 130));

    public static Rgb Accent => new(255, 200, 80);

    public static Rgb Brand => new(0xd0, 0x69, 0x00);

    public static Rgb CellLight => Pick(new(40, 45, 62), new(236, 239, 246));

    public static Rgb CellDark => Pick(new(32, 36, 50), new(226, 230, 240));

    public static Rgb UnitHighlight => Pick(new(58, 70, 102), new(204, 216, 240));

    public static Rgb UnitHighlightDark => Pick(new(50, 61, 90), new(194, 207, 235));

    public static Rgb SameDigit => Pick(new(100, 78, 156), new(208, 188, 242));

    public static Rgb Selected => Pick(new(70, 110, 190), new(130, 170, 240));

    public static Rgb Given => Pick(new(240, 240, 250), new(20, 22, 32));

    public static Rgb Entry => Pick(new(110, 210, 255), new(10, 90, 190));

    public static Rgb Conflict => Pick(new(255, 165, 0), new(196, 100, 0));

    public static Rgb Right => new(144, 238, 144);

    public static Rgb Wrong => new(205, 55, 55);

    public static Rgb OnWrong => new(250, 250, 255);

    public static Rgb Flash => Pick(new(255, 255, 255), new(255, 226, 120));

    public static Rgb Dark => new(15, 15, 20);

    public static Rgb ButtonBackground => Pick(new(55, 62, 88), new(208, 213, 228));

    public static Rgb ButtonDisabled => Pick(new(34, 38, 52), new(228, 231, 239));

    public static Rgb DialogBackground => Pick(new(28, 32, 46), new(255, 255, 255));

    public static Style Normal => new(Text, Background);

    public static Style Muted => new(Dim, Background);

    public static Style Title => new(Brand, Background);

    public static Style Company => new(Brand, Background);

    public static Style Error => new(Pick(new(255, 110, 110), new(196, 40, 40)), Background);

    public static Style Dialog => new(Text, DialogBackground);

    public static Style DialogMuted => new(Dim, DialogBackground);

    public static Style DialogTitle => new(Brand, DialogBackground);

    public static Style ButtonStyle => new(Text, ButtonBackground);

    public static Style ButtonDone => new(Dim, ButtonDisabled);

    public static Style ButtonOff => new(Dim, ButtonDisabled);

    public static Style ButtonOn => new(Dark, Accent);

    private static Rgb Pick(Rgb dark, Rgb light) => Light ? light : dark;
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

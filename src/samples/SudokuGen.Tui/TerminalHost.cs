using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace SudokuGen.Tui;

/// <summary>
/// The Terminal.Gui end of <see cref="Ui"/>: translates keys, mouse clicks and drawing calls and nothing else.
/// Excluded from coverage because it only runs inside a real terminal driver (see ADR 0005).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Terminal.Gui glue that needs a live terminal; all logic lives in Ui (ADR 0005).")]
internal sealed class TerminalHost : Runnable
{
    private readonly IApplication app;
    private readonly Ui ui;

    public TerminalHost(IApplication app, Ui ui)
    {
        this.app = app;
        this.ui = ui;
        CanFocus = true;
        Width = Dim.Fill();
        Height = Dim.Fill();
    }

    public void Refresh()
    {
        ui.Tick();
        SetNeedsDraw();
        StopIfQuit();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        ui.Paint(new ViewCanvas(this));
        return true;
    }

    protected override bool OnKeyDown(Key key)
    {
        if (ToInput(key) is { } input)
        {
            ui.HandleKey(input);
            Refresh();
            return true;
        }

        return base.OnKeyDown(key);
    }

    protected override bool OnPaste(string text)
    {
        ui.Paste(text);
        Refresh();
        return true;
    }

    protected override bool OnMouseEvent(Mouse mouse)
    {
        if (mouse.Flags.HasFlag(MouseFlags.LeftButtonClicked) && mouse.Position is { } position)
        {
            ui.HandleClick(position.X, position.Y, Viewport.Width, Viewport.Height);
            Refresh();
            return true;
        }

        return base.OnMouseEvent(mouse);
    }

    private void StopIfQuit()
    {
        if (ui.QuitRequested)
        {
            app.RequestStop();
        }
    }

    private static Input? ToInput(Key key)
    {
        var code = key.KeyCode & ~(KeyCode.CtrlMask | KeyCode.ShiftMask | KeyCode.AltMask);
        var name = code switch
        {
            KeyCode.CursorUp => "Up",
            KeyCode.CursorDown => "Down",
            KeyCode.CursorLeft => "Left",
            KeyCode.CursorRight => "Right",
            KeyCode.Enter => "Enter",
            KeyCode.Esc => "Esc",
            KeyCode.Tab => "Tab",
            KeyCode.Backspace => "Backspace",
            KeyCode.Delete => "Delete",
            >= (KeyCode)' ' and <= (KeyCode)'~' => char.ToLowerInvariant((char)code).ToString(),
            _ => null,
        };

        return name is null ? null : new Input(name, key.IsCtrl);
    }

    [ExcludeFromCodeCoverage(Justification = "Terminal.Gui glue that needs a live terminal.")]
    private sealed class ViewCanvas(View view) : ICanvas
    {
        public int Width => view.Viewport.Width;

        public int Height => view.Viewport.Height;

        public void Put(int x, int y, string text, Style style)
        {
            view.SetAttribute(new Terminal.Gui.Drawing.Attribute(ToColor(style.Fg), ToColor(style.Bg)));
            view.Move(x, y);
            view.AddStr(text);
        }

        private static Color ToColor(Rgb rgb) => new(rgb.R, rgb.G, rgb.B);
    }
}

/// <summary>Clipboard and browser access for <see cref="Ui"/>; best effort, a failure never interrupts the game.</summary>
[ExcludeFromCodeCoverage(Justification = "Talks to the operating system (ADR 0005).")]
internal sealed class TerminalSystem(IApplication app) : ISystem
{
    public string? ReadClipboard() => app.Clipboard is { } clipboard && clipboard.TryGetClipboardData(out var text) ? text : null;

    public void WriteClipboard(string text) => app.Clipboard?.TrySetClipboardData(text);

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
    }
}

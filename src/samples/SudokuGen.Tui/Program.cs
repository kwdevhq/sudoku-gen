using System.Diagnostics.CodeAnalysis;
using SudokuGen.Tui;
using Terminal.Gui.App;

[ExcludeFromCodeCoverage(Justification = "Process entry point that starts a real terminal session (ADR 0005).")]
internal static class TuiApp
{
    public static int Main()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SudokuGen");
        var session = new Session(directory, TimeProvider.System);
        Theme.Light = session.LightTheme;
        using var app = Application.Create();
        app.Init();
        var ui = new Ui(session, TimeProvider.System, new TerminalSystem(app));
        using var host = new TerminalHost(app, ui);
        app.AddTimeout(TimeSpan.FromMilliseconds(100), () =>
        {
            host.Refresh();
            return true;
        });
        app.Run(host);

        session.SaveOnExit();
        return 0;
    }
}

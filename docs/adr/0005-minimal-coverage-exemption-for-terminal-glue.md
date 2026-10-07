---
status: accepted
---
# Exempt only the Terminal.Gui glue from coverage

The 100% coverage gate stays for the TUI sample. Every rule, screen, key, click and paint decision lives in `Game`, `Session`, `Statistics` and `Ui`, which draw through the `ICanvas` interface and are tested with a recording canvas. Only the thin `TerminalHost` (forwards key, mouse and draw calls to Terminal.Gui) and the `TuiApp` entry point are marked `[ExcludeFromCodeCoverage]`, because they need a real terminal. New exemptions need a new decision here.
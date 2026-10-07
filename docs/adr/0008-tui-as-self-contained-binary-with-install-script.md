---
status: accepted
---
# Ship the TUI as a self-contained binary with an install script

The release workflow publishes the TUI as a self-contained `win-x64` build (`sudoku-tui.exe` and one native DLL), without trimming or AOT because Terminal.Gui depends on reflection. It attaches `sudoku-tui-win-x64.zip`, its SHA256 file and `install.ps1` to the GitHub Release. The script verifies the checksum, installs to `%LOCALAPPDATA%\kwdevhq\sudoku-tui`, and adds that folder to the user PATH. It needs no .NET SDK and no administrator rights. Uninstall never deletes the save and statistics files (ADR 0006).

Windows x64 is the only target: the maintainer cannot test Linux or macOS. Other platforms are added as new publish runtime identifiers when they can be tested. A `dotnet tool` package was rejected because it needs the .NET runtime on the user machine.

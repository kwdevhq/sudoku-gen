# SudokuGen

[![NuGet](https://img.shields.io/nuget/v/SudokuGen.svg)](https://www.nuget.org/packages/SudokuGen)
[![Downloads](https://img.shields.io/nuget/dt/SudokuGen.svg)](https://www.nuget.org/packages/SudokuGen)
[![PR pipeline](https://github.com/kwdevhq/sudoku-gen/actions/workflows/pr.yml/badge.svg)](https://github.com/kwdevhq/sudoku-gen/actions/workflows/pr.yml)
[![Release pipeline](https://github.com/kwdevhq/sudoku-gen/actions/workflows/release.yml/badge.svg)](https://github.com/kwdevhq/sudoku-gen/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512bd4.svg)](https://dotnet.microsoft.com)

A fast .NET library that generates sudoku puzzles, and a terminal game that uses it.

- Fast: it transforms known seed puzzles. It does not solve boards.
- Reproducible: each sudoku has an id, for example `DHS6-RJN0-C38`. The same id always gives the same sudoku.
- Four difficulties: easy, medium, hard and expert.

## Try it in the terminal

![Sudoku TUI](docs/tui.png)

The TUI is a sudoku game for the terminal. It has mouse and keyboard control, autosave and statistics.

Windows x64. Run this in PowerShell:

```powershell
iex (irm https://raw.githubusercontent.com/kwdevhq/sudoku-gen/main/scripts/install.ps1)
sudoku-tui
```

The script checks the SHA256 checksum and installs to `%LOCALAPPDATA%\kwdevhq\sudoku-tui`. It adds that folder to your user PATH. It needs no administrator rights and no .NET. Run it again to upgrade.

To select a version or to remove the game:

```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/kwdevhq/sudoku-gen/main/scripts/install.ps1))) -Version 1.0.0
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/kwdevhq/sudoku-gen/main/scripts/install.ps1))) -Uninstall
```

Uninstall keeps your save and statistics files.

Linux and macOS: coming soon. Until then, run `dotnet run --project src/samples/SudokuGen.Tui` with the .NET 10 SDK.

## Use the library

```
dotnet add package SudokuGen
```

```csharp
using SudokuGen;

Sudoku any = SudokuGenerator.Generate();
Sudoku hard = SudokuGenerator.Generate(Difficulty.Hard);

// Get the same sudoku again from its id
SudokuId id = SudokuId.Parse("DHS6-RJN0-C38");
Sudoku again = SudokuGenerator.FromId(id);

// A seeded Random only changes which new id is picked
Sudoku picked = SudokuGenerator.Generate(Difficulty.Easy, random: new Random(42));
```

A `Sudoku` has `Puzzle`, `Solution`, `Difficulty` and `Id`. `Puzzle` and `Solution` are 81-character strings, row by row. `Puzzle` uses `-` for an empty cell.

The CLI sample shows the data:

```
dotnet run --project src/samples/SudokuGen.Cli -- --difficulty hard --solution
```

```
Difficulty: Hard
Id: DHS6-RJN0-C38

+-------+-------+-------+
| 2 3 . | . . . | 8 . . |
| . . . | . 6 . | 1 7 5 |
| 6 . 5 | . . 8 | . 3 4 |
+-------+-------+-------+
| 8 . 6 | . . . | . . 7 |
| . . 9 | . . . | . . 2 |
| . 1 . | . 5 . | . . . |
+-------+-------+-------+
| . . . | . . 7 | . 2 3 |
| 7 . . | . . . | . . . |
| . 4 . | 3 . 6 | 7 9 . |
+-------+-------+-------+

Solution:

+-------+-------+-------+
| 2 3 1 | 7 4 5 | 8 6 9 |
| 9 8 4 | 2 6 3 | 1 7 5 |
| 6 7 5 | 9 1 8 | 2 3 4 |
+-------+-------+-------+
| 8 2 6 | 4 3 9 | 5 1 7 |
| 4 5 9 | 6 7 1 | 3 8 2 |
| 3 1 7 | 8 5 2 | 9 4 6 |
+-------+-------+-------+
| 1 6 8 | 5 9 7 | 4 2 3 |
| 7 9 3 | 1 2 4 | 6 5 8 |
| 5 4 2 | 3 8 6 | 7 9 1 |
+-------+-------+-------+
```

```
dotnet run --project src/samples/SudokuGen.Cli -- --id DHS6-RJN0-C38 --format json
```

```json
{
  "puzzle": "23----8------6-1756-5--8-348-6-----7--9-----2-1--5---------7-237---------4-3-679-",
  "solution": "231745869984263175675918234826439517459671382317852946168597423793124658542386791",
  "difficulty": "hard",
  "id": "DHS6-RJN0-C38"
}
```

## How it works

SudokuGen starts from a known seed puzzle that has one solution. It then applies random transformations:

- rotate the board (4 ways)
- shuffle the stacks (3!) and the bands (3!)
- shuffle the columns in each stack (6³) and the rows in each band (6³)
- replace the digits with a new order (9!)

Each seed gives more than 2.4 trillion different puzzles.

## Inspired by

SudokuGen is a .NET port of [petewritescode/sudoku-gen](https://github.com/petewritescode/sudoku-gen) by Pete Williams (MIT). The seed-and-transform method comes from that project. See [NOTICE.md](NOTICE.md).

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/core` | The library (NuGet package `SudokuGen`) |
| `src/tests/SudokuGen.Tests` | Tests |
| `src/samples/SudokuGen.Cli` | Command-line sample |
| `src/samples/SudokuGen.Tui` | The terminal game |
| `src/benchmarks/SudokuGen.Benchmarks` | Generation benchmarks |

Every sample uses the same library.

## Built with

- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui): the terminal user interface
- [TUnit](https://github.com/thomhurst/TUnit): the test framework and coverage (100% test coverage 🎉)
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet): the benchmarks

## Development

You need the .NET 10 SDK.

```
dotnet test --solution src/SudokuGen.slnx --coverage
pwsh scripts/coverage-report.ps1   # HTML report: coverage-report/index.html
pwsh scripts/check.ps1             # all CI gates: format, warnings as errors, 100% coverage
dotnet run -c Release --project src/benchmarks/SudokuGen.Benchmarks -- --filter '*'   # add --job short for a quick run
```

Enable the pre-commit hook once with `git config core.hooksPath .githooks`.

More: [GLOSSARY.md](GLOSSARY.md) (terms), [docs/adr](docs/adr) (decisions), [CODING_STANDARDS.md](CODING_STANDARDS.md), [AGENTS.md](AGENTS.md) (AI agents), [docs/releasing.md](docs/releasing.md).

## Feedback

Found a bug or have an idea? [Open an issue](https://github.com/kwdevhq/sudoku-gen/issues/new). Include the Sudoku id, if you have one, and the steps to repeat the problem.

You can also send a pull request. For a large change, open an issue first. Run `pwsh scripts/check.ps1` before you push.

## License

MIT. The core is derived from the original by Pete Williams. The port, its extensions and everything else are by [kw.dev gmbh](https://kw.dev). See [LICENSE](LICENSE).

# SudokuGen

A fast, allocation-light sudoku puzzle generator for .NET.

> .NET port of [petewritescode/sudoku-gen](https://github.com/petewritescode/sudoku-gen) by Pete Williams (MIT). See [NOTICE.md](NOTICE.md).

## Usage

```csharp
using SudokuGen;

Sudoku any = SudokuGenerator.Generate();
Sudoku hard = SudokuGenerator.Generate(Difficulty.Hard);

// Reproduce exactly the same sudoku later (no randomness involved)
SudokuId id = SudokuId.Parse(any.Id.ToString());   // e.g. DHS6-RJN0-C38
Sudoku again = SudokuGenerator.FromId(id);

// Seeded Random only affects which new id is picked
Sudoku picked = SudokuGenerator.Generate(Difficulty.Easy, random: new Random(42));
```n
`Sudoku` has `Puzzle`, `Solution`, `Difficulty` and `Id`. `Puzzle` and `Solution` are 81-character row-major strings; `Puzzle` uses `-` for cells the player must fill in.

```
Puzzle:   41--75-----53--7--2-36-81--7-9--25-1-3--9-47--2-1-7---6587--9-----26-8--1925---47
Solution: 416975238985321764273648159769432581531896472824157396658714923347269815192583647
```

![Numbered grid](docs/numbered-grid.png)
![Example puzzle and solution](docs/puzzle-solution.png)

## How it works

Instead of solving and removing clues (slow), SudokuGen starts from a known, uniquely solvable seed and applies random transformations:

- rotate the board (4), shuffle stacks (3!) and bands (3!), columns within stacks (6^3) and rows within bands (6^3), and relabel digits (9!).

Each seed yields over 2.4 trillion distinct puzzles, and generation is a couple of small allocations.

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/core` | The reusable library (NuGet package `SudokuGen`) |
| `src/tests/SudokuGen.Tests` | TUnit tests (with coverage), including unique-solution checks |
| `src/samples/SudokuGen.Cli` | Minimal CLI playground |
| `src/samples/SudokuGen.Tui` | Mouse and keyboard Sudoku game for the terminal (Terminal.Gui), with autosave and statistics |
| `src/benchmarks/SudokuGen.Benchmarks` | BenchmarkDotNet benchmarks for generation |

This repository is a playground: every sample (TUI, API, solver, ...) consumes the same library.

```
dotnet run --project src/samples/SudokuGen.Cli -- --difficulty hard --solution
dotnet run --project src/samples/SudokuGen.Cli -- --id DHS6-RJN0-C38 --format json
dotnet run --project src/samples/SudokuGen.Tui
dotnet test --solution src/SudokuGen.slnx --coverage
pwsh scripts/coverage-report.ps1   # visual per-line HTML report (coverage-report/index.html)
pwsh scripts/check-coverage.ps1   # clean run that fails below 100% line/branch coverage (used by CI)
dotnet run -c Release --project src/benchmarks/SudokuGen.Benchmarks -- --filter '*'   # add --job short for a quick run
```

Contributors and AI agents: see [AGENTS.md](AGENTS.md) and [GLOSSARY.md](GLOSSARY.md).

## License

MIT. The core is derived from the original by Pete Williams; the port, its extensions and everything else is by [kw.dev GmbH](https://kw.dev). See [LICENSE](LICENSE).

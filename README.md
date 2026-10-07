# SudokuGen

A fast, allocation-light sudoku puzzle generator for .NET.

> .NET port of [petewritescode/sudoku-gen](https://github.com/petewritescode/sudoku-gen) by Pete Williams (MIT). See [NOTICE.md](NOTICE.md).

## Usage

```csharp
using SudokuGen;

Sudoku any = SudokuGenerator.Generate();
Sudoku hard = SudokuGenerator.Generate(Difficulty.Hard);

// Reproduce exactly the same sudoku later (no randomness involved)
SudokuId id = SudokuId.Parse(any.Id.ToString());   // e.g. 12-3-457-1022-203456
Sudoku again = SudokuGenerator.Generate(id: id);

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
| `src/SudokuGen` | The reusable library (NuGet package `SudokuGen`) |
| `tests/SudokuGen.Tests` | TUnit tests (with coverage), including unique-solution checks |
| `samples/SudokuGen.Cli` | Minimal CLI playground |
| `benchmarks/SudokuGen.Benchmarks` | BenchmarkDotNet benchmarks for generation |

This repository is a playground: every sample (TUI, API, solver, ...) consumes the same library.

```
dotnet run --project samples/SudokuGen.Cli -- --difficulty hard --solution
dotnet run --project samples/SudokuGen.Cli -- --id 12-3-457-1022-203456 --format json
dotnet test --solution SudokuGen.slnx --coverage
pwsh scripts/coverage-report.ps1   # visual per-line HTML report (coverage-report/index.html)
pwsh scripts/check-coverage.ps1   # clean run that fails below 100% line/branch coverage (used by CI)
dotnet run -c Release --project benchmarks/SudokuGen.Benchmarks -- --filter '*'   # add --job short for a quick run
```

Contributors and AI agents: see [AGENTS.md](AGENTS.md) and [GLOSSARY.md](GLOSSARY.md).

## License

MIT. Copyright (c) Pete Williams and kwdevhq. See [LICENSE](LICENSE).

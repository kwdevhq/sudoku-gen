# Coding standards

Judgement calls for review. Mechanical rules (format, warnings, coverage) are enforced by CI, not listed here.

- **Single source**: generation logic lives only in `src/core`; samples and benchmarks consume it.
- **Stable ids**: `SudokuId` text form and seed ids are persisted ([ADR 0003](docs/adr/0003-reproduce-with-sudoku-id.md)). Changing the encoding, or a seed's `Id`, or removing seeds, is a breaking change; append new seeds at the end of a difficulty's array with the next id of its block (`SeedTests` verify this).
- **Draw order**: the order of `Random` draws in `SudokuGenerator.RandomId` decides what a seeded run produces. Changing it, or the id encoding, must update ADR 0003 and the golden ids in `src/tests/SudokuGen.Tests/KnownIds.cs`.
- **Human-facing text**: usage, errors and docs say what is wrong in plain language and match current behaviour (`--help` lists every mode; an invalid id names its actual problem).
- **Tight**: library hot paths stay allocation-light (spans, `stackalloc`); back any performance claim with `src/benchmarks/`.
- **Zero dependencies** in `src/core`.
- **Public API**: every public member has XML docs; new terms appear in GLOSSARY.md first.
- **Tests**: verify behaviour through the public API; unique-solution checks use the independent solver in `src/tests/SudokuGen.Tests/SudokuChecks.cs`, never library code.
- `src/core/Seeds.Data.cs` is upstream seed data copied once from `seeds.constant.ts` and maintained by hand; `SeedPinTests` pins every seed's content, so an appended seed needs its id and hash added there.
- Target frameworks only in `src/Directory.Build.props`; package versions only in `src/Directory.Packages.props`.

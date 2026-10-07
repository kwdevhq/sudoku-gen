# Coding standards

Judgement calls for review. Mechanical rules (format, warnings, coverage) are enforced by CI, not listed here.

- **Single source**: generation logic lives only in `src/SudokuGen`; samples and benchmarks consume it.
- **Stable ids**: `SudokuId` text form and seed order are persisted ([ADR 0003](docs/adr/0003-reproduce-with-sudoku-id.md)). Reordering or removing seeds, or changing the encoding, is a breaking change; append new seeds at the end of a difficulty's array.
- **Tight**: library hot paths stay allocation-light (spans, `stackalloc`); back any performance claim with `benchmarks/`.
- **Zero dependencies** in `src/SudokuGen`.
- **Public API**: every public member has XML docs; new terms appear in GLOSSARY.md first.
- **Tests**: verify behaviour through the public API; unique-solution checks use the independent solver in `tests/SudokuGen.Tests/SudokuChecks.cs`, never library code.
- `src/SudokuGen/Seeds.Data.cs` is upstream seed data copied once from `seeds.constant.ts` and maintained by hand; only append seeds.
- Target frameworks only in `Directory.Build.props`; package versions only in `Directory.Packages.props`.

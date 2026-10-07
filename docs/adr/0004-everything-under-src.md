---
status: accepted
---
# Keep all source under `src/`

Library, tests, samples, benchmarks and the solution live under `src/` (`core`, `tests`, `samples`, `benchmarks`, `SudokuGen.slnx`, `Directory.*.props`), so the repository root holds only documents, pipelines, scripts and tool configuration. `.github/`, `global.json`, `dotnet-tools.json` and `.editorconfig` stay at the root because GitHub and the .NET tooling expect them there. `DocumentedPathTests` fails when a markdown file names a path that no longer exists, so a layout move cannot leave stale references.

# AGENTS.md

.NET port of petewritescode/sudoku-gen: all source, tests, samples, benchmarks and the solution live under `src/` ([ADR 0004](docs/adr/0004-everything-under-src.md)).

- Vocabulary: [GLOSSARY.md](GLOSSARY.md). Decisions and their reasons: `docs/adr/`. Update both through the `domain-modeling` skill when terms or decisions change.
- Reviewing or finishing a change: [CODING_STANDARDS.md](CODING_STANDARDS.md).
- Build, test and benchmark commands: [README.md](README.md). `dotnet test` needs `--solution src/SudokuGen.slnx`.
- CI (`.github/workflows/pr.yml`) enforces format, warnings-as-errors and 100% coverage via `scripts/check-coverage.ps1` (clean test run, then verifies every coverage report). `pwsh scripts/check.ps1` runs all of it; run it before committing, or enable the hook once with `git config core.hooksPath .githooks`.
- The aihero.dev skills are installed globally, not vendored. Reach for `grilling` to settle design, `tdd` to implement, `domain-modeling` for glossary and ADRs, `diagnosing-bugs` for failures.
- Copyright holder is "kw.dev gmbh", never a personal name; keep upstream attribution in LICENSE and NOTICE.md.

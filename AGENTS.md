# AGENTS.md

.NET port of petewritescode/sudoku-gen: library `src/SudokuGen`, tests `tests/`, playgrounds `samples/`, benchmarks `benchmarks/`.

- Vocabulary: [GLOSSARY.md](GLOSSARY.md). Decisions and their reasons: `docs/adr/`. Update both through the `domain-modeling` skill when terms or decisions change.
- Reviewing or finishing a change: [CODING_STANDARDS.md](CODING_STANDARDS.md).
- Build, test and benchmark commands: [README.md](README.md). `dotnet test` needs `--solution SudokuGen.slnx`.
- CI (`.github/workflows/pr.yml`) enforces format, warnings-as-errors and 100% coverage via `scripts/check-coverage.ps1`; run the same steps before committing.
- The aihero.dev skills are installed globally, not vendored. Reach for `grilling` to settle design, `tdd` to implement, `domain-modeling` for glossary and ADRs, `diagnosing-bugs` for failures.
- Copyright holder is "kwdevhq", never a personal name; keep upstream attribution in LICENSE and NOTICE.md.

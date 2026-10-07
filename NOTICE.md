# Notice

SudokuGen is a .NET port of [petewritescode/sudoku-gen](https://github.com/petewritescode/sudoku-gen)
(npm package `sudoku-gen`), Copyright (c) Pete Williams, released under the MIT License.

What was carried over from the original project:

- The generation approach (transform a known seed puzzle instead of solving and removing clues).
- The 40 seed puzzles (10 per difficulty) and their puzzle/solution encoding.
- The README illustrations in `docs/`.

The original TypeScript sources remain available in this repository's git history
(commits before the .NET port). The original MIT license text and copyright notice are
retained in [LICENSE](LICENSE) together with the copyright of kw.dev GmbH.

The core algorithm and seed data originate from Pete Williams. The .NET port, its extensions (solver, samples,
tests, benchmarks) and everything else in this repository are Copyright (c) 2026 kw.dev GmbH ([kw.dev](https://kw.dev)).

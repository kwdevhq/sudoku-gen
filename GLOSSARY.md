# Glossary

**Sudoku**: A generated puzzle together with its solution and difficulty.

**Puzzle**: The partially filled grid handed to the player. Empty cells are shown as `-`.

**Solution**: The completely filled grid that satisfies the puzzle; unique for every puzzle.

**Difficulty**: One of easy, medium, hard, expert. Inherited from the seed a sudoku was made from.

**Seed**: A known, uniquely solvable puzzle/solution pair that new sudokus are derived from.

**Transformation**: A change to a seed that preserves solvability: rotation, shuffling of bands, stacks, rows or columns, or relabelling of digits.

**Sudoku id**: The identifier of one specific sudoku: which seed it came from and which transformations were applied, written as 11 characters like `DHS6-RJN0-C38` (the last is a check character). The same id always recreates the same sudoku.

**Seed id**: The stable number of a seed, `difficulty * 64 + position`; part of the sudoku id and never reused or changed.

**Band**: One of the three horizontal groups of three rows.

**Stack**: One of the three vertical groups of three columns.

**Playground**: A sample application in this repository that consumes the library.

**Given**: A cell that is filled in the puzzle. The player cannot change it.

**Entry**: A digit the player has put into an empty cell. Entries are only verified against the solution by a check, or implicitly when the board becomes solved.

**Check**: A player-requested comparison of all entries against the solution, marking each as right or wrong. Never happens automatically.

**Conflict**: The same digit appearing twice in one row, column or box. Shown to the player as a visual cue; it does not mean the entry is wrong.

**Solved**: The state where every cell matches the solution. Reached on the last correct entry; ends the game.

**Save**: The single stored in-progress game of the TUI playground: Sudoku id, entries, history and elapsed time. Resumed automatically on start; removed when the game is solved.

**Statistics**: Per-difficulty tallies kept by the TUI playground across games: games started, games won, fastest and average solve time, current and best win streak.

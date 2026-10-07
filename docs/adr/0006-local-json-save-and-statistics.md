---
status: accepted
---
# Store the TUI save and statistics as local JSON

The TUI keeps two files in `%LOCALAPPDATA%\SudokuGen`: `tui-save.json` (Sudoku id, entries, history, elapsed time) and `tui-stats.json` (per-difficulty tallies). The save is loaded automatically and written atomically (temp file, then replace) after each change; it is deleted when the puzzle is solved or a new game is confirmed. Unreadable or inconsistent files are ignored, and storage failures never interrupt play. Sudoku ids are not recorded in the statistics.
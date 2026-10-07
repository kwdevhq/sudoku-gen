# TUI playground spec

A rich terminal Sudoku game as a **Playground** that consumes the library. Vocabulary: [GLOSSARY.md](../../GLOSSARY.md).

## Project

- `src/samples/SudokuGen.Tui`, added to `src/SudokuGen.slnx`.
- Terminal.Gui v2 (package added to `src/Directory.Packages.props`).
- Windows Terminal is the target. Keyboard and mouse both work fully.
- Below the minimum size (80x30) a "please enlarge" message replaces the board.

## Play

- Select a cell by mouse click or arrow keys.
- Digits 1-9 enter, Backspace/Delete/0 clear. A clickable number pad does the same; a digit whose nine cells are all placed without a conflict turns gray on the pad.
- Digits are drawn as large (fullwidth) glyphs centered in 6x3 cells.
- The Sudoku id can be copied (Ctrl+C, the Copy button, or C / Copy id on the win dialog). Pasting into the id field of the start screen works with the terminal paste and Ctrl+V.
- A footer credits kw.dev and links to the site and to the license (clicking opens the browser). The brand color #d06900 is used for headers and the company name only, never for conflicts or errors.
- Undo/redo with Ctrl+Z / Ctrl+Y. History holds only cell changes.
- **Givens** are locked.
- No notes. No automatic hints except the visual cues below.
- Timer counts play time only and starts at the first input of a new or resumed game.

### Visual cues

- Highlight the selected cell's row, column and box, and all cells with the same digit.
- Distinct colors for givens and entries; thick box borders.
- **Conflicts** (duplicate digit in a row, column or box) are shown in amber on both cells, givens included. This is a cue only; entries are never rejected.
- **Check** is a button. Wrong entries turn red, right entries light green. These win over amber. Empty cells are not marked. Status line: "N wrong, M empty".
- Check marks clear when the cell is edited, on undo/redo and on restart. They are not saved.

### Solved

- The game is solved automatically on the last correct entry. No Check needed.
- Order on win: input locks, the save is deleted, statistics are recorded, then the ~1 s color wave plays, then a dialog "Solved in mm:ss, id ..." offers New game or Quit.
- The dialog shows a compact statistics table with the current difficulty highlighted and a note when the time is a new fastest.

### Confirmations

- Reset confirms when the board has entries; with no entries it does nothing and says so in the status line.
- New game confirms only when the game is unfinished and has at least one entry. Quit never confirms: the game autosaves.

## Start

- With a **Save**: go straight into that board, no menu.
- Without: start screen with difficulty choice, optional Sudoku id, Start, and a Statistics entry.
- New game returns to the start screen. The old save is replaced only once a new game starts; cancelling keeps it.
- The Sudoku id is always displayed in the game.
- The Statistics entry shows the table with no difficulty highlighted.

## Persistence

Directory `%LOCALAPPDATA%\SudokuGen\`, injected into the code so tests use a temp directory.

- `tui-save.json`: format version, Sudoku id, entries, undo/redo history, elapsed time. One slot.
  - Written atomically (temp file, then move) after every change and on exit.
  - Deleted on solved, confirmed New game, and confirmed Reset (Reset rewrites an empty board).
  - Corrupt, unknown-version or invalid-id files are ignored (start screen); overwritten by the next save.
- `tui-stats.json`: format version and per-difficulty statistics.
  - Corrupt or unknown-version files are treated as empty and overwritten by the next update.
  - A failure to read or write stats never blocks the win flow.

## Statistics

Per difficulty (easy, medium, hard, expert):

- Games started, games won, win rate (derived), fastest time, average time (over won games), current win streak, best win streak.
- A Total line sums games, wins and win rate over all difficulties (times and streaks do not add up).

Rules:

- A game is started when it begins from the start screen. Resume does not count.
- Reset counts as a new game (started +1) and breaks the streak, so a Reset cannot be used to shorten the time.
- An unfinished game replaced by New game stays counted as started, not won, and breaks the streak.
- Typed-in Sudoku ids count like random ones. Ids are not stored.
- Time is the persisted play time, so it includes time before a resume.
- No reset-statistics action, no total time, no fastest ids.

## Code and tests

- All rules (cursor, entries, history, conflicts, check, solved, timer, save, statistics) live in a UI-free logic layer with an injected clock and directory. It is covered 100% by tests.
- The Terminal.Gui view and input glue is a thin layer. Only the bare minimum that cannot reasonably be tested is excluded from coverage, strictly and individually documented (ADR).
- Implement with the `tdd` skill; run `pwsh scripts/check.ps1` before committing.

## Decisions to record as ADRs

1. Minimal, strict coverage exemption for the UI shell.
2. Local JSON save and statistics; save deleted on win.

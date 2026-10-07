---
status: accepted
---
# Generate sudokus by transforming seeds

Like the upstream petewritescode/sudoku-gen, we derive new sudokus by transforming a fixed set of known, uniquely solvable seeds instead of solving and removing clues. This keeps generation to microseconds with no solver, which fits the speed goal, at the cost of difficulty being limited to what the seeds provide. Tests verify uniqueness of every seed's solution independently.

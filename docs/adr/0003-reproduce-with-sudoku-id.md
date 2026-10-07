---
status: accepted
---
# Reproduce sudokus with a SudokuId, not a Random seed

Two random components (choosing a seed, then choosing transformations) made `Random`-seeded output depend on implementation details of the generator. Instead a `SudokuId` records the seed index and every transformation (rotation, row/column arrangement, digit relabelling), and `Generate` with an id uses no randomness. Consequence: the seed order and id encoding are a persisted format and must stay append-only/stable; a `Random` is only a source for picking a new id.

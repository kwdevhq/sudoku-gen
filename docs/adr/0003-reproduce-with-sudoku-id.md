---
status: accepted
---
# Reproduce sudokus with a SudokuId, not a Random seed

Two random components (choosing a seed, then choosing transformations) made `Random`-seeded output depend on implementation details of the generator. Instead a `SudokuId` records the seed and every transformation (rotation, row/column arrangement, digit relabelling), and `SudokuGenerator.FromId` uses no randomness; `Generate` only picks a new id from a `Random`. Consequence: the seed ids and id encoding are a persisted format and must stay stable.

## Encoding

- Each seed has an explicit, never-reused `Id` in the seed data: `difficulty * 64 + position` (Easy 0-63, Medium 64-127, Hard 128-191, Expert 192-255). Appending seeds to one difficulty therefore cannot shift another's ids; tests verify the ids against the data.
- The id packs into one integer in mixed radix (seed id 256, rotation 4, rows 1296, columns 1296, digits 9!, seed least significant), written as 10 Crockford Base32 characters plus one check character (sum of position x symbol, modulo 31), grouped as in `DHS6-RJN0-C38`. About 50 bits, so 11 characters.
- Parsing ignores case and hyphens, reads `O` as `0` and `I`/`L` as `1`. The check catches typos and swapped neighbours (the only blind spot is `0` <-> `Z`).
- The earlier `seed-rotation-rows-columns-digits` decimal form was never released and is not parsed.

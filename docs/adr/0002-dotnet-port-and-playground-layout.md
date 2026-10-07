---
status: accepted
---
# Port faithfully to .NET as a reusable library

The repository is a .NET 10 port (later multi-targeting .NET 11) of the MIT-licensed petewritescode/sudoku-gen, with attribution kept in LICENSE and NOTICE.md. The library `src/core` is the single source of generation logic, intended for NuGet; all samples under `src/samples/` are playgrounds that consume it and must not duplicate generation code. The public API keeps upstream's 81-character puzzle/solution strings.

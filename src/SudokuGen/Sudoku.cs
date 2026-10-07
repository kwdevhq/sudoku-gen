namespace SudokuGen;

/// <summary>A generated sudoku puzzle together with its solution.</summary>
/// <param name="Puzzle">
/// 81 characters in row-major order; <c>1</c>-<c>9</c> are given digits and <c>-</c> marks a cell the player must fill in.
/// </param>
/// <param name="Solution">81 characters in row-major order with every cell filled (<c>1</c>-<c>9</c>).</param>
/// <param name="Difficulty">The difficulty of the puzzle.</param>
/// <param name="Id">Identifier that recreates exactly this sudoku via <see cref="SudokuGenerator.FromId"/>.</param>
public sealed record Sudoku(string Puzzle, string Solution, Difficulty Difficulty, SudokuId Id);

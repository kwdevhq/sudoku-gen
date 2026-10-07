using BenchmarkDotNet.Attributes;

namespace SudokuGen.Benchmarks;

[MemoryDiagnoser]
public class GenerateBenchmarks
{
    private static readonly SudokuId FixedId = new(12, 3, 457, 1022, 203456);

    private readonly Random _random = new(42);

    [Benchmark(Baseline = true)]
    public Sudoku Random_AnyDifficulty() => SudokuGenerator.Generate(random: _random);

    [Benchmark]
    public Sudoku Shared_AnyDifficulty() => SudokuGenerator.Generate();

    [Benchmark]
    [Arguments(Difficulty.Easy)]
    [Arguments(Difficulty.Medium)]
    [Arguments(Difficulty.Hard)]
    [Arguments(Difficulty.Expert)]
    public Sudoku ByDifficulty(Difficulty difficulty) => SudokuGenerator.Generate(difficulty, random: _random);

    [Benchmark]
    public Sudoku FromId() => SudokuGenerator.Generate(id: FixedId);

    [Benchmark]
    public SudokuId ParseId() => SudokuId.Parse("12-3-457-1022-203456");
}

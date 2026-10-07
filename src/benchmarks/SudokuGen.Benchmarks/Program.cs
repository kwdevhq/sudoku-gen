using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(SudokuGen.Benchmarks.GenerateBenchmarks).Assembly).Run(args);

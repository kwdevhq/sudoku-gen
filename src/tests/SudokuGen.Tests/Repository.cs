namespace SudokuGen.Tests;

internal static class Repository
{
    // global.json stays at the repository root regardless of how the source tree is laid out.
    public static string Root { get; } = FindRoot();

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("global.json not found above the test output.");
    }
}

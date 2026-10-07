using System.Text.Json.Serialization;

namespace SudokuGen.Tui;

internal readonly record struct DifficultyStats(int Started, int Won, long? FastestMs, long TotalWonMs, int Streak, int BestStreak)
{
    [JsonIgnore]
    public double? WinRate => Started == 0 ? null : (double)Won / Started;

    [JsonIgnore]
    public TimeSpan? Fastest => FastestMs is { } ms ? TimeSpan.FromMilliseconds(ms) : null;

    [JsonIgnore]
    public TimeSpan? Average => Won == 0 ? null : TimeSpan.FromMilliseconds(TotalWonMs / Won);
}

/// <summary>Per-difficulty tallies. Immutable: every change returns a new instance.</summary>
internal sealed record Statistics([property: JsonRequired] int Version, [property: JsonRequired] IReadOnlyList<DifficultyStats> ByDifficulty)
{
    public const int CurrentVersion = 1;

    private static readonly int DifficultyCount = Enum.GetValues<Difficulty>().Length;

    public static Statistics Empty { get; } = new(CurrentVersion, [.. new DifficultyStats[DifficultyCount]]);

    [JsonIgnore]
    public bool IsValid => Version == CurrentVersion && ByDifficulty.Count == DifficultyCount;

    /// <summary>Games and wins over all difficulties; times and streaks do not add up across difficulties and stay empty.</summary>
    [JsonIgnore]
    public DifficultyStats Total => new(ByDifficulty.Sum(s => s.Started), ByDifficulty.Sum(s => s.Won), null, 0, 0, 0);

    public DifficultyStats this[Difficulty difficulty] => ByDifficulty[(int)difficulty];

    public Statistics Started(Difficulty difficulty) => Update(difficulty, s => s with { Started = s.Started + 1 });

    public Statistics Abandoned(Difficulty difficulty) => Update(difficulty, s => s with { Streak = 0 });

    public Statistics Won(Difficulty difficulty, TimeSpan time, out bool newFastest)
    {
        var previous = this[difficulty].FastestMs;
        var ms = (long)time.TotalMilliseconds;
        newFastest = previous is { } best && ms < best;
        return Update(difficulty, s => s with
        {
            Won = s.Won + 1,
            FastestMs = previous is { } fastest ? Math.Min(fastest, ms) : ms,
            TotalWonMs = s.TotalWonMs + ms,
            Streak = s.Streak + 1,
            BestStreak = Math.Max(s.BestStreak, s.Streak + 1),
        });
    }

    private Statistics Update(Difficulty difficulty, Func<DifficultyStats, DifficultyStats> change)
    {
        var list = ByDifficulty.ToList();
        list[(int)difficulty] = change(list[(int)difficulty]);
        return this with { ByDifficulty = list };
    }
}

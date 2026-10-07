using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class StatisticsTests
{
    [Test]
    public async Task Empty_HasNothingToShow()
    {
        var stats = Statistics.Empty[Difficulty.Easy];

        await Assert.That(stats.WinRate).IsNull();
        await Assert.That(stats.Fastest).IsNull();
        await Assert.That(stats.Average).IsNull();
        await Assert.That(Statistics.Empty.IsValid).IsTrue();
    }

    [Test]
    public async Task Started_And_Won_AreTalliedPerDifficulty()
    {
        var stats = Statistics.Empty
            .Started(Difficulty.Hard)
            .Started(Difficulty.Hard)
            .Won(Difficulty.Hard, TimeSpan.FromSeconds(100), out _)
            .Started(Difficulty.Easy);

        var hard = stats[Difficulty.Hard];
        await Assert.That(hard.Started).IsEqualTo(2);
        await Assert.That(hard.Won).IsEqualTo(1);
        await Assert.That(hard.WinRate).IsEqualTo(0.5);
        await Assert.That(hard.Fastest).IsEqualTo(TimeSpan.FromSeconds(100));
        await Assert.That(stats[Difficulty.Easy].Started).IsEqualTo(1);
        await Assert.That(stats[Difficulty.Medium].Started).IsEqualTo(0);
    }

    [Test]
    public async Task Won_TracksFastestAverageAndPersonalBests()
    {
        var stats = Statistics.Empty.Won(Difficulty.Easy, TimeSpan.FromSeconds(100), out var first);
        stats = stats.Won(Difficulty.Easy, TimeSpan.FromSeconds(300), out var slower);
        stats = stats.Won(Difficulty.Easy, TimeSpan.FromSeconds(50), out var faster);

        var easy = stats[Difficulty.Easy];
        await Assert.That(first).IsFalse();
        await Assert.That(slower).IsFalse();
        await Assert.That(faster).IsTrue();
        await Assert.That(easy.Fastest).IsEqualTo(TimeSpan.FromSeconds(50));
        await Assert.That(easy.Average).IsEqualTo(TimeSpan.FromSeconds(150));
    }

    [Test]
    public async Task Streak_GrowsOnWinsAndBreaksWhenAbandoned_KeepingTheBest()
    {
        var stats = Statistics.Empty
            .Won(Difficulty.Medium, TimeSpan.FromSeconds(1), out _)
            .Won(Difficulty.Medium, TimeSpan.FromSeconds(1), out _)
            .Abandoned(Difficulty.Medium)
            .Won(Difficulty.Medium, TimeSpan.FromSeconds(1), out _);

        await Assert.That(stats[Difficulty.Medium].Streak).IsEqualTo(1);
        await Assert.That(stats[Difficulty.Medium].BestStreak).IsEqualTo(2);
    }

    [Test]
    public async Task IsValid_RejectsOtherVersionsAndShapes()
    {
        await Assert.That((Statistics.Empty with { Version = 2 }).IsValid).IsFalse();
        await Assert.That((Statistics.Empty with { ByDifficulty = [] }).IsValid).IsFalse();
    }
}

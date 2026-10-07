namespace SudokuGen.Tests;

public class SudokuIdTests
{
    [Test]
    public async Task ToString_And_Parse_RoundTrip()
    {
        var id = new SudokuId(12, 3, 457, 1022, 203456);

        await Assert.That(id.ToString()).IsEqualTo("12-3-457-1022-203456");
        await Assert.That(SudokuId.Parse("12-3-457-1022-203456")).IsEqualTo(id);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("1-2-3-4")]
    [Arguments("1-2-3-4-5-6")]
    [Arguments("a-0-0-0-0")]
    [Arguments("-1-0-0-0-0")]
    [Arguments("40-0-0-0-0")]
    [Arguments("0-4-0-0-0")]
    [Arguments("0-0-1296-0-0")]
    [Arguments("0-0-0-1296-0")]
    [Arguments("0-0-0-0-362880")]
    public async Task TryParse_RejectsInvalidText(string? text)
    {
        await Assert.That(SudokuId.TryParse(text, out _)).IsFalse();
        await Assert.That(() => SudokuId.Parse(text!)).Throws<FormatException>();
    }

    [Test]
    [Arguments(-1, 0, 0, 0, 0)]
    [Arguments(40, 0, 0, 0, 0)]
    [Arguments(0, 4, 0, 0, 0)]
    [Arguments(0, 0, 1296, 0, 0)]
    [Arguments(0, 0, 0, 1296, 0)]
    [Arguments(0, 0, 0, 0, 362_880)]
    public async Task Constructor_RejectsOutOfRangeComponents(int seed, int rotation, int rows, int columns, int digits) =>
        await Assert.That(() => new SudokuId(seed, rotation, rows, columns, digits))
            .Throws<ArgumentOutOfRangeException>();

    [Test]
    public async Task Difficulty_FollowsSeedOrder()
    {
        await Assert.That(new SudokuId(0, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Easy);
        await Assert.That(new SudokuId(10, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Medium);
        await Assert.That(new SudokuId(20, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Hard);
        await Assert.That(new SudokuId(39, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Expert);
    }
}

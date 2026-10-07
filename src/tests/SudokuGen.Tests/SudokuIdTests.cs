namespace SudokuGen.Tests;

public class SudokuIdTests
{
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    [Test]
    [Arguments(0, 0, 0, 0, 0, "0000-0000-000")]
    [Arguments(1, 0, 0, 0, 0, "0000-0000-01A")]
    [Arguments(0, 1, 0, 0, 0, "0000-0000-80A")]
    public async Task ToString_MatchesThePersistedFormat(int seed, int rotation, int rows, int columns, int digits, string expected)
    {
        var id = new SudokuId(seed, rotation, rows, columns, digits);

        await Assert.That(id.ToString()).IsEqualTo(expected);
        await Assert.That(SudokuId.Parse(expected)).IsEqualTo(id);
    }

    [Test]
    public async Task ToString_And_Parse_RoundTripExtremes()
    {
        var ids = new[]
        {
            new SudokuId(0, 0, 0, 0, 0),
            new SudokuId(Seeds.Expert[^1].Id, 3, SudokuId.LineArrangementCount - 1, SudokuId.LineArrangementCount - 1, SudokuId.DigitArrangementCount - 1),
            new SudokuId(Seeds.Medium[2].Id, 3, 457, 1022, 203_456),
        };

        foreach (var id in ids)
        {
            var text = id.ToString();

            await Assert.That(text).Matches("^[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{3}$");
            await Assert.That(SudokuId.Parse(text)).IsEqualTo(id);
        }
    }

    [Test]
    public async Task Parse_IgnoresCaseHyphensAndReadsLookalikes()
    {
        var zero = new SudokuId(0, 0, 0, 0, 0);
        var one = new SudokuId(1, 0, 0, 0, 0);

        await Assert.That(SudokuId.Parse("OOOO-oooo-OOO")).IsEqualTo(zero);
        await Assert.That(SudokuId.Parse("00000000000")).IsEqualTo(zero);
        await Assert.That(SudokuId.Parse("0000-0000-0Ia")).IsEqualTo(one);
        await Assert.That(SudokuId.Parse("0000-0000-0la")).IsEqualTo(one);
    }

    // The check weighs symbols by position modulo 31, so the one undetected typo is 0 <-> Z (values 0 and 31).
    private static bool IsUndetectablePair(char a, char b) => (a, b) is ('0', 'Z') or ('Z', '0');

    [Test]
    public async Task Parse_DetectsEverySingleCharacterTypo()
    {
        var text = new SudokuId(Seeds.Hard[3].Id, 2, 1000, 77, 123_456).ToString();

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '-')
            {
                continue;
            }

            foreach (var replacement in Alphabet.Where(c => c != text[i] && !IsUndetectablePair(c, text[i])))
            {
                var typo = text[..i] + replacement + text[(i + 1)..];

                await Assert.That(SudokuId.TryParse(typo, out _)).IsFalse();
            }
        }
    }

    [Test]
    public async Task Parse_DetectsSwappedNeighbours()
    {
        var text = new SudokuId(Seeds.Hard[3].Id, 2, 1000, 77, 123_456).ToString().Replace("-", string.Empty);

        for (var i = 0; i < text.Length - 1; i++)
        {
            if (text[i] == text[i + 1] || IsUndetectablePair(text[i], text[i + 1]))
            {
                continue;
            }

            var swapped = text[..i] + text[i + 1] + text[i] + text[(i + 2)..];

            await Assert.That(SudokuId.TryParse(swapped, out _)).IsFalse();
        }
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("0000-0000-00")]
    [Arguments("0000-0000-0000")]
    [Arguments("0000-0000-00U")]
    [Arguments("0000-0000-00*")]
    [Arguments("0000 0000 000")]
    [Arguments("0000-0000-001")]
    [Arguments("12-3-457-1022-203456")]
    public async Task TryParse_RejectsInvalidText(string? text)
    {
        await Assert.That(SudokuId.TryParse(text, out _)).IsFalse();
        await Assert.That(() => SudokuId.Parse(text!)).Throws<FormatException>();
    }

    [Test]
    [Arguments(null, "no id was given.")]
    [Arguments("DHS6-RJN0", "has 8.")]
    [Arguments("DHS6-RJN0-C38-X", "has 12.")]
    [Arguments("DHS6-RJN0-C3U", "'U' is not a valid character")]
    [Arguments("DHS6-RJN0-C37", "the last character doesn't match the others, so the id contains a typo.")]
    public async Task Parse_ExplainsWhatIsWrongInPlainLanguage(string? text, string expectedPart)
    {
        await Assert.That(() => SudokuId.Parse(text!)).Throws<FormatException>().WithMessageContaining(expectedPart);
        if (text is not null)
        {
            await Assert.That(() => SudokuId.Parse(text)).Throws<FormatException>().WithMessageContaining($"'{text}'");
        }
    }

    [Test]
    public async Task Parse_ExplainsAWellFormedIdThatBelongsToNoSudoku() =>
        await Assert.That(() => SudokuId.Parse(Encode(10))).Throws<FormatException>()
            .WithMessageContaining("no sudoku belongs to this id.");

    [Test]
    public async Task TryParse_RejectsWellFormedTextThatIsNotAnId()
    {
        // 256 seed ids x 4 rotations x 1296 x 1296 lines precede the digit arrangement.
        const ulong digitStep = 256UL * 4 * 1296 * 1296;
        var unknownSeed = Encode(10);
        var digitsOutOfRange = Encode(digitStep * SudokuId.DigitArrangementCount);

        await Assert.That(SudokuId.TryParse(unknownSeed, out _)).IsFalse();
        await Assert.That(SudokuId.TryParse(digitsOutOfRange, out _)).IsFalse();
        await Assert.That(SudokuId.TryParse(Encode(digitStep * (SudokuId.DigitArrangementCount - 1)), out var last)).IsTrue();
        await Assert.That(last.Digits).IsEqualTo(SudokuId.DigitArrangementCount - 1);
    }

    [Test]
    [Arguments(-1, 0, 0, 0, 0)]
    [Arguments(10, 0, 0, 0, 0)]
    [Arguments(256, 0, 0, 0, 0)]
    [Arguments(0, -1, 0, 0, 0)]
    [Arguments(0, 4, 0, 0, 0)]
    [Arguments(0, 0, 1296, 0, 0)]
    [Arguments(0, 0, 0, 1296, 0)]
    [Arguments(0, 0, 0, 0, 362_880)]
    public async Task Constructor_RejectsOutOfRangeComponents(int seed, int rotation, int rows, int columns, int digits) =>
        await Assert.That(() => new SudokuId(seed, rotation, rows, columns, digits))
            .Throws<ArgumentOutOfRangeException>();

    [Test]
    public async Task Difficulty_FollowsTheSeedIdBlock()
    {
        await Assert.That(new SudokuId(0, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Easy);
        await Assert.That(new SudokuId(64, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Medium);
        await Assert.That(new SudokuId(128, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Hard);
        await Assert.That(new SudokuId(201, 0, 0, 0, 0).Difficulty).IsEqualTo(Difficulty.Expert);
    }

    // Independent encoder: 10 payload symbols, then the check symbol (sum of position * symbol mod 31).
    private static string Encode(ulong value)
    {
        var symbols = new int[10];
        for (var i = 9; i >= 0; i--)
        {
            symbols[i] = (int)(value & 31);
            value >>= 5;
        }

        var check = symbols.Select((symbol, i) => (i + 1) * symbol).Sum() % 31;
        return string.Concat(symbols.Append(check).Select(s => Alphabet[s]));
    }
}

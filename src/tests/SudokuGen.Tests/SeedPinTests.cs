using System.Security.Cryptography;
using System.Text;

namespace SudokuGen.Tests;

// Seed ids are persisted inside SudokuIds, so a seed's content must never change once released.
// When appending a seed, add its id and hash here.
public class SeedPinTests
{
    private static readonly Dictionary<int, string> Pinned = new()
    {
        [0] = "1A976C8E5CCCA22E",
        [1] = "828C90ABECE465D2",
        [2] = "404BCFA5F877264E",
        [3] = "DCEE22D3E609BBCA",
        [4] = "A7BB9240D63034A0",
        [5] = "D0150B7AC489ED56",
        [6] = "997616E8B55914DD",
        [7] = "ED85C2787068752C",
        [8] = "E17D1E692276A2C0",
        [9] = "ECE6E14FC03F83FD",
        [64] = "16F474BA34667F18",
        [65] = "282F1A3CCA6846E8",
        [66] = "225CF42569DBB61F",
        [67] = "5A401032595D9BEA",
        [68] = "1F7989B895BE0C37",
        [69] = "B0D01147751A7FDC",
        [70] = "B9D98073C2A5493C",
        [71] = "17B63BE8584FEA77",
        [72] = "8C281CF0DF8C7A03",
        [73] = "ABEF81D81D9E5CFB",
        [128] = "49E1AF3330921C80",
        [129] = "3C34EF0E5F9C660C",
        [130] = "4DD68CA183EF3DD8",
        [131] = "9783587E14FDECFF",
        [132] = "13786F1308172F17",
        [133] = "C62FD2350590C164",
        [134] = "BD873BB768039824",
        [135] = "9D079C7FC72958E4",
        [136] = "4892D99F40CCEA9B",
        [137] = "20ECF801872C3F80",
        [192] = "6E8EAE392A3B9EA1",
        [193] = "F3F2BDA8ECFE4C26",
        [194] = "0905376CE66829C0",
        [195] = "7640AA2A9479616C",
        [196] = "1E0927FF1CDBE37E",
        [197] = "62D42E1C4181EAF2",
        [198] = "DAE3C4B621D260E8",
        [199] = "50FD9A2C0AF3C8FC",
        [200] = "A7BDA836FC801AC1",
        [201] = "3C85E9F5FBAFA1FF",
    };

    [Test]
    public async Task EverySeed_IsPinnedAndUnchanged()
    {
        var actual = Enum.GetValues<Difficulty>()
            .SelectMany(Seeds.For)
            .ToDictionary(seed => seed.Id, seed => Hash(seed));

        await Assert.That(actual.Keys.Order().SequenceEqual(Pinned.Keys.Order())).IsTrue();
        foreach (var (id, hash) in actual)
        {
            await Assert.That(hash).IsEqualTo(Pinned[id]);
        }
    }

    private static string Hash(Seed seed) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{seed.Puzzle}|{seed.Solution}")))[..16];
}

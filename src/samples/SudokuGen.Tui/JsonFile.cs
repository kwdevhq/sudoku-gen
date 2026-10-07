using System.Text.Json;

namespace SudokuGen.Tui;

/// <summary>
/// A JSON file that is read and written on a best-effort basis: a missing or damaged file reads as nothing and a failed
/// write is reported, never thrown, because a broken disk must not stop a game.
/// </summary>
internal sealed class JsonFile(string path)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, RespectNullableAnnotations = true };

    public T? Read<T>()
        where T : class
    {
        try
        {
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Writes through a temporary file so a crash never leaves half a file behind.</summary>
    public bool Write<T>(T value)
    {
        var temp = path + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(temp, JsonSerializer.Serialize(value, Options));
            File.Move(temp, path, overwrite: true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Delete()
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
            // Best effort: a stale save is ignored on the next start if it no longer matches a game.
        }
    }
}

using System.IO;

namespace FavaStudio.Services;

public static class FileService
{
    public static string ReadText(string path) => File.Exists(path) ? File.ReadAllText(path) : "";
    public static void WriteText(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, content);
    }
}

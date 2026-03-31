using System.IO;

namespace FavaStudio.Services;

public static class FileService
{
    public static string ReadText(string path) => File.Exists(path) ? File.ReadAllText(path) : "";
    public static void WriteText(string path, string content) => File.WriteAllText(path, content);
}

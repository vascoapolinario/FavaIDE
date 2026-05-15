using System.IO;

namespace FavaStudio.Models;

public class RecentProjectItem
{
    public RecentProjectItem(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(path.TrimEnd(
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar));

        if (string.IsNullOrWhiteSpace(Name))
            Name = path;

        var parent = System.IO.Path.GetDirectoryName(path);
        Location = string.IsNullOrWhiteSpace(parent) ? path : parent;
        Details = Directory.Exists(path)
            ? $"Last changed {Directory.GetLastWriteTime(path):MMM d, yyyy h:mm tt}"
            : "Missing project folder";
    }

    public string Name { get; }
    public string Path { get; }
    public string Location { get; }
    public string Details { get; }
}

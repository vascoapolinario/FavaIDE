using System.IO;

namespace FavaStudio.Models;

public class RecentProjectItem
{
    public RecentProjectItem(string path, ProjectMetadata? metadata = null)
    {
        Path = path;
        var folderName = System.IO.Path.GetFileName(path.TrimEnd(
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar));

        if (string.IsNullOrWhiteSpace(folderName))
            folderName = path;

        Name = string.IsNullOrWhiteSpace(metadata?.Title) ? folderName : metadata.Title.Trim();
        Description = string.IsNullOrWhiteSpace(metadata?.Description) ? "No description yet." : metadata.Description.Trim();

        var parent = System.IO.Path.GetDirectoryName(path);
        Location = string.IsNullOrWhiteSpace(parent) ? path : parent;
        Details = Directory.Exists(path)
            ? $"Last changed {Directory.GetLastWriteTime(path):MMM d, yyyy h:mm tt}"
            : "Missing project folder";
    }

    public string Name { get; }
    public string Path { get; }
    public string Location { get; }
    public string Description { get; }
    public string Details { get; }
}

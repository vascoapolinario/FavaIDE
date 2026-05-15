using System;
using System.Collections.ObjectModel;

namespace FavaStudio.Models;

public class ProjectNode
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public bool IsExpanded { get; set; }
    public ObservableCollection<ProjectNode> Children { get; } = new();

    public string Icon => IsDirectory
        ? "DIR"
        : Name.EndsWith(".fava", StringComparison.OrdinalIgnoreCase)
            ? "F"
            : Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? "TXT"
                : "FILE";
}

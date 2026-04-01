using System.Collections.ObjectModel;
using System;

namespace FavaStudio.Models;

public class ProjectNode
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public bool IsExpanded { get; set; }
    public ObservableCollection<ProjectNode> Children { get; } = new();

    public string Icon => IsDirectory
        ? "📁"
        : Name.EndsWith(".fava", StringComparison.OrdinalIgnoreCase)
            ? "🟧F"
            : Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? "📝"
                : "📄";
}

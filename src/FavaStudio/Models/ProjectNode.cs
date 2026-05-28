using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FavaStudio.Models;

public class ProjectNode
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public bool IsRoot { get; set; }
    public bool IsExpanded { get; set; }
    public ObservableCollection<ProjectNode> Children { get; } = new();

    public string Icon => IsDirectory ? "DIR" : EditorTab.GetFileBadge(Name);
    public string TreeIcon => IsDirectory ? "\uE8B7" : "\uE8A5";
    public Brush BadgeBrush => IsDirectory
        ? new SolidColorBrush(Color.FromRgb(47, 49, 54))
        : EditorTab.GetFileBadgeBrush(Name);
    public Brush BadgeTextBrush => IsDirectory
        ? new SolidColorBrush(Color.FromRgb(154, 164, 178))
        : EditorTab.GetFileBadgeTextBrush(Name);
    public Brush TreeIconBrush => IsDirectory
        ? new SolidColorBrush(Color.FromRgb(255, 174, 92))
        : EditorTab.GetFileBadgeBrush(Name);
    public string Detail => IsRoot
        ? FullPath
        : IsDirectory
            ? $"{Children.Count} item{(Children.Count == 1 ? "" : "s")}"
            : EditorTab.GetFileBadge(Name);
}

using System.Collections.ObjectModel;

namespace FavaStudio.Models;

public class ProjectNode
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public ObservableCollection<ProjectNode> Children { get; } = new();
}

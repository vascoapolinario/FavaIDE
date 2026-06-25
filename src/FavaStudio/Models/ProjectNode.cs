using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
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
    public string TreeIcon => IconKind switch
    {
        ProjectNodeIconKind.Root => "\uE8B7",
        ProjectNodeIconKind.Folder => "\uE8B7",
        ProjectNodeIconKind.FavaModule => "M",
        ProjectNodeIconKind.FavaFile => "F",
        ProjectNodeIconKind.TestInput => "IN",
        ProjectNodeIconKind.TestOutput => "OUT",
        ProjectNodeIconKind.Bytecode => "BC",
        ProjectNodeIconKind.Text => "TXT",
        ProjectNodeIconKind.Config => "\uE713",
        _ => "\uE8A5"
    };
    public FontFamily TreeIconFontFamily => IconKind is ProjectNodeIconKind.Root or ProjectNodeIconKind.Folder or ProjectNodeIconKind.Config or ProjectNodeIconKind.Generic
        ? new FontFamily("Segoe MDL2 Assets")
        : new FontFamily("Segoe UI");
    public Brush BadgeBrush => IsDirectory
        ? new SolidColorBrush(Color.FromRgb(47, 49, 54))
        : EditorTab.GetFileBadgeBrush(Name);
    public Brush BadgeTextBrush => IsDirectory
        ? new SolidColorBrush(Color.FromRgb(154, 164, 178))
        : EditorTab.GetFileBadgeTextBrush(Name);
    public Brush TreeIconBrush => IconKind switch
    {
        ProjectNodeIconKind.Root => new SolidColorBrush(Color.FromRgb(255, 174, 92)),
        ProjectNodeIconKind.Folder => new SolidColorBrush(Color.FromRgb(255, 174, 92)),
        ProjectNodeIconKind.FavaModule => new SolidColorBrush(Color.FromRgb(197, 155, 255)),
        ProjectNodeIconKind.FavaFile => new SolidColorBrush(Color.FromRgb(86, 214, 163)),
        ProjectNodeIconKind.TestInput => new SolidColorBrush(Color.FromRgb(123, 193, 255)),
        ProjectNodeIconKind.TestOutput => new SolidColorBrush(Color.FromRgb(71, 209, 106)),
        ProjectNodeIconKind.Bytecode => new SolidColorBrush(Color.FromRgb(255, 154, 61)),
        ProjectNodeIconKind.Text => new SolidColorBrush(Color.FromRgb(154, 164, 178)),
        ProjectNodeIconKind.Config => new SolidColorBrush(Color.FromRgb(232, 202, 112)),
        _ => EditorTab.GetFileBadgeBrush(Name)
    };
    public Brush TreeIconBackgroundBrush => IconKind switch
    {
        ProjectNodeIconKind.Root => new SolidColorBrush(Color.FromRgb(52, 42, 29)),
        ProjectNodeIconKind.Folder => new SolidColorBrush(Color.FromRgb(52, 42, 29)),
        ProjectNodeIconKind.FavaModule => new SolidColorBrush(Color.FromRgb(45, 35, 65)),
        ProjectNodeIconKind.FavaFile => new SolidColorBrush(Color.FromRgb(24, 55, 47)),
        ProjectNodeIconKind.TestInput => new SolidColorBrush(Color.FromRgb(28, 48, 68)),
        ProjectNodeIconKind.TestOutput => new SolidColorBrush(Color.FromRgb(24, 58, 36)),
        ProjectNodeIconKind.Bytecode => new SolidColorBrush(Color.FromRgb(60, 39, 22)),
        ProjectNodeIconKind.Text => new SolidColorBrush(Color.FromRgb(43, 45, 50)),
        ProjectNodeIconKind.Config => new SolidColorBrush(Color.FromRgb(55, 49, 30)),
        _ => new SolidColorBrush(Color.FromRgb(38, 40, 45))
    };
    public Brush TreeIconBorderBrush => IconKind switch
    {
        ProjectNodeIconKind.Root => new SolidColorBrush(Color.FromRgb(112, 82, 48)),
        ProjectNodeIconKind.Folder => new SolidColorBrush(Color.FromRgb(112, 82, 48)),
        ProjectNodeIconKind.FavaModule => new SolidColorBrush(Color.FromRgb(87, 63, 120)),
        ProjectNodeIconKind.FavaFile => new SolidColorBrush(Color.FromRgb(49, 92, 80)),
        ProjectNodeIconKind.TestInput => new SolidColorBrush(Color.FromRgb(49, 83, 114)),
        ProjectNodeIconKind.TestOutput => new SolidColorBrush(Color.FromRgb(47, 96, 58)),
        ProjectNodeIconKind.Bytecode => new SolidColorBrush(Color.FromRgb(118, 74, 34)),
        ProjectNodeIconKind.Text => new SolidColorBrush(Color.FromRgb(70, 75, 84)),
        ProjectNodeIconKind.Config => new SolidColorBrush(Color.FromRgb(100, 87, 42)),
        _ => new SolidColorBrush(Color.FromRgb(60, 63, 65))
    };
    public string Detail => IsRoot
        ? FullPath
        : IsDirectory
            ? $"{Children.Count} item{(Children.Count == 1 ? "" : "s")}"
            : IconKind switch
            {
                ProjectNodeIconKind.FavaModule => "Fava module",
                ProjectNodeIconKind.FavaFile => "Fava program",
                ProjectNodeIconKind.TestInput => "Test input",
                ProjectNodeIconKind.TestOutput => "Expected output",
                ProjectNodeIconKind.Bytecode => "Bytecode",
                ProjectNodeIconKind.Config => "Config",
                _ => EditorTab.GetFileBadge(Name)
            };

    private ProjectNodeIconKind IconKind
    {
        get
        {
            if (IsRoot)
                return ProjectNodeIconKind.Root;
            if (IsDirectory)
                return ProjectNodeIconKind.Folder;

            var extension = Path.GetExtension(Name).ToLowerInvariant();
            var normalizedPath = FullPath.Replace('\\', '/');
            if (extension == ".fava")
            {
                if (normalizedPath.Contains("/tests/inputs/", StringComparison.OrdinalIgnoreCase))
                    return ProjectNodeIconKind.TestInput;

                return IsFavaModuleFile() ? ProjectNodeIconKind.FavaModule : ProjectNodeIconKind.FavaFile;
            }

            if (normalizedPath.Contains("/tests/outputs/", StringComparison.OrdinalIgnoreCase))
                return ProjectNodeIconKind.TestOutput;
            if (extension is ".bc" or ".bytecode")
                return ProjectNodeIconKind.Bytecode;
            if (extension is ".txt" or ".md" or ".log")
                return ProjectNodeIconKind.Text;
            if (extension is ".json" or ".toml" or ".xml" or ".config" or ".props" or ".targets")
                return ProjectNodeIconKind.Config;

            return ProjectNodeIconKind.Generic;
        }
    }

    private bool IsFavaModuleFile()
    {
        try
        {
            if (!File.Exists(FullPath))
                return false;

            using var reader = new StreamReader(FullPath);
            var buffer = new char[4096];
            var count = reader.Read(buffer, 0, buffer.Length);
            var text = new string(buffer, 0, count);
            return Regex.IsMatch(text, @"(?im)^\s*module\s+[A-Za-z_][A-Za-z0-9_]*\s*;");
        }
        catch
        {
            return false;
        }
    }
}

internal enum ProjectNodeIconKind
{
    Root,
    Folder,
    FavaFile,
    FavaModule,
    TestInput,
    TestOutput,
    Bytecode,
    Text,
    Config,
    Generic
}

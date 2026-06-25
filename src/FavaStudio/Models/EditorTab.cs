using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace FavaStudio.Models;

public class EditorTab : INotifyPropertyChanged
{
    private string _filePath = "";
    private bool _isDirty;
    private bool _isMarkdownPreview;
    private string _content = "";

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value)
                return;

            _filePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(FileBadge));
            OnPropertyChanged(nameof(FileBadgeBrush));
            OnPropertyChanged(nameof(FileBadgeTextBrush));
            OnPropertyChanged(nameof(Header));
        }
    }
    public string FileName => Path.GetFileName(FilePath);
    public string FileBadge => GetFileBadge(FilePath);
    public Brush FileBadgeBrush => GetFileBadgeBrush(FilePath);
    public Brush FileBadgeTextBrush => GetFileBadgeTextBrush(FilePath);

    public static string GetFileBadge(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        var normalizedPath = filePath.Replace('\\', '/');
        if (extension.Equals(".fava", StringComparison.OrdinalIgnoreCase))
        {
            if (normalizedPath.Contains("/tests/inputs/", StringComparison.OrdinalIgnoreCase))
                return "IN";

            return IsFavaModuleFile(filePath) ? "M" : "F";
        }
        if (normalizedPath.Contains("/tests/outputs/", StringComparison.OrdinalIgnoreCase))
            return "OUT";
        if (extension.Equals(".bc", StringComparison.OrdinalIgnoreCase) || extension.Equals(".bytecode", StringComparison.OrdinalIgnoreCase))
            return "BC";
        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) || extension.Equals(".md", StringComparison.OrdinalIgnoreCase) || extension.Equals(".log", StringComparison.OrdinalIgnoreCase))
            return "TXT";
        return "..";
    }

    public static Brush GetFileBadgeBrush(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        var normalizedPath = filePath.Replace('\\', '/');
        if (extension.Equals(".fava", StringComparison.OrdinalIgnoreCase))
        {
            if (normalizedPath.Contains("/tests/inputs/", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(123, 193, 255));

            return IsFavaModuleFile(filePath)
                ? new SolidColorBrush(Color.FromRgb(197, 155, 255))
                : new SolidColorBrush(Color.FromRgb(86, 214, 163));
        }
        if (normalizedPath.Contains("/tests/outputs/", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(71, 209, 106));
        if (extension.Equals(".bc", StringComparison.OrdinalIgnoreCase) || extension.Equals(".bytecode", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(255, 154, 61));
        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) || extension.Equals(".md", StringComparison.OrdinalIgnoreCase) || extension.Equals(".log", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(154, 164, 178));
        return new SolidColorBrush(Color.FromRgb(58, 63, 71));
    }

    public static Brush GetFileBadgeTextBrush(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        var normalizedPath = filePath.Replace('\\', '/');
        if (extension.Equals(".fava", StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.Contains("/tests/outputs/", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".bc", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".bytecode", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".log", StringComparison.OrdinalIgnoreCase))
        {
            return new SolidColorBrush(Color.FromRgb(18, 22, 24));
        }

        return new SolidColorBrush(Color.FromRgb(230, 234, 240));
    }

    private static bool IsFavaModuleFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            using var reader = new StreamReader(filePath);
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

    public string Header => _isDirty ? $"{FileName} *" : FileName;

    public string Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            _content = value;
            OnPropertyChanged();
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Header));
        }
    }

    public bool IsMarkdownPreview
    {
        get => _isMarkdownPreview;
        set
        {
            if (_isMarkdownPreview == value) return;
            _isMarkdownPreview = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FavaStudio.Models;

public class EditorTab : INotifyPropertyChanged
{
    private bool _isDirty;
    private string _content = "";

    public string FilePath { get; set; } = "";
    public string FileName => Path.GetFileName(FilePath);
    public string FileBadge => GetFileBadge(FilePath);
    public Brush FileBadgeBrush => GetFileBadgeBrush(FilePath);
    public Brush FileBadgeTextBrush => GetFileBadgeTextBrush(FilePath);

    public static string GetFileBadge(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (extension.Equals(".fava", StringComparison.OrdinalIgnoreCase))
            return "F";
        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            return "TXT";
        return "..";
    }

    public static Brush GetFileBadgeBrush(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (extension.Equals(".fava", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(255, 154, 61));
        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(108, 166, 255));
        return new SolidColorBrush(Color.FromRgb(58, 63, 71));
    }

    public static Brush GetFileBadgeTextBrush(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (extension.Equals(".fava", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(26, 17, 16));
        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Color.FromRgb(10, 20, 36));
        return new SolidColorBrush(Color.FromRgb(230, 234, 240));
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

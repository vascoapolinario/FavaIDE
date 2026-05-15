using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace FavaStudio.Models;

public class EditorTab : INotifyPropertyChanged
{
    private bool _isDirty;
    private string _content = "";

    public string FilePath { get; set; } = "";
    public string FileName => Path.GetFileName(FilePath);
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

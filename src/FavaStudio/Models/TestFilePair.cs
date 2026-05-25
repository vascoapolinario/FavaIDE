using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace FavaStudio.Models;

public class TestFilePair : INotifyPropertyChanged
{
    private string _inputFile = "";
    private string _expectedOutputFile = "";
    private string _result = "Pending";
    private string _actualOutput = "";
    private string _expectedOutput = "";
    private string _diffOutput = "";

    public string Name => string.IsNullOrWhiteSpace(InputFile) ? "(unassigned)" : Path.GetFileNameWithoutExtension(InputFile);
    public string InputFileName => string.IsNullOrWhiteSpace(InputFile) ? "(choose input)" : Path.GetFileName(InputFile);
    public string ExpectedOutputFileName => string.IsNullOrWhiteSpace(ExpectedOutputFile) ? "(choose expected)" : Path.GetFileName(ExpectedOutputFile);
    public string InputFile { get => _inputFile; set { _inputFile = value; OnPropertyChanged(); OnPropertyChanged(nameof(Name)); OnPropertyChanged(nameof(InputFileName)); } }
    public string ExpectedOutputFile { get => _expectedOutputFile; set { _expectedOutputFile = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExpectedOutputFileName)); } }
    public string Result { get => _result; set { _result = value; OnPropertyChanged(); } }
    public string ActualOutput { get => _actualOutput; set { _actualOutput = value; OnPropertyChanged(); } }
    public string ExpectedOutput { get => _expectedOutput; set { _expectedOutput = value; OnPropertyChanged(); } }
    public string DiffOutput { get => _diffOutput; set { _diffOutput = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

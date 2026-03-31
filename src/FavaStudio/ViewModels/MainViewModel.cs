using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using FavaStudio.Models;
using FavaStudio.Services;

namespace FavaStudio.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly TextEditor _editor;
    private string? _currentFile;

    public ObservableCollection<string> ProjectFiles { get; } = new();
    public ObservableCollection<TestResult> TestResults { get; } = new();

    public SettingsService Settings { get; } = SettingsService.Load();

    private string _consoleOutput = "";
    public string ConsoleOutput { get => _consoleOutput; set { _consoleOutput = value; OnPropertyChanged(); } }

    private string _statusText = "Ready.";
    public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

    private string _testSummary = "";
    public string TestSummary { get => _testSummary; set { _testSummary = value; OnPropertyChanged(); } }

    private Brush _statusColor = Brushes.LightGray;
    public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

    private TestResult? _selectedTestResult;
    public TestResult? SelectedTestResult
    {
        get => _selectedTestResult;
        set { _selectedTestResult = value; OnPropertyChanged(); }
    }

    public string FooterText => "Fava Studio • built for your Fava compiler";

    public bool ShowTestOutput
    {
        get => Settings.ShowTestOutput;
        set { Settings.ShowTestOutput = value; OnPropertyChanged(); }
    }

    public RelayCommand OpenProjectCommand { get; }
    public RelayCommand NewFileCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand RunCurrentCommand { get; }
    public RelayCommand RunAllTestsCommand { get; }
    public RelayCommand RunSelectedTestsCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }

    public string? SelectedFile
    {
        get => _currentFile;
        set
        {
            _currentFile = value;
            if (!string.IsNullOrWhiteSpace(_currentFile))
            {
                _editor.Text = FileService.ReadText(_currentFile);
            }
            OnPropertyChanged();
        }
    }

    public MainViewModel(TextEditor editor)
    {
        _editor = editor;

        OpenProjectCommand = new RelayCommand(_ => OpenProject());
        NewFileCommand = new RelayCommand(_ => NewFile());
        SaveFileCommand = new RelayCommand(_ => SaveFile());
        RunCurrentCommand = new RelayCommand(_ => RunCurrentFile(), _ => !string.IsNullOrWhiteSpace(_currentFile));
        RunAllTestsCommand = new RelayCommand(_ => RunAllTests());
        RunSelectedTestsCommand = new RelayCommand(_ => RunSelectedTest(), _ => SelectedTestResult != null);
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        OpenSettingsCommand = new RelayCommand(_ => StatusText = "Settings tab ready.");

        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
        {
            LoadProject(Settings.ProjectRoot);
        }
    }

    private void OpenProject()
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = false,
            FileName = "Select folder",
            Filter = "Folder|."
        };

        if (dialog.ShowDialog() == true)
        {
            var folder = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                LoadProject(folder);
                Settings.ProjectRoot = folder;
                Settings.Save();
            }
        }
    }

    private void LoadProject(string folder)
    {
        ProjectFiles.Clear();
        try
        {
            foreach (var file in Directory.GetFiles(folder, "*.fava", SearchOption.AllDirectories))
            {
                ProjectFiles.Add(file);
            }
            StatusText = $"Loaded project: {folder}";
            StatusColor = Brushes.LightBlue;
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to load project: {ex.Message}";
            StatusColor = Brushes.IndianRed;
        }
    }

    private void NewFile()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Fava file (*.fava)|*.fava"
        };

        if (dialog.ShowDialog() == true)
        {
            FileService.WriteText(dialog.FileName, "");
            ProjectFiles.Add(dialog.FileName);
            SelectedFile = dialog.FileName;
        }
    }

    private void SaveFile()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;
        FileService.WriteText(_currentFile, _editor.Text);
        StatusText = $"Saved: {_currentFile}";
        StatusColor = Brushes.LightGreen;
    }

    private async void RunCurrentFile()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;

        SaveFile();
        StatusText = "Running…";
        StatusColor = Brushes.LightGray;

        var runner = new JavaCompilerService(Settings);
        var result = await runner.RunFileAsync(_currentFile);

        ConsoleOutput = result.Output;
        StatusText = result.Success ? "✅ Execution complete" : "❌ Execution failed";
        StatusColor = result.Success ? Brushes.LightGreen : Brushes.IndianRed;
    }

    private async void RunAllTests()
    {
        TestResults.Clear();
        TestSummary = "Running tests…";
        StatusColor = Brushes.LightGray;

        var runner = new TestRunnerService(Settings);
        var results = await runner.RunAllTestsAsync();

        foreach (var r in results) TestResults.Add(r);

        var passed = results.Count(r => r.Passed);
        var total = results.Count;

        TestSummary = passed == total
            ? $"✅ ALL TESTS PASSED ({passed}/{total})"
            : $"❌ {passed}/{total} tests passed";

        StatusColor = passed == total ? Brushes.LightGreen : Brushes.IndianRed;
    }

    private async void RunSelectedTest()
    {
        if (SelectedTestResult is null) return;

        var name = SelectedTestResult.Name;
        TestSummary = $"Running '{name}'…";
        StatusColor = Brushes.LightGray;

        var runner = new TestRunnerService(Settings);
        var result = await runner.RunSingleTestAsync(name);

        var idx = TestResults.IndexOf(SelectedTestResult);
        if (idx >= 0)
            TestResults[idx] = result;
        else
            TestResults.Add(result);

        SelectedTestResult = result;
        TestSummary = result.Passed ? $"✅ '{name}' passed" : $"❌ '{name}' failed";
        StatusColor = result.Passed ? Brushes.LightGreen : Brushes.IndianRed;
    }

    private void SaveSettings()
    {
        Settings.Save();
        StatusText = "Settings saved.";
        StatusColor = Brushes.LightGreen;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

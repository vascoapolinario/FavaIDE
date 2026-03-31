using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using FavaStudio.Models;
using FavaStudio.Services;
using System.IO;

namespace FavaStudio.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly TextEditor _editor;
    private string? _currentFile;
    private bool _isLiveChecking;
    private readonly DispatcherTimer _liveCheckTimer;

    public ObservableCollection<string> ProjectFiles { get; } = new();
    public ObservableCollection<TestResult> TestResults { get; } = new();
    public ObservableCollection<FavaDiagnostic> Diagnostics { get; } = new();

    public SettingsService Settings { get; } = SettingsService.Load();

    private string _consoleOutput = "";
    public string ConsoleOutput { get => _consoleOutput; set { _consoleOutput = value; OnPropertyChanged(); } }

    private string _statusText = "Ready.";
    public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

    public string DiagnosticsHeader => Diagnostics.Count == 0
        ? "Errors"
        : $"Errors ({Diagnostics.Count})";

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

    public string CurrentProjectDirectory => string.IsNullOrWhiteSpace(Settings.ProjectRoot)
        ? "Project directory: (not set)"
        : Settings.ProjectRoot;

    public string CurrentFileName => string.IsNullOrWhiteSpace(_currentFile)
        ? "No file open"
        : Path.GetFileName(_currentFile);

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
    public RelayCommand BrowseJavaPathCommand { get; }
    public RelayCommand BrowseCompilerRootCommand { get; }
    public RelayCommand BrowseAntlrJarCommand { get; }
    public RelayCommand BrowseInputsDirCommand { get; }
    public RelayCommand BrowseOutputsDirCommand { get; }

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
            OnPropertyChanged(nameof(CurrentFileName));
        }
    }

    public MainViewModel(TextEditor editor)
    {
        _editor = editor;

        _liveCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
        _liveCheckTimer.Tick += async (_, _) =>
        {
            _liveCheckTimer.Stop();
            await RunLiveCheckAsync();
        };
        _editor.TextChanged += (_, _) =>
        {
            _liveCheckTimer.Stop();
            _liveCheckTimer.Start();
        };

        OpenProjectCommand = new RelayCommand(_ => OpenProject());
        NewFileCommand = new RelayCommand(_ => NewFile());
        SaveFileCommand = new RelayCommand(_ => SaveFile());
        RunCurrentCommand = new RelayCommand(_ => RunCurrentFile(), _ => !string.IsNullOrWhiteSpace(_currentFile));
        RunAllTestsCommand = new RelayCommand(_ => RunAllTests());
        RunSelectedTestsCommand = new RelayCommand(_ => RunSelectedTest(), _ => SelectedTestResult != null);
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        OpenSettingsCommand = new RelayCommand(_ => StatusText = "Settings tab ready.");
        BrowseJavaPathCommand = new RelayCommand(_ => BrowseJavaPath());
        BrowseCompilerRootCommand = new RelayCommand(_ => BrowseFolder(v => Settings.CompilerRoot = v, "Compiler Root Folder"));
        BrowseAntlrJarCommand = new RelayCommand(_ => BrowseAntlrJar());
        BrowseInputsDirCommand = new RelayCommand(_ => BrowseFolder(v => Settings.InputsDir = v, "Test Inputs Folder"));
        BrowseOutputsDirCommand = new RelayCommand(_ => BrowseFolder(v => Settings.OutputsDir = v, "Test Outputs Folder"));

        EnsureProjectDirectory();
        LoadProject(Settings.ProjectRoot);
    }

    private void OpenProject()
    {
        var dialog = new OpenFolderDialog { Title = "Select Project Folder" };

        if (dialog.ShowDialog() == true)
        {
            var folder = dialog.FolderName;
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                LoadProject(folder);
                Settings.ProjectRoot = folder;
                Settings.Save();
                OnPropertyChanged(nameof(CurrentProjectDirectory));
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
            OnPropertyChanged(nameof(CurrentProjectDirectory));
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
            InitialDirectory = Directory.Exists(Settings.ProjectRoot) ? Settings.ProjectRoot : null,
            Filter = "Fava file (*.fava)|*.fava"
        };

        if (dialog.ShowDialog() == true)
        {
            FileService.WriteText(dialog.FileName, "");
            if (!ProjectFiles.Contains(dialog.FileName))
                ProjectFiles.Add(dialog.FileName);
            SelectedFile = dialog.FileName;
        }
    }

    private void EnsureProjectDirectory()
    {
        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot) && Directory.Exists(Settings.ProjectRoot))
            return;

        var defaultProjectRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "FavaStudioProject");

        Directory.CreateDirectory(defaultProjectRoot);
        Settings.ProjectRoot = defaultProjectRoot;
        Settings.Save();
        OnPropertyChanged(nameof(CurrentProjectDirectory));
    }

    private void SaveFile()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;
        FileService.WriteText(_currentFile, _editor.Text);
        StatusText = $"Saved: {_currentFile}";
        StatusColor = Brushes.LightGreen;
    }

    private async Task RunLiveCheckAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;
        if (_isLiveChecking) return;

        _isLiveChecking = true;
        try
        {
            FileService.WriteText(_currentFile, _editor.Text);

            var runner = new JavaCompilerService(Settings);
            var result = await runner.RunFileAsync(_currentFile);

            var diagnostics = DiagnosticsParser.Parse(result.Output);
            Diagnostics.Clear();
            foreach (var d in diagnostics) Diagnostics.Add(d);
            OnPropertyChanged(nameof(DiagnosticsHeader));

            ConsoleOutput = result.Output;

            if (diagnostics.Count > 0)
            {
                StatusText = $"⚠️ {diagnostics.Count} error(s) found";
                StatusColor = Brushes.Orange;
            }
            else
            {
                StatusText = result.Success ? "✅ No errors" : "❌ Execution failed";
                StatusColor = result.Success ? Brushes.LightGreen : Brushes.IndianRed;
            }
        }
        finally
        {
            _isLiveChecking = false;
        }
    }

    private async void RunCurrentFile()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;

        SaveFile();
        StatusText = "Running…";
        StatusColor = Brushes.LightGray;

        var runner = new JavaCompilerService(Settings);
        var result = await runner.RunFileAsync(_currentFile);

        var diagnostics = DiagnosticsParser.Parse(result.Output);
        Diagnostics.Clear();
        foreach (var d in diagnostics) Diagnostics.Add(d);
        OnPropertyChanged(nameof(DiagnosticsHeader));

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

    private void BrowseJavaPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Java Executable",
            Filter = "Java Executable (java.exe)|java.exe|All Files|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            Settings.JavaPath = dialog.FileName;
            OnPropertyChanged(nameof(Settings));
        }
    }

    private void BrowseAntlrJar()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select ANTLR Jar",
            Filter = "JAR Files|*.jar|All Files|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            Settings.AntlrJar = dialog.FileName;
            OnPropertyChanged(nameof(Settings));
        }
    }

    private void BrowseFolder(Action<string> setter, string title)
    {
        var dialog = new OpenFolderDialog { Title = $"Select {title}" };
        if (dialog.ShowDialog() == true)
        {
            setter(dialog.FolderName);
            OnPropertyChanged(nameof(Settings));
        }
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

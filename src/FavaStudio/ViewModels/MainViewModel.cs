using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using FavaStudio.Models;
using FavaStudio.Services;
using Microsoft.Win32;

namespace FavaStudio.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly TextEditor _editor;
    private bool _isLiveChecking;
    private readonly DispatcherTimer _liveCheckTimer;
    private string? _currentFile;
    private ProjectNode? _selectedProjectNode;
    private Brush _statusColor = Brushes.LightGray;
    private string _statusText = "Ready.";
    private string _vmOutput = "";
    private string _constantPoolOutput = "";
    private string _instructionsOutput = "";
    private bool _showDiagnostics = true;
    private bool _isSettingsViewVisible;
    private bool _isToolsViewVisible;
    private bool _showOutputOnly = true;
    private bool _toolCompareFullOutput;
    private TestResult? _selectedTestResult;
    private string _testSummary = "";

    public ObservableCollection<ProjectNode> ProjectTree { get; } = new();
    public ObservableCollection<FavaDiagnostic> Diagnostics { get; } = new();
    public ObservableCollection<TestResult> TestResults { get; } = new();
    public ObservableCollection<TestFilePair> ToolTestPairs { get; } = new();

    public SettingsService Settings { get; } = SettingsService.Load();

    public string FooterText => "Fava Studio • built for your Fava compiler";
    public string CurrentFileName => string.IsNullOrWhiteSpace(_currentFile) ? "No file open" : Path.GetFileName(_currentFile);
    public string CurrentProjectDirectory => string.IsNullOrWhiteSpace(Settings.ProjectRoot) ? "Project directory: (not set)" : Settings.ProjectRoot;
    public string DiagnosticsHeader => Diagnostics.Count == 0 ? "Errors" : $"Errors ({Diagnostics.Count})";

    public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }
    public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }
    public string VmOutput { get => _vmOutput; set { _vmOutput = value; OnPropertyChanged(); } }
    public string ConstantPoolOutput { get => _constantPoolOutput; set { _constantPoolOutput = value; OnPropertyChanged(); } }
    public string InstructionsOutput { get => _instructionsOutput; set { _instructionsOutput = value; OnPropertyChanged(); } }
    public bool ShowDiagnostics { get => _showDiagnostics; set { _showDiagnostics = value; OnPropertyChanged(); } }
    public bool IsSettingsViewVisible { get => _isSettingsViewVisible; set { _isSettingsViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsToolsViewVisible { get => _isToolsViewVisible; set { _isToolsViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsWorkspaceVisible => !IsSettingsViewVisible && !IsToolsViewVisible;
    public bool ShowOutputOnly { get => _showOutputOnly; set { _showOutputOnly = value; OnPropertyChanged(); } }
    public bool ToolCompareFullOutput { get => _toolCompareFullOutput; set { _toolCompareFullOutput = value; OnPropertyChanged(); } }
    public string TestSummary { get => _testSummary; set { _testSummary = value; OnPropertyChanged(); } }
    public bool ShowTestOutput { get => Settings.ShowTestOutput; set { Settings.ShowTestOutput = value; OnPropertyChanged(); } }

    public TestResult? SelectedTestResult
    {
        get => _selectedTestResult;
        set { _selectedTestResult = value; OnPropertyChanged(); }
    }

    public ProjectNode? SelectedProjectNode
    {
        get => _selectedProjectNode;
        set
        {
            _selectedProjectNode = value;
            OnPropertyChanged();
            if (_selectedProjectNode is not null && !_selectedProjectNode.IsDirectory)
            {
                _currentFile = _selectedProjectNode.FullPath;
                _editor.Text = FileService.ReadText(_currentFile);
                OnPropertyChanged(nameof(CurrentFileName));
            }
        }
    }

    public RelayCommand OpenProjectCommand { get; }
    public RelayCommand CreateProjectCommand { get; }
    public RelayCommand NewFavaFileCommand { get; }
    public RelayCommand NewTextFileCommand { get; }
    public RelayCommand DeleteNodeCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand RunCurrentCommand { get; }
    public RelayCommand ToggleDiagnosticsCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }
    public RelayCommand BackToEditorCommand { get; }
    public RelayCommand BrowseJavaPathCommand { get; }
    public RelayCommand BrowseCompilerRootCommand { get; }
    public RelayCommand BrowseAntlrJarCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand OpenToolsCommand { get; }
    public RelayCommand AddToolInputFileCommand { get; }
    public RelayCommand AddToolExpectedOutputCommand { get; }
    public RelayCommand RunSelectedToolPairsCommand { get; }
    public RelayCommand ClearToolPairsCommand { get; }
    public RelayCommand RunAllTestsCommand { get; }
    public RelayCommand RunSelectedTestsCommand { get; }

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
        CreateProjectCommand = new RelayCommand(_ => CreateProject());
        NewFavaFileCommand = new RelayCommand(node => NewFile(".fava", node as ProjectNode), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        NewTextFileCommand = new RelayCommand(node => NewFile(".txt", node as ProjectNode), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        DeleteNodeCommand = new RelayCommand(n => DeleteNode(n as ProjectNode), n => n is ProjectNode);
        SaveFileCommand = new RelayCommand(_ => SaveFile());
        RunCurrentCommand = new RelayCommand(_ => RunCurrentFile(), _ => !string.IsNullOrWhiteSpace(_currentFile));
        ToggleDiagnosticsCommand = new RelayCommand(_ => ShowDiagnostics = !ShowDiagnostics);
        OpenSettingsCommand = new RelayCommand(_ =>
        {
            IsToolsViewVisible = false;
            IsSettingsViewVisible = true;
        });
        BackToEditorCommand = new RelayCommand(_ =>
        {
            IsSettingsViewVisible = false;
            IsToolsViewVisible = false;
        });
        BrowseJavaPathCommand = new RelayCommand(_ => BrowseJavaPath());
        BrowseCompilerRootCommand = new RelayCommand(_ => BrowseFolder(v => Settings.CompilerRoot = v, "Compiler Root Folder"));
        BrowseAntlrJarCommand = new RelayCommand(_ => BrowseAntlrJar());
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());

        OpenToolsCommand = new RelayCommand(_ =>
        {
            IsSettingsViewVisible = false;
            IsToolsViewVisible = true;
        });
        AddToolInputFileCommand = new RelayCommand(_ => AddToolInputFile());
        AddToolExpectedOutputCommand = new RelayCommand(_ => AddToolExpectedOutput());
        RunSelectedToolPairsCommand = new RelayCommand(_ => RunSelectedToolPairs());
        ClearToolPairsCommand = new RelayCommand(_ => ToolTestPairs.Clear());

        RunAllTestsCommand = new RelayCommand(_ => RunAllTests());
        RunSelectedTestsCommand = new RelayCommand(_ => RunSelectedTest(), _ => SelectedTestResult != null);

        EnsureProjectDirectory();
        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            LoadProject(Settings.ProjectRoot);
    }

    private void OpenProject()
    {
        var dialog = new OpenFolderDialog { Title = "Select Project Folder" };
        if (dialog.ShowDialog() != true) return;

        if (!string.IsNullOrWhiteSpace(dialog.FolderName) && Directory.Exists(dialog.FolderName))
        {
            Settings.ProjectRoot = dialog.FolderName;
            Settings.Save();
            LoadProject(dialog.FolderName);
            OnPropertyChanged(nameof(CurrentProjectDirectory));
        }
    }

    private void CreateProject()
    {
        var dialog = new OpenFolderDialog { Title = "Select Parent Folder for New Project" };
        if (dialog.ShowDialog() != true) return;

        var baseFolder = dialog.FolderName;
        if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder)) return;

        var projectRoot = Path.Combine(baseFolder, "FavaProject");
        if (Directory.Exists(projectRoot))
        {
            var maxSuffix = Directory.GetDirectories(baseFolder, "FavaProject*")
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name =>
                {
                    if (name == "FavaProject") return 0;
                    var suffixText = name!["FavaProject".Length..];
                    return int.TryParse(suffixText, out var parsed) ? parsed : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            projectRoot = Path.Combine(baseFolder, $"FavaProject{maxSuffix + 1}");
        }

        Directory.CreateDirectory(projectRoot);
        Settings.ProjectRoot = projectRoot;
        Settings.Save();
        LoadProject(projectRoot);
        StatusText = $"Created project: {projectRoot}";
        StatusColor = Brushes.LightBlue;
    }

    private void LoadProject(string folder)
    {
        ProjectTree.Clear();
        try
        {
            var root = BuildNode(folder);
            ProjectTree.Add(root);
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

    private ProjectNode BuildNode(string path)
    {
        var isDirectory = Directory.Exists(path);
        var name = Path.GetFileName(path);
        var node = new ProjectNode
        {
            Name = string.IsNullOrWhiteSpace(name) ? path : name,
            FullPath = path,
            IsDirectory = isDirectory
        };

        if (!isDirectory) return node;

        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
            node.Children.Add(BuildNode(dir));

        foreach (var file in Directory.GetFiles(path).OrderBy(f => f))
            node.Children.Add(BuildNode(file));

        return node;
    }

    private void NewFile(string extension, ProjectNode? node)
    {
        var basePath = Settings.ProjectRoot;
        var targetNode = node ?? SelectedProjectNode;
        if (targetNode is not null)
            basePath = targetNode.IsDirectory
                ? targetNode.FullPath
                : Path.GetDirectoryName(targetNode.FullPath) ?? Settings.ProjectRoot;

        if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            return;

        var dialog = new SaveFileDialog
        {
            InitialDirectory = basePath,
            Filter = extension == ".fava"
                ? "Fava file (*.fava)|*.fava"
                : "Text file (*.txt)|*.txt",
            DefaultExt = extension
        };

        if (dialog.ShowDialog() != true) return;

        FileService.WriteText(dialog.FileName, "");
        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            LoadProject(Settings.ProjectRoot);
    }

    private void DeleteNode(ProjectNode? node)
    {
        if (node is null || string.IsNullOrWhiteSpace(node.FullPath)) return;
        if (node.FullPath == Settings.ProjectRoot) return;

        var answer = MessageBox.Show($"Delete '{node.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;

        if (node.IsDirectory && Directory.Exists(node.FullPath))
            Directory.Delete(node.FullPath, true);
        else if (File.Exists(node.FullPath))
            File.Delete(node.FullPath);

        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            LoadProject(Settings.ProjectRoot);
    }

    private void EnsureProjectDirectory()
    {
        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot) && Directory.Exists(Settings.ProjectRoot))
            return;

        try
        {
            var defaultProjectRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FavaStudio");
            Directory.CreateDirectory(defaultProjectRoot);
            Settings.ProjectRoot = defaultProjectRoot;
            Settings.Save();
            OnPropertyChanged(nameof(CurrentProjectDirectory));
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to prepare project directory: {ex.Message}";
            StatusColor = Brushes.IndianRed;
        }
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
        if (string.IsNullOrWhiteSpace(_currentFile) || _isLiveChecking) return;

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

            UpdateOutputs(result.Output);
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

        UpdateOutputs(result.Output);
        StatusText = result.Success ? "✅ Execution complete" : "❌ Execution failed";
        StatusColor = result.Success ? Brushes.LightGreen : Brushes.IndianRed;
    }

    private static string SliceSection(string output, string[] starts, string[] stops)
    {
        var lines = output.Replace("\r\n", "\n").Split('\n');
        var result = new List<string>();
        var inSection = false;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!inSection && starts.Any(s => trimmed.Equals(s, StringComparison.OrdinalIgnoreCase)))
            {
                inSection = true;
                continue;
            }

            if (inSection && stops.Any(s => trimmed.Equals(s, StringComparison.OrdinalIgnoreCase)))
                break;

            if (inSection) result.Add(line);
        }
        return string.Join("\n", result).Trim();
    }

    private void UpdateOutputs(string fullOutput)
    {
        ConstantPoolOutput = SliceSection(fullOutput,
            ["Constant Pool", "**Constant Pool**", "CONSTANT POOL"],
            ["Instructions", "**Instructions**", "INSTRUCTIONS", "VM Output", "VM OUTPUT"]);

        InstructionsOutput = SliceSection(fullOutput,
            ["Instructions", "**Instructions**", "INSTRUCTIONS"],
            ["VM Output", "VM OUTPUT"]);

        if (ShowOutputOnly)
        {
            VmOutput = ExtractVmOnlyOutput(fullOutput);
        }
        else
        {
            VmOutput = fullOutput;
        }
    }

    private void AddToolInputFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Input Fava File",
            Filter = "Fava Files|*.fava|All Files|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        ToolTestPairs.Add(new TestFilePair
        {
            InputFile = dialog.FileName,
            ExpectedOutputFile = "",
            Result = "Missing output file"
        });
    }

    private void AddToolExpectedOutput()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Expected Output File",
            Filter = "Text Files|*.txt|All Files|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var pair = ToolTestPairs.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.ExpectedOutputFile));
        if (pair is null)
        {
            ToolTestPairs.Add(new TestFilePair
            {
                InputFile = "",
                ExpectedOutputFile = dialog.FileName,
                Result = "Missing input file"
            });
        }
        else
        {
            pair.ExpectedOutputFile = dialog.FileName;
        }
        OnPropertyChanged(nameof(ToolTestPairs));
    }

    private async void RunSelectedToolPairs()
    {
        if (ToolTestPairs.Count == 0)
        {
            StatusText = "No tool test pairs configured.";
            StatusColor = Brushes.Orange;
            return;
        }

        var runner = new JavaCompilerService(Settings);
        foreach (var pair in ToolTestPairs)
        {
            if (string.IsNullOrWhiteSpace(pair.InputFile) || string.IsNullOrWhiteSpace(pair.ExpectedOutputFile))
            {
                pair.Result = "Incomplete pair";
                continue;
            }

            var run = await runner.RunFileAsync(pair.InputFile);
            if (!run.Success)
            {
                pair.Result = "Compiler/runtime error";
                continue;
            }

            var expected = File.Exists(pair.ExpectedOutputFile) ? File.ReadAllText(pair.ExpectedOutputFile).Replace("\r\n", "\n").Trim() : "";
            var actual = ToolCompareFullOutput
                ? run.Output.Replace("\r\n", "\n").Trim()
                : ExtractVmOnlyOutput(run.Output).Replace("\r\n", "\n").Trim();
            pair.Result = actual == expected ? "PASS" : "FAIL";
        }
        StatusText = "Tool tests complete.";
        StatusColor = Brushes.LightBlue;
        OnPropertyChanged(nameof(ToolTestPairs));
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
        TestSummary = passed == total ? $"✅ ALL TESTS PASSED ({passed}/{total})" : $"❌ {passed}/{total} tests passed";
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
        if (idx >= 0) TestResults[idx] = result; else TestResults.Add(result);
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

    public void SetSelectedProjectNode(ProjectNode? node) => SelectedProjectNode = node;

    public void CreateNewFavaAtSelectedNode()
    {
        if (SelectedProjectNode is null) return;
        NewFile(".fava", SelectedProjectNode);
    }

    public void CreateNewTextAtSelectedNode()
    {
        if (SelectedProjectNode is null) return;
        NewFile(".txt", SelectedProjectNode);
    }

    public void DeleteSelectedNode()
    {
        if (SelectedProjectNode is null) return;
        DeleteNode(SelectedProjectNode);
    }

    private string ExtractVmOnlyOutput(string fullOutput)
    {
        var lines = fullOutput.Replace("\r\n", "\n").Split('\n');
        var vmLines = new List<string>();
        var skippingConstant = false;
        var skippingInstructions = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.Equals("Constant Pool", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("**Constant Pool**", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("CONSTANT POOL", StringComparison.OrdinalIgnoreCase))
            {
                skippingConstant = true;
                skippingInstructions = false;
                continue;
            }

            if (trimmed.Equals("Instructions", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("**Instructions**", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("INSTRUCTIONS", StringComparison.OrdinalIgnoreCase))
            {
                skippingInstructions = true;
                skippingConstant = false;
                continue;
            }

            if (trimmed.Equals("VM Output", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("VM OUTPUT", StringComparison.OrdinalIgnoreCase))
            {
                skippingConstant = false;
                skippingInstructions = false;
                continue;
            }

            if (!skippingConstant && !skippingInstructions)
                vmLines.Add(line);
        }

        return string.Join("\n", vmLines).Trim();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

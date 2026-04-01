using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
    private bool _isVisualizerViewVisible;
    private bool _showOutputOnly = true;
    private bool _toolCompareFullOutput;
    private TestFilePair? _selectedToolTestPair;
    private string _toolRunSummary = "No tool runs yet.";
    private string _selectedToolExpectedOutput = "";
    private string _selectedToolActualOutput = "";
    private string _selectedToolDiffOutput = "";
    private TestResult? _selectedTestResult;
    private string _testSummary = "";
    private readonly List<VisualizerInstruction> _allVisualizerInstructions = [];
    private readonly List<string> _allVisualizerConstants = [];
    private readonly List<VisualizerValue> _visualizerRuntimeStack = [];
    private readonly List<OpcodeReferenceItem> _allOpcodeReference = VisualizerService.BuildReference().ToList();
    private int _visualizerStepIndex;
    private bool _visualizerHalted;
    private string _visualizerRunOutput = "";
    private string _visualizerInfo = "Run a file and open Visualizer to inspect stack execution.";
    private string _visualizerInstructionFilter = "";
    private string _visualizerConstantFilter = "";
    private string _visualizerOpcodeSearch = "";
    private bool _visualizerAutoSync = true;

    public ObservableCollection<ProjectNode> ProjectTree { get; } = new();
    public ObservableCollection<FavaDiagnostic> Diagnostics { get; } = new();
    public ObservableCollection<TestResult> TestResults { get; } = new();
    public ObservableCollection<TestFilePair> ToolTestPairs { get; } = new();
    public ObservableCollection<VisualizerInstruction> VisualizerInstructions { get; } = new();
    public ObservableCollection<string> VisualizerConstantPool { get; } = new();
    public ObservableCollection<VisualizerTimelineEntry> VisualizerTimeline { get; } = new();
    public ObservableCollection<VisualizerStackEntry> VisualizerStack { get; } = new();
    public ObservableCollection<OpcodeReferenceItem> VisualizerOpcodeReference { get; } = new();

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
    public bool IsVisualizerViewVisible { get => _isVisualizerViewVisible; set { _isVisualizerViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsWorkspaceVisible => !IsSettingsViewVisible && !IsToolsViewVisible && !IsVisualizerViewVisible;
    public bool ShowOutputOnly { get => _showOutputOnly; set { _showOutputOnly = value; OnPropertyChanged(); } }
    public bool ToolCompareFullOutput { get => _toolCompareFullOutput; set { _toolCompareFullOutput = value; OnPropertyChanged(); } }
    public string ToolInputsFolder { get => Settings.InputsDir; set { Settings.InputsDir = value; Settings.Save(); OnPropertyChanged(); } }
    public string ToolOutputsFolder { get => Settings.OutputsDir; set { Settings.OutputsDir = value; Settings.Save(); OnPropertyChanged(); } }
    public string ToolRunSummary { get => _toolRunSummary; set { _toolRunSummary = value; OnPropertyChanged(); } }
    public string SelectedToolExpectedOutput { get => _selectedToolExpectedOutput; set { _selectedToolExpectedOutput = value; OnPropertyChanged(); } }
    public string SelectedToolActualOutput { get => _selectedToolActualOutput; set { _selectedToolActualOutput = value; OnPropertyChanged(); } }
    public string SelectedToolDiffOutput { get => _selectedToolDiffOutput; set { _selectedToolDiffOutput = value; OnPropertyChanged(); } }
    public string TestSummary { get => _testSummary; set { _testSummary = value; OnPropertyChanged(); } }
    public bool ShowTestOutput { get => Settings.ShowTestOutput; set { Settings.ShowTestOutput = value; OnPropertyChanged(); } }
    public string VisualizerRunOutput { get => _visualizerRunOutput; set { _visualizerRunOutput = value; OnPropertyChanged(); } }
    public string VisualizerInfo { get => _visualizerInfo; set { _visualizerInfo = value; OnPropertyChanged(); } }
    public bool VisualizerAutoSync { get => _visualizerAutoSync; set { _visualizerAutoSync = value; OnPropertyChanged(); } }
    public bool VisualizerCanStep => _allVisualizerInstructions.Count > 0 && !_visualizerHalted && _visualizerStepIndex < _allVisualizerInstructions.Count;
    public bool VisualizerHasData => _allVisualizerInstructions.Count > 0;
    public string VisualizerInstructionFilter
    {
        get => _visualizerInstructionFilter;
        set
        {
            _visualizerInstructionFilter = value;
            OnPropertyChanged();
            ApplyVisualizerInstructionFilter();
        }
    }

    public string VisualizerConstantFilter
    {
        get => _visualizerConstantFilter;
        set
        {
            _visualizerConstantFilter = value;
            OnPropertyChanged();
            ApplyVisualizerConstantFilter();
        }
    }

    public string VisualizerOpcodeSearch
    {
        get => _visualizerOpcodeSearch;
        set
        {
            _visualizerOpcodeSearch = value;
            OnPropertyChanged();
            ApplyVisualizerOpcodeFilter();
        }
    }

    public TestResult? SelectedTestResult
    {
        get => _selectedTestResult;
        set { _selectedTestResult = value; OnPropertyChanged(); }
    }

    public TestFilePair? SelectedToolTestPair
    {
        get => _selectedToolTestPair;
        set
        {
            _selectedToolTestPair = value;
            OnPropertyChanged();
            UpdateSelectedToolPairDetails();
            RemoveToolPairCommand.RaiseCanExecuteChanged();
        }
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
    public RelayCommand NewDirectoryCommand { get; }
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
    public RelayCommand OpenVisualizerCommand { get; }
    public RelayCommand AddToolPairCommand { get; }
    public RelayCommand RemoveToolPairCommand { get; }
    public RelayCommand BrowseToolPairInputCommand { get; }
    public RelayCommand BrowseToolPairExpectedOutputCommand { get; }
    public RelayCommand BrowseToolInputsFolderCommand { get; }
    public RelayCommand BrowseToolOutputsFolderCommand { get; }
    public RelayCommand BuildToolPairsFromFoldersCommand { get; }
    public RelayCommand RunSelectedToolPairsCommand { get; }
    public RelayCommand ClearToolPairsCommand { get; }
    public RelayCommand RunAllTestsCommand { get; }
    public RelayCommand RunSelectedTestsCommand { get; }
    public RelayCommand VisualizerLoadCurrentCommand { get; }
    public RelayCommand VisualizerStepCommand { get; }
    public RelayCommand VisualizerRunAllCommand { get; }
    public RelayCommand VisualizerResetCommand { get; }
    public RelayCommand VisualizerJumpToEndCommand { get; }

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
        NewDirectoryCommand = new RelayCommand(node => NewDirectory(node as ProjectNode), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        DeleteNodeCommand = new RelayCommand(n => DeleteNode(n as ProjectNode), n => n is ProjectNode);
        SaveFileCommand = new RelayCommand(_ => SaveFile());
        RunCurrentCommand = new RelayCommand(_ => RunCurrentFile(), _ => !string.IsNullOrWhiteSpace(_currentFile));
        ToggleDiagnosticsCommand = new RelayCommand(_ => ShowDiagnostics = !ShowDiagnostics);
        OpenSettingsCommand = new RelayCommand(_ =>
        {
            IsToolsViewVisible = false;
            IsVisualizerViewVisible = false;
            IsSettingsViewVisible = true;
        });
        BackToEditorCommand = new RelayCommand(_ =>
        {
            IsSettingsViewVisible = false;
            IsToolsViewVisible = false;
            IsVisualizerViewVisible = false;
        });
        BrowseJavaPathCommand = new RelayCommand(_ => BrowseJavaPath());
        BrowseCompilerRootCommand = new RelayCommand(_ => BrowseFolder(v => Settings.CompilerRoot = v, "Compiler Root Folder"));
        BrowseAntlrJarCommand = new RelayCommand(_ => BrowseAntlrJar());
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());

        OpenToolsCommand = new RelayCommand(_ =>
        {
            IsSettingsViewVisible = false;
            IsVisualizerViewVisible = false;
            IsToolsViewVisible = true;
        });
        OpenVisualizerCommand = new RelayCommand(_ =>
        {
            IsSettingsViewVisible = false;
            IsToolsViewVisible = false;
            IsVisualizerViewVisible = true;
            EnsureVisualizerDataLoaded();
        });
        AddToolPairCommand = new RelayCommand(_ => AddToolPair());
        RemoveToolPairCommand = new RelayCommand(_ => RemoveSelectedToolPair(), _ => SelectedToolTestPair != null);
        BrowseToolPairInputCommand = new RelayCommand(p => BrowseToolPairInputFile(p as TestFilePair));
        BrowseToolPairExpectedOutputCommand = new RelayCommand(p => BrowseToolPairExpectedOutputFile(p as TestFilePair));
        BrowseToolInputsFolderCommand = new RelayCommand(_ => BrowseFolder(v => ToolInputsFolder = v, "Inputs Folder"));
        BrowseToolOutputsFolderCommand = new RelayCommand(_ => BrowseFolder(v => ToolOutputsFolder = v, "Outputs Folder"));
        BuildToolPairsFromFoldersCommand = new RelayCommand(_ => BuildToolPairsFromFolders());
        RunSelectedToolPairsCommand = new RelayCommand(_ => RunSelectedToolPairs());
        ClearToolPairsCommand = new RelayCommand(_ =>
        {
            ToolTestPairs.Clear();
            SelectedToolTestPair = null;
            ToolRunSummary = "Tool pairs cleared.";
        });

        RunAllTestsCommand = new RelayCommand(_ => RunAllTests());
        RunSelectedTestsCommand = new RelayCommand(_ => RunSelectedTest(), _ => SelectedTestResult != null);
        VisualizerLoadCurrentCommand = new RelayCommand(_ => LoadVisualizerFromCurrentFile());
        VisualizerStepCommand = new RelayCommand(_ => VisualizerStep(), _ => VisualizerCanStep);
        VisualizerRunAllCommand = new RelayCommand(_ => VisualizerRunAll(), _ => VisualizerCanStep);
        VisualizerResetCommand = new RelayCommand(_ => VisualizerReset(), _ => VisualizerHasData);
        VisualizerJumpToEndCommand = new RelayCommand(_ => VisualizerRunAll(), _ => VisualizerCanStep);

        EnsureProjectDirectory();
        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            LoadProject(Settings.ProjectRoot);
        ApplyVisualizerOpcodeFilter();
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

    private void NewDirectory(ProjectNode? node)
    {
        var basePath = Settings.ProjectRoot;
        var targetNode = node ?? SelectedProjectNode;
        if (targetNode is not null)
            basePath = targetNode.IsDirectory
                ? targetNode.FullPath
                : Path.GetDirectoryName(targetNode.FullPath) ?? Settings.ProjectRoot;

        if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            return;

        var folderName = "NewFolder";
        var candidate = Path.Combine(basePath, folderName);
        var suffix = 1;
        while (Directory.Exists(candidate))
        {
            suffix++;
            candidate = Path.Combine(basePath, $"{folderName}{suffix}");
        }

        Directory.CreateDirectory(candidate);
        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            LoadProject(Settings.ProjectRoot);
        StatusText = $"Created directory: {Path.GetFileName(candidate)}";
        StatusColor = Brushes.LightGreen;
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
        var startSet = starts.Select(NormalizeHeader).ToHashSet();
        var stopSet = stops.Select(NormalizeHeader).ToHashSet();
        var lines = output.Replace("\r\n", "\n").Split('\n');
        var result = new List<string>();
        var inSection = false;
        foreach (var line in lines)
        {
            var normalized = NormalizeHeader(line);
            if (!inSection && startSet.Contains(normalized))
            {
                inSection = true;
                continue;
            }

            if (inSection && stopSet.Contains(normalized))
                break;

            if (inSection) result.Add(line);
        }
        return string.Join("\n", result).Trim();
    }

    private void UpdateOutputs(string fullOutput)
    {
        ConstantPoolOutput = SliceSection(fullOutput,
            ["constant pool"],
            ["instructions", "vm output"]);

        InstructionsOutput = SliceSection(fullOutput,
            ["instructions"],
            ["vm output"]);

        if (ShowOutputOnly)
        {
            VmOutput = ExtractVmOnlyOutput(fullOutput);
        }
        else
        {
            VmOutput = fullOutput;
        }

        if (VisualizerAutoSync)
            LoadVisualizerFromSections(ConstantPoolOutput, InstructionsOutput);
    }

    private async void LoadVisualizerFromCurrentFile()
    {
        if (string.IsNullOrWhiteSpace(_currentFile))
        {
            VisualizerInfo = "Open a .fava file first.";
            return;
        }

        SaveFile();
        var runner = new JavaCompilerService(Settings);
        var result = await runner.RunFileAsync(_currentFile);
        UpdateOutputs(result.Output);
        LoadVisualizerFromSections(ConstantPoolOutput, InstructionsOutput);
        StatusText = result.Success ? "Visualizer data loaded." : "Visualizer loaded with execution errors.";
        StatusColor = result.Success ? Brushes.LightGreen : Brushes.Orange;
    }

    private void EnsureVisualizerDataLoaded()
    {
        if (_allVisualizerInstructions.Count > 0) return;
        if (string.IsNullOrWhiteSpace(InstructionsOutput))
            VisualizerInfo = "Run the current file or click “Load Current File” in Visualizer.";
        else
            LoadVisualizerFromSections(ConstantPoolOutput, InstructionsOutput);
    }

    private void LoadVisualizerFromSections(string constantsSection, string instructionsSection)
    {
        _allVisualizerConstants.Clear();
        _allVisualizerConstants.AddRange(VisualizerService.ParseConstantPool(constantsSection));
        _allVisualizerInstructions.Clear();
        _allVisualizerInstructions.AddRange(VisualizerService.ParseInstructions(instructionsSection));

        VisualizerReset();
        ApplyVisualizerInstructionFilter();
        ApplyVisualizerConstantFilter();
        VisualizerInfo = _allVisualizerInstructions.Count == 0
            ? "No instructions found. Run a valid file to visualize execution."
            : $"Loaded {_allVisualizerInstructions.Count} instruction(s) and {_allVisualizerConstants.Count} constant(s).";
    }

    private void VisualizerReset()
    {
        _visualizerRuntimeStack.Clear();
        VisualizerStack.Clear();
        VisualizerTimeline.Clear();
        _visualizerStepIndex = 0;
        _visualizerHalted = false;
        VisualizerRunOutput = "";
        foreach (var instruction in _allVisualizerInstructions)
            instruction.IsCurrent = false;

        if (_allVisualizerInstructions.Count > 0)
            _allVisualizerInstructions[0].IsCurrent = true;

        RaiseVisualizerStateChanged();
    }

    private void VisualizerStep()
    {
        if (!VisualizerCanStep) return;
        var instruction = _allVisualizerInstructions[_visualizerStepIndex];
        instruction.IsCurrent = true;
        var before = VisualizerService.StackToText(_visualizerRuntimeStack);
        var success = VisualizerService.ApplyInstruction(instruction, _visualizerRuntimeStack, _allVisualizerConstants, out var note, out var outputLine, out var halted);
        var after = VisualizerService.StackToText(_visualizerRuntimeStack);
        VisualizerTimeline.Add(new VisualizerTimelineEntry
        {
            Step = _visualizerStepIndex + 1,
            Instruction = instruction.Display,
            StackBefore = before,
            StackAfter = after,
            Note = note
        });

        if (!string.IsNullOrWhiteSpace(outputLine))
            VisualizerRunOutput = string.IsNullOrWhiteSpace(VisualizerRunOutput) ? outputLine : $"{VisualizerRunOutput}\n{outputLine}";

        RefreshVisualizerStack();
        instruction.IsCurrent = false;
        if (!success || halted)
            _visualizerHalted = true;

        _visualizerStepIndex++;
        if (!_visualizerHalted && _visualizerStepIndex < _allVisualizerInstructions.Count)
            _allVisualizerInstructions[_visualizerStepIndex].IsCurrent = true;

        RaiseVisualizerStateChanged();
    }

    private void VisualizerRunAll()
    {
        while (VisualizerCanStep)
            VisualizerStep();
    }

    private void RefreshVisualizerStack()
    {
        VisualizerStack.Clear();
        foreach (var entry in VisualizerService.StackToEntries(_visualizerRuntimeStack))
            VisualizerStack.Add(entry);
    }

    private void RaiseVisualizerStateChanged()
    {
        OnPropertyChanged(nameof(VisualizerCanStep));
        OnPropertyChanged(nameof(VisualizerHasData));
        VisualizerStepCommand.RaiseCanExecuteChanged();
        VisualizerRunAllCommand.RaiseCanExecuteChanged();
        VisualizerResetCommand.RaiseCanExecuteChanged();
        VisualizerJumpToEndCommand.RaiseCanExecuteChanged();
    }

    private void ApplyVisualizerInstructionFilter()
    {
        VisualizerInstructions.Clear();
        var filter = (VisualizerInstructionFilter ?? "").Trim();
        var source = _allVisualizerInstructions.Where(i =>
            string.IsNullOrWhiteSpace(filter) ||
            i.Opcode.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            i.Display.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            i.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));

        foreach (var instruction in source)
            VisualizerInstructions.Add(instruction);
    }

    private void ApplyVisualizerConstantFilter()
    {
        VisualizerConstantPool.Clear();
        var filter = (VisualizerConstantFilter ?? "").Trim();
        for (var i = 0; i < _allVisualizerConstants.Count; i++)
        {
            var value = _allVisualizerConstants[i];
            var line = $"{i}: {value}";
            if (!string.IsNullOrWhiteSpace(filter) && !line.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;
            VisualizerConstantPool.Add(line);
        }
    }

    private void ApplyVisualizerOpcodeFilter()
    {
        VisualizerOpcodeReference.Clear();
        var filter = (VisualizerOpcodeSearch ?? "").Trim();
        var source = _allOpcodeReference.Where(item =>
            string.IsNullOrWhiteSpace(filter)
            || item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || item.Summary.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || item.Opcode.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase));
        foreach (var item in source)
            VisualizerOpcodeReference.Add(item);
    }

    private void AddToolPair()
    {
        var pair = new TestFilePair();
        ToolTestPairs.Add(pair);
        SelectedToolTestPair = pair;
    }

    private void RemoveSelectedToolPair()
    {
        if (SelectedToolTestPair is null) return;

        ToolTestPairs.Remove(SelectedToolTestPair);
        SelectedToolTestPair = ToolTestPairs.FirstOrDefault();
    }

    private void BrowseToolPairInputFile(TestFilePair? pair)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Input Fava File",
            Filter = "Fava Files|*.fava|All Files|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var target = pair ?? SelectedToolTestPair;
        if (target is null) return;
        target.InputFile = dialog.FileName;
        target.Result = "Ready";
        SetSelectedToolPairIfChanged(target);
    }

    private void BrowseToolPairExpectedOutputFile(TestFilePair? pair)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Expected Output File",
            Filter = "Text Files|*.txt|All Files|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var target = pair ?? SelectedToolTestPair;
        if (target is null) return;
        target.ExpectedOutputFile = dialog.FileName;
        target.Result = "Ready";
        SetSelectedToolPairIfChanged(target);
    }

    private void BuildToolPairsFromFolders()
    {
        if (string.IsNullOrWhiteSpace(ToolInputsFolder) || !Directory.Exists(ToolInputsFolder))
        {
            StatusText = "Select a valid inputs folder.";
            StatusColor = Brushes.Orange;
            return;
        }

        if (string.IsNullOrWhiteSpace(ToolOutputsFolder) || !Directory.Exists(ToolOutputsFolder))
        {
            StatusText = "Select a valid outputs folder.";
            StatusColor = Brushes.Orange;
            return;
        }

        ToolTestPairs.Clear();
        foreach (var input in Directory.GetFiles(ToolInputsFolder, "*.fava").OrderBy(f => f))
        {
            var name = Path.GetFileNameWithoutExtension(input);
            var expected = Path.Combine(ToolOutputsFolder, $"{name}.txt");
            ToolTestPairs.Add(new TestFilePair
            {
                InputFile = input,
                ExpectedOutputFile = expected,
                Result = File.Exists(expected) ? "Ready" : "Missing expected file"
            });
        }

        SelectedToolTestPair = ToolTestPairs.FirstOrDefault();
        ToolRunSummary = $"Loaded {ToolTestPairs.Count} pair(s) from folders.";
        Settings.Save();
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
        var passed = 0;
        var failed = 0;
        var skipped = 0;
        foreach (var pair in ToolTestPairs)
        {
            pair.ActualOutput = "";
            pair.ExpectedOutput = "";
            pair.DiffOutput = "";

            if (string.IsNullOrWhiteSpace(pair.InputFile) || !File.Exists(pair.InputFile))
            {
                pair.Result = "Missing input file";
                skipped++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(pair.ExpectedOutputFile) || !File.Exists(pair.ExpectedOutputFile))
            {
                pair.Result = "Missing expected file";
                skipped++;
                continue;
            }

            var run = await runner.RunFileAsync(pair.InputFile);
            if (!run.Success)
            {
                pair.Result = "Compiler/runtime error";
                pair.ActualOutput = run.Output;
                pair.ExpectedOutput = File.ReadAllText(pair.ExpectedOutputFile);
                pair.DiffOutput = "Execution failed before comparison.";
                failed++;
                continue;
            }

            var expected = File.ReadAllText(pair.ExpectedOutputFile).Replace("\r\n", "\n").Trim();
            var actual = ToolCompareFullOutput
                ? run.Output.Replace("\r\n", "\n").Trim()
                : ExtractVmOnlyOutput(run.Output).Replace("\r\n", "\n").Trim();
            pair.ExpectedOutput = expected;
            pair.ActualOutput = actual;
            pair.DiffOutput = BuildDiff(expected, actual);
            if (actual == expected)
            {
                pair.Result = "PASS";
                passed++;
            }
            else
            {
                pair.Result = "FAIL";
                failed++;
            }
        }

        ToolRunSummary = FormatToolRunSummary(passed, failed, skipped, ToolTestPairs.Count);
        StatusText = ToolRunSummary;
        StatusColor = failed == 0 && skipped == 0 ? Brushes.LightGreen : (failed > 0 ? Brushes.IndianRed : Brushes.Orange);
        UpdateSelectedToolPairDetails();
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
            var normalized = NormalizeHeader(line);

            if (normalized == "constant pool")
            {
                skippingConstant = true;
                skippingInstructions = false;
                continue;
            }

            if (normalized == "instructions")
            {
                skippingInstructions = true;
                skippingConstant = false;
                continue;
            }

            if (normalized == "vm output")
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

    private static string NormalizeHeader(string line)
    {
        var normalized = line.Trim().Trim('*').Trim();
        if (normalized.EndsWith(':'))
            normalized = normalized[..^1];
        return normalized.ToLowerInvariant();
    }

    private void UpdateSelectedToolPairDetails()
    {
        SelectedToolExpectedOutput = SelectedToolTestPair?.ExpectedOutput ?? "";
        SelectedToolActualOutput = SelectedToolTestPair?.ActualOutput ?? "";
        SelectedToolDiffOutput = SelectedToolTestPair?.DiffOutput ?? "";
    }

    private void SetSelectedToolPairIfChanged(TestFilePair? pair)
    {
        if (!ReferenceEquals(SelectedToolTestPair, pair))
            SelectedToolTestPair = pair;
    }

    private static string FormatToolRunSummary(int passed, int failed, int skipped, int total) =>
        string.Join(" | ", $"Passed: {passed}", $"Failed: {failed}", $"Skipped: {skipped}", $"Total: {total}");

    private static string BuildDiff(string expected, string actual)
    {
        if (expected == actual)
            return "No differences.";

        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');
        var max = Math.Max(expectedLines.Length, actualLines.Length);
        var diff = new StringBuilder();
        var shown = 0;
        const int maxDiffLines = 200;

        for (var i = 0; i < max; i++)
        {
            var exp = i < expectedLines.Length ? expectedLines[i] : "";
            var act = i < actualLines.Length ? actualLines[i] : "";
            if (exp == act) continue;

            if (shown >= maxDiffLines)
            {
                diff.AppendLine($"... diff truncated after {maxDiffLines} differing lines.");
                break;
            }

            diff.AppendLine($"Line {i + 1}:");
            diff.AppendLine($"  - expected: {exp}");
            diff.AppendLine($"  + actual:   {act}");
            shown++;
        }

        return diff.ToString().Trim();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

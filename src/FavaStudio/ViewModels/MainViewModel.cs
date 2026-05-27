using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    private bool _suppressDirtyTracking;
    private bool _hasUnsavedChanges;
    private Brush _statusColor = Brushes.LightGray;
    private string _statusText = "Ready.";
    private string _vmOutput = "";
    private string _constantPoolOutput = "";
    private string _instructionsOutput = "";
    private string _typeInfoOutput = "";
    private string _sourceMapOutput = "";
    private string _lastFullOutput = "";
    private string _lastRunStatus = "No run yet";
    private string _lastRunDurationText = "--";
    private Brush _lastRunStatusBrush = Brushes.Gray;
    private bool _showDiagnostics = true;
    private bool _isSettingsViewVisible;
    private bool _isToolsViewVisible;
    private bool _isVisualizerViewVisible;
    private bool _isWelcomeViewVisible;
    private bool _isQuickOpenVisible;
    private string _quickOpenQuery = "";
    private string? _selectedQuickOpenFile;
    private bool _showOutputOnly = true;
    private bool _toolCompareFullOutput;
    private TestFilePair? _selectedToolTestPair;
    private EditorTab? _selectedEditorTab;
    private readonly HashSet<string> _currentTestTabPaths = new(StringComparer.OrdinalIgnoreCase);
    private string _toolRunSummary = "No tool runs yet.";
    private string _selectedToolExpectedOutput = "";
    private string _selectedToolActualOutput = "";
    private string _selectedToolDiffOutput = "";
    private TestResult? _selectedTestResult;
    private bool _suppressTestSelectionOpen;
    private string _testSummary = "No tests run yet.";
    private readonly List<VisualizerInstruction> _allVisualizerInstructions = [];
    private IReadOnlyList<FavaHoverInfo> _hoverInfos = [];
    private readonly List<string> _allVisualizerConstants = [];
    private readonly List<VisualizerValue> _visualizerRuntimeStack = [];
    private readonly List<VisualizerValue?> _visualizerGlobals = [];
    private readonly List<VisualizerFrameState> _visualizerFrames = [];
    private int _visualizerFramePointer = -1;
    private readonly List<OpcodeReferenceItem> _allOpcodeReference = VisualizerService.BuildReference().ToList();
    private int _visualizerStepIndex;
    private bool _visualizerHalted;
    private string _visualizerRunOutput = "";
    private string _visualizerInfo = "Run a file and open Visualizer to inspect stack execution.";
    private string _visualizerInstructionFilter = "";
    private string _visualizerConstantFilter = "";
    private string _visualizerOpcodeSearch = "";
    private bool _visualizerAutoSync = true;
    private const int MaxVisualizerTimelineEntries = 500;

    // ── Debug mode ──────────────────────────────────────────────────────────
    private readonly HashSet<int> _breakpointLines = [];
    private bool _isDebugging;
    private string _debugCurrentInstruction = "";
    private string _debugCurrentNote = "";
    private string _debugStepStatus = "";
    private int? _debugCurrentSourceLine;
    private readonly List<DebugSnapshot> _debugHistory = [];
    private readonly Dictionary<int, List<int>> _sourceLineToInstructionPositions = [];
    private readonly Dictionary<int, int> _instructionPositionToSourceLine = [];
    private readonly List<int> _debugTraceInstructionPositions = [];
    private string _vmTraceOutput = "";

    public ObservableCollection<ProjectNode> ProjectTree { get; } = new();
    public ObservableCollection<EditorTab> OpenEditorTabs { get; } = new();
    public ObservableCollection<FavaDiagnostic> Diagnostics { get; } = new();
    public ObservableCollection<TestResult> TestResults { get; } = new();
    public ObservableCollection<TestFilePair> ToolTestPairs { get; } = new();
    public ObservableCollection<VisualizerInstruction> VisualizerInstructions { get; } = new();
    public ObservableCollection<string> VisualizerConstantPool { get; } = new();
    public ObservableCollection<VisualizerTimelineEntry> VisualizerTimeline { get; } = new();
    public ObservableCollection<VisualizerStackEntry> VisualizerStack { get; } = new();
    public ObservableCollection<VisualizerGlobalEntry> VisualizerGlobals { get; } = new();
    public ObservableCollection<OpcodeReferenceItem> VisualizerOpcodeReference { get; } = new();
    public ObservableCollection<FavaOutlineItem> OutlineItems { get; } = new();
    public ObservableCollection<FavaReferenceItem> ReferenceResults { get; } = new();
    public ObservableCollection<InlineValueHint> DebugInlineValueHints { get; } = new();
    public ObservableCollection<string> RecentProjects { get; } = new();
    public ObservableCollection<RecentProjectItem> WelcomeRecentProjects { get; } = new();
    public ObservableCollection<string> RecentFiles { get; } = new();
    public ObservableCollection<string> QuickOpenResults { get; } = new();
    public ObservableCollection<DebugStackEntry> DebugStack { get; } = new();

    public SettingsService Settings { get; } = SettingsService.Load();

    public string FooterText => "Fava Studio • built for your Fava compiler";
    public string CurrentFileName
    {
        get
        {
            if (_selectedEditorTab is null || string.IsNullOrWhiteSpace(_selectedEditorTab.FilePath))
                return "No file open";
            return _selectedEditorTab.Header;
        }
    }
    public EditorTab? SelectedEditorTab
    {
        get => _selectedEditorTab;
        set => SelectEditorTab(value);
    }
    public string CurrentProjectDirectory => string.IsNullOrWhiteSpace(Settings.ProjectRoot) ? "Project directory: (not set)" : Settings.ProjectRoot;
    public string SettingsProjectName => string.IsNullOrWhiteSpace(Settings.ProjectRoot)
        ? "No project loaded"
        : Path.GetFileName(Settings.ProjectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    public string SettingsProjectPath => string.IsNullOrWhiteSpace(Settings.ProjectRoot) ? "Open a project to enable project-local workflows." : Settings.ProjectRoot;
    public string JavaPath
    {
        get => Settings.JavaPath;
        set
        {
            if (Settings.JavaPath == value) return;
            Settings.JavaPath = value;
            OnPropertyChanged();
            RaiseSettingsValidationChanged();
        }
    }
    public string CompilerRoot
    {
        get => Settings.CompilerRoot;
        set
        {
            if (Settings.CompilerRoot == value) return;
            Settings.CompilerRoot = value;
            OnPropertyChanged();
            RaiseSettingsValidationChanged();
        }
    }
    public string AntlrJar
    {
        get => Settings.AntlrJar;
        set
        {
            if (Settings.AntlrJar == value) return;
            Settings.AntlrJar = value;
            OnPropertyChanged();
            RaiseSettingsValidationChanged();
        }
    }
    public string CompilerStatusText => IsCompilerConfigured ? "Ready to run" : "Needs configuration";
    public Brush CompilerStatusBrush => IsCompilerConfigured ? Brushes.LightGreen : Brushes.Orange;
    public string JavaStatusText => string.IsNullOrWhiteSpace(Settings.JavaPath) ? "Required" : "Configured";
    public Brush JavaStatusBrush => string.IsNullOrWhiteSpace(Settings.JavaPath) ? Brushes.Orange : Brushes.LightGreen;
    public string CompilerRootStatusText => Directory.Exists(Settings.CompilerRoot) ? "Folder found" : "Missing folder";
    public Brush CompilerRootStatusBrush => Directory.Exists(Settings.CompilerRoot) ? Brushes.LightGreen : Brushes.Orange;
    public string AntlrStatusText => File.Exists(Settings.AntlrJar) ? "Jar found" : "Missing jar";
    public Brush AntlrStatusBrush => File.Exists(Settings.AntlrJar) ? Brushes.LightGreen : Brushes.Orange;
    public bool IsCompilerConfigured =>
        !string.IsNullOrWhiteSpace(Settings.JavaPath) &&
        Directory.Exists(Settings.CompilerRoot) &&
        File.Exists(Settings.AntlrJar);
    public string TestFoldersStatusText => Directory.Exists(Settings.InputsDir) && !string.IsNullOrWhiteSpace(Settings.OutputsDir)
        ? "Test folders configured"
        : "Test folders need setup";
    public Brush TestFoldersStatusBrush => Directory.Exists(Settings.InputsDir) && !string.IsNullOrWhiteSpace(Settings.OutputsDir)
        ? Brushes.LightGreen
        : Brushes.Orange;
    public string RecentSummary => $"{RecentProjects.Count} projects | {RecentFiles.Count} files";
    public string DiagnosticsHeader => Diagnostics.Count == 0 ? "Diagnostics" : $"Diagnostics ({Diagnostics.Count})";
    public string ReferenceResultsHeader => ReferenceResults.Count == 0 ? "References" : $"References ({ReferenceResults.Count})";
    public bool HasReferenceResults => ReferenceResults.Count > 0;

    public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }
    public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }
    public string VmOutput { get => _vmOutput; set { _vmOutput = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConsoleLineCountText)); } }
    public string ConstantPoolOutput { get => _constantPoolOutput; set { _constantPoolOutput = value; OnPropertyChanged(); } }
    public string InstructionsOutput { get => _instructionsOutput; set { _instructionsOutput = value; OnPropertyChanged(); } }
    public IReadOnlyList<FavaHoverInfo> HoverInfos => _hoverInfos;
    public bool ShowDiagnostics
    {
        get => _showDiagnostics;
        set
        {
            _showDiagnostics = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsRightPanelVisible));
            OnPropertyChanged(nameof(ShowDiagnosticsPanel));
        }
    }
    public bool IsSettingsViewVisible { get => _isSettingsViewVisible; set { _isSettingsViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsToolsViewVisible { get => _isToolsViewVisible; set { _isToolsViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsVisualizerViewVisible { get => _isVisualizerViewVisible; set { _isVisualizerViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsWelcomeViewVisible { get => _isWelcomeViewVisible; set { _isWelcomeViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsWorkspaceVisible => !IsSettingsViewVisible && !IsToolsViewVisible && !IsVisualizerViewVisible && !IsWelcomeViewVisible;
    public bool IsQuickOpenVisible { get => _isQuickOpenVisible; set { _isQuickOpenVisible = value; OnPropertyChanged(); } }
    public string QuickOpenQuery
    {
        get => _quickOpenQuery;
        set
        {
            _quickOpenQuery = value;
            OnPropertyChanged();
            ApplyQuickOpenFilter();
        }
    }
    public string? SelectedQuickOpenFile
    {
        get => _selectedQuickOpenFile;
        set
        {
            _selectedQuickOpenFile = value;
            OnPropertyChanged();
            OpenSelectedQuickOpenFileCommand.RaiseCanExecuteChanged();
        }
    }
    public bool ShowOutputOnly
    {
        get => _showOutputOnly;
        set
        {
            _showOutputOnly = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OutputHeaderTitle));
            if (!string.IsNullOrWhiteSpace(_lastFullOutput))
                UpdateOutputs(_lastFullOutput);
        }
    }
    public bool ToolCompareFullOutput { get => _toolCompareFullOutput; set { _toolCompareFullOutput = value; OnPropertyChanged(); } }
    public string ToolInputsFolder { get => Settings.InputsDir; set { Settings.InputsDir = value; Settings.Save(); OnPropertyChanged(); RaiseSettingsValidationChanged(); } }
    public string ToolOutputsFolder { get => Settings.OutputsDir; set { Settings.OutputsDir = value; Settings.Save(); OnPropertyChanged(); RaiseSettingsValidationChanged(); } }
    public string ToolRunSummary { get => _toolRunSummary; set { _toolRunSummary = value; OnPropertyChanged(); } }
    public string SelectedToolExpectedOutput { get => _selectedToolExpectedOutput; set { _selectedToolExpectedOutput = value; OnPropertyChanged(); } }
    public string SelectedToolActualOutput { get => _selectedToolActualOutput; set { _selectedToolActualOutput = value; OnPropertyChanged(); } }
    public string SelectedToolDiffOutput { get => _selectedToolDiffOutput; set { _selectedToolDiffOutput = value; OnPropertyChanged(); } }
    public string TestSummary { get => _testSummary; set { _testSummary = value; OnPropertyChanged(); } }
    public int TestTotalCount => TestResults.Count;
    public int TestRunCount => TestResults.Count(test => test.HasRun);
    public int TestPassedCount => TestResults.Count(test => test.HasRun && test.Passed);
    public int TestFailedCount => TestResults.Count(test => test.HasRun && !test.Passed);
    public double TestPassPercent
    {
        get => TestRunCount == 0 ? 0 : (double)TestPassedCount / TestRunCount * 100;
        set { }
    }
    public string TestPassPercentText => TestRunCount == 0 ? "No runs" : $"{TestPassPercent:0}% pass";
    public string SelectedTestExpectedOutput
    {
        get => SelectedTestResult?.ExpectedOutput ?? "";
        set { }
    }
    public string SelectedTestActualOutput
    {
        get => SelectedTestResult?.ActualOutput ?? "";
        set { }
    }
    public string SelectedTestDiffOutput
    {
        get => SelectedTestResult?.DiffOutput ?? "";
        set { }
    }
    public string SelectedTestDurationText => SelectedTestResult?.DurationText ?? "--";
    public bool ShowTestOutput { get => Settings.ShowTestOutput; set { Settings.ShowTestOutput = value; OnPropertyChanged(); } }
    public string LastRunStatus { get => _lastRunStatus; set { _lastRunStatus = value; OnPropertyChanged(); } }
    public string LastRunDurationText { get => _lastRunDurationText; set { _lastRunDurationText = value; OnPropertyChanged(); } }
    public Brush LastRunStatusBrush { get => _lastRunStatusBrush; set { _lastRunStatusBrush = value; OnPropertyChanged(); } }
    public string OutputHeaderTitle => ShowOutputOnly ? "Program Output" : "Full Compiler Output";
    public string ConsoleLineCountText => $"{CountOutputLines(VmOutput)} lines";
    public string VisualizerRunOutput { get => _visualizerRunOutput; set { _visualizerRunOutput = value; OnPropertyChanged(); } }
    public string VisualizerInfo { get => _visualizerInfo; set { _visualizerInfo = value; OnPropertyChanged(); } }
    public bool VisualizerAutoSync { get => _visualizerAutoSync; set { _visualizerAutoSync = value; OnPropertyChanged(); } }
    public bool VisualizerCanStep => _allVisualizerInstructions.Count > 0 && !_visualizerHalted && _visualizerStepIndex < _allVisualizerInstructions.Count;
    public bool VisualizerHasData => _allVisualizerInstructions.Count > 0;

    // ── Debug mode properties ────────────────────────────────────────────────
    public bool IsDebugging
    {
        get => _isDebugging;
        private set
        {
            _isDebugging = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsRightPanelVisible));
            OnPropertyChanged(nameof(ShowDiagnosticsPanel));
        }
    }

    public bool IsRightPanelVisible => _showDiagnostics || _isDebugging;
    public bool ShowDiagnosticsPanel => _showDiagnostics && !_isDebugging;

    public string DebugCurrentInstruction
    {
        get => _debugCurrentInstruction;
        private set { _debugCurrentInstruction = value; OnPropertyChanged(); }
    }

    public string DebugCurrentNote
    {
        get => _debugCurrentNote;
        private set { _debugCurrentNote = value; OnPropertyChanged(); }
    }

    public string DebugStepStatus
    {
        get => _debugStepStatus;
        private set { _debugStepStatus = value; OnPropertyChanged(); }
    }

    public int? DebugCurrentSourceLine
    {
        get => _debugCurrentSourceLine;
        private set { _debugCurrentSourceLine = value; OnPropertyChanged(); }
    }

    public bool CanDebugStep => _isDebugging && VisualizerCanStep;
    public bool CanDebugBack => _isDebugging && _debugHistory.Count > 0;
    public bool CanDebugContinue => _isDebugging && VisualizerCanStep;
    public bool CanDebugJumpToCall => _isDebugging && VisualizerCanStep;
    public bool CanDebugStop => _isDebugging;
    public IReadOnlySet<int> BreakpointLines => _breakpointLines;
    public bool HasBreakpoints => _breakpointLines.Count > 0;
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
        set
        {
            _selectedTestResult = value;
            OnPropertyChanged();
            RunSelectedTestsCommand.RaiseCanExecuteChanged();
            OpenSelectedTestInputCommand.RaiseCanExecuteChanged();
            OpenSelectedTestExpectedOutputCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(SelectedTestExpectedOutput));
            OnPropertyChanged(nameof(SelectedTestActualOutput));
            OnPropertyChanged(nameof(SelectedTestDiffOutput));
            OnPropertyChanged(nameof(SelectedTestDurationText));
            if (!_suppressTestSelectionOpen && _selectedTestResult is not null)
                OpenSelectedTestFilesInTabs(_selectedTestResult);
        }
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
            if (ReferenceEquals(_selectedProjectNode, value))
                return;

            _selectedProjectNode = value;
            OnPropertyChanged();
            if (_selectedProjectNode is not null && !_selectedProjectNode.IsDirectory)
            {
                OpenFileInEditorTab(_selectedProjectNode.FullPath, focus: true);
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
    public RelayCommand CopyOutputCommand { get; }
    public RelayCommand ClearOutputCommand { get; }
    public RelayCommand ToggleDiagnosticsCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }
    public RelayCommand BackToEditorCommand { get; }
    public RelayCommand BrowseJavaPathCommand { get; }
    public RelayCommand BrowseCompilerRootCommand { get; }
    public RelayCommand BrowseAntlrJarCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand ClearRecentProjectsCommand { get; }
    public RelayCommand ClearRecentFilesCommand { get; }
    public RelayCommand OpenToolsCommand { get; }
    public RelayCommand OpenVisualizerCommand { get; }
    public RelayCommand OpenRecentProjectCommand { get; }
    public RelayCommand OpenRecentFileCommand { get; }
    public RelayCommand OpenQuickOpenCommand { get; }
    public RelayCommand CloseQuickOpenCommand { get; }
    public RelayCommand OpenSelectedQuickOpenFileCommand { get; }
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
    public RelayCommand CreateTestPairCommand { get; }
    public RelayCommand OpenSelectedTestInputCommand { get; }
    public RelayCommand OpenSelectedTestExpectedOutputCommand { get; }
    public RelayCommand CloseEditorTabCommand { get; }
    public RelayCommand VisualizerLoadCurrentCommand { get; }
    public RelayCommand VisualizerStepCommand { get; }
    public RelayCommand VisualizerRunAllCommand { get; }
    public RelayCommand VisualizerResetCommand { get; }
    public RelayCommand VisualizerJumpToEndCommand { get; }

    // ── Debug mode commands ──────────────────────────────────────────────────
    public RelayCommand StartDebugCommand { get; }
    public RelayCommand DebugStepCommand { get; }
    public RelayCommand DebugContinueCommand { get; }
    public RelayCommand DebugJumpToCallCommand { get; }
    public RelayCommand DebugStepBackCommand { get; }
    public RelayCommand DebugStopCommand { get; }
    public RelayCommand ToggleBreakpointAtCaretCommand { get; }

    public MainViewModel(TextEditor editor)
    {
        _editor = editor;
        ReferenceResults.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ReferenceResultsHeader));
            OnPropertyChanged(nameof(HasReferenceResults));
        };

        _liveCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
        _liveCheckTimer.Tick += async (_, _) =>
        {
            _liveCheckTimer.Stop();
            await RunLiveCheckAsync();
        };
        _editor.TextChanged += (_, _) =>
        {
            if (!_suppressDirtyTracking && _selectedEditorTab is not null)
            {
                _selectedEditorTab.Content = _editor.Text;
                _selectedEditorTab.IsDirty = true;
                _currentFile = _selectedEditorTab.FilePath;
                _hasUnsavedChanges = true;
                OnPropertyChanged(nameof(CurrentFileName));
            }
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
        CopyOutputCommand = new RelayCommand(_ => CopyConsoleOutput(), _ => !string.IsNullOrWhiteSpace(VmOutput));
        ClearOutputCommand = new RelayCommand(_ => ClearConsoleOutput(), _ => HasConsoleOutput());
        ToggleDiagnosticsCommand = new RelayCommand(_ => ShowDiagnostics = !ShowDiagnostics);
        OpenSettingsCommand = new RelayCommand(_ =>
        {
            IsToolsViewVisible = false;
            IsVisualizerViewVisible = false;
            IsSettingsViewVisible = true;
        });
        BackToEditorCommand = new RelayCommand(_ => BackToEditor());
        BrowseJavaPathCommand = new RelayCommand(_ => BrowseJavaPath());
        BrowseCompilerRootCommand = new RelayCommand(_ => BrowseFolder(v => CompilerRoot = v, "Compiler Root Folder"));
        BrowseAntlrJarCommand = new RelayCommand(_ => BrowseAntlrJar());
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        ClearRecentProjectsCommand = new RelayCommand(_ =>
        {
            Settings.RecentProjects.Clear();
            Settings.Save();
            RefreshRecentCollections();
            RaiseSettingsValidationChanged();
            StatusText = "Recent projects cleared.";
            StatusColor = Brushes.LightGreen;
        });
        ClearRecentFilesCommand = new RelayCommand(_ =>
        {
            Settings.RecentFiles.Clear();
            Settings.Save();
            RefreshRecentCollections();
            RaiseSettingsValidationChanged();
            StatusText = "Recent files cleared.";
            StatusColor = Brushes.LightGreen;
        });

        OpenToolsCommand = new RelayCommand(_ =>
        {
            IsWelcomeViewVisible = false;
            IsSettingsViewVisible = false;
            IsVisualizerViewVisible = false;
            IsToolsViewVisible = true;
        });
        OpenVisualizerCommand = new RelayCommand(_ =>
        {
            IsSettingsViewVisible = false;
            IsToolsViewVisible = false;
            IsWelcomeViewVisible = false;
            IsVisualizerViewVisible = true;
            EnsureVisualizerDataLoaded();
        });
        OpenRecentProjectCommand = new RelayCommand(p => OpenRecentProject(p as string));
        OpenRecentFileCommand = new RelayCommand(p => OpenRecentFile(p as string));
        OpenQuickOpenCommand = new RelayCommand(_ => OpenQuickOpen(), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        CloseQuickOpenCommand = new RelayCommand(_ => CloseQuickOpen());
        OpenSelectedQuickOpenFileCommand = new RelayCommand(_ => OpenSelectedQuickOpenFile(), _ => !string.IsNullOrWhiteSpace(SelectedQuickOpenFile));
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
        CreateTestPairCommand = new RelayCommand(_ => CreateTestPairFromSuite(), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        OpenSelectedTestInputCommand = new RelayCommand(_ => OpenSelectedTestInputFile(), _ => SelectedTestResult is not null);
        OpenSelectedTestExpectedOutputCommand = new RelayCommand(_ => OpenSelectedTestExpectedOutputFile(), _ => SelectedTestResult is not null);
        CloseEditorTabCommand = new RelayCommand(tab => CloseEditorTab(tab as EditorTab), tab => tab is EditorTab);
        VisualizerLoadCurrentCommand = new RelayCommand(_ => LoadVisualizerFromCurrentFile());
        VisualizerStepCommand = new RelayCommand(_ => VisualizerStep(), _ => VisualizerCanStep);
        VisualizerRunAllCommand = new RelayCommand(_ => VisualizerRunAll(), _ => VisualizerCanStep);
        VisualizerResetCommand = new RelayCommand(_ => VisualizerReset(), _ => VisualizerHasData);
        VisualizerJumpToEndCommand = new RelayCommand(_ => VisualizerJumpToEnd(), _ => VisualizerCanStep);

        StartDebugCommand = new RelayCommand(_ => StartDebug(), _ => !string.IsNullOrWhiteSpace(_currentFile));
        DebugStepCommand = new RelayCommand(_ => DebugStep(), _ => CanDebugStep);
        DebugContinueCommand = new RelayCommand(_ => DebugContinue(), _ => CanDebugContinue);
        DebugJumpToCallCommand = new RelayCommand(_ => DebugJumpToCall(), _ => CanDebugJumpToCall);
        DebugStepBackCommand = new RelayCommand(_ => DebugStepBack(), _ => CanDebugBack);
        DebugStopCommand = new RelayCommand(_ => DebugStop(), _ => CanDebugStop);
        ToggleBreakpointAtCaretCommand = new RelayCommand(_ => ToggleBreakpointAtCaret(), _ => _editor.Document is not null);

        RefreshRecentCollections();
        IsWelcomeViewVisible = true;
        ApplyVisualizerOpcodeFilter();
    }

    public void SetReferenceResults(IEnumerable<FavaReferenceItem> references)
    {
        ReferenceResults.Clear();
        foreach (var reference in references)
            ReferenceResults.Add(reference);
    }

    public void ClearReferenceResults() => ReferenceResults.Clear();

    private void OpenFileInEditorTab(string filePath, bool focus)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        var existing = OpenEditorTabs.FirstOrDefault(tab =>
            string.Equals(tab.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            existing = new EditorTab
            {
                FilePath = filePath,
                Content = FileService.ReadText(filePath),
                IsDirty = false
            };
            OpenEditorTabs.Add(existing);
        }

        AddRecentFile(filePath);
        RefreshRecentCollections();

        if (focus || SelectedEditorTab is null)
            SelectedEditorTab = existing;
    }

    private void CloseEditorTab(EditorTab? tab)
    {
        if (tab is null)
            return;
        TryCloseEditorTab(tab);
    }

    private bool TryCloseEditorTab(EditorTab tab)
    {
        if (tab.IsDirty)
        {
            var answer = MessageBox.Show(
                $"Save changes to '{tab.FileName}' before closing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (answer == MessageBoxResult.Cancel)
                return false;

            if (answer == MessageBoxResult.Yes)
            {
                FileService.WriteText(tab.FilePath, tab.Content);
                tab.IsDirty = false;
            }
            else
            {
                tab.Content = FileService.ReadText(tab.FilePath);
                tab.IsDirty = false;
            }
        }

        var removedIndex = OpenEditorTabs.IndexOf(tab);
        var wasSelected = ReferenceEquals(_selectedEditorTab, tab);

        OpenEditorTabs.Remove(tab);
        _currentTestTabPaths.Remove(tab.FilePath);

        if (!wasSelected)
            return true;

        _hasUnsavedChanges = false;
        var newSelectedTab = OpenEditorTabs.Count == 0
            ? null
            : OpenEditorTabs[Math.Clamp(removedIndex, 0, OpenEditorTabs.Count - 1)];

        SelectEditorTab(newSelectedTab);
        return true;
    }

    private bool ClosePreviousTestTabs(IReadOnlyCollection<string> newTestPaths)
    {
        var newSet = new HashSet<string>(newTestPaths, StringComparer.OrdinalIgnoreCase);
        var tabsToClose = OpenEditorTabs
            .Where(tab => _currentTestTabPaths.Contains(tab.FilePath) && !newSet.Contains(tab.FilePath))
            .ToList();

        foreach (var tab in tabsToClose)
        {
            if (!TryCloseEditorTab(tab))
                return false;
        }

        return true;
    }

    private void SelectEditorTab(EditorTab? tab)
    {
        if (ReferenceEquals(_selectedEditorTab, tab))
            return;

        if (_selectedEditorTab is not null)
        {
            _selectedEditorTab.Content = _editor.Text;
            _selectedEditorTab.IsDirty = _hasUnsavedChanges;
        }

        _selectedEditorTab = tab;
        _currentFile = tab?.FilePath;
        _hasUnsavedChanges = tab?.IsDirty ?? false;

        _suppressDirtyTracking = true;
        _editor.Text = tab?.Content ?? "";
        _suppressDirtyTracking = false;

        OnPropertyChanged(nameof(SelectedEditorTab));
        OnPropertyChanged(nameof(CurrentFileName));
        RunCurrentCommand.RaiseCanExecuteChanged();
        StartDebugCommand.RaiseCanExecuteChanged();
    }

    private void OpenProject()
    {
        var dialog = new OpenFolderDialog { Title = "Select Project Folder" };
        if (dialog.ShowDialog() != true) return;

        if (!string.IsNullOrWhiteSpace(dialog.FolderName) && Directory.Exists(dialog.FolderName))
        {
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
        LoadProject(projectRoot);
        StatusText = $"Created project: {projectRoot}";
        StatusColor = Brushes.LightBlue;
    }

    private void LoadProject(string folder, bool skipUnsavedCheck = false)
    {
        if (!skipUnsavedCheck && !TryResolveUnsavedChanges())
            return;

        OpenEditorTabs.Clear();
        _currentTestTabPaths.Clear();
        _selectedEditorTab = null;
        _currentFile = null;
        _hasUnsavedChanges = false;
        _suppressDirtyTracking = true;
        _editor.Text = "";
        _suppressDirtyTracking = false;
        OnPropertyChanged(nameof(SelectedEditorTab));
        OnPropertyChanged(nameof(CurrentFileName));
        RunCurrentCommand.RaiseCanExecuteChanged();
        StartDebugCommand.RaiseCanExecuteChanged();

        var expandedPaths = CaptureExpandedPaths(ProjectTree.FirstOrDefault());
        var selectedPath = SelectedProjectNode?.FullPath;
        ProjectTree.Clear();
        try
        {
            var root = BuildNode(folder);
            ProjectTree.Add(root);
            Settings.ProjectRoot = folder;
            AddRecentProject(folder);
            Settings.Save();
            RefreshRecentCollections();
            EnsureTestFoldersConfigured(createIfMissing: false);
            RefreshTestSuiteCases();
            IsWelcomeViewVisible = false;
            BackToEditor();
            StatusText = $"Loaded project: {folder}";
            StatusColor = Brushes.LightBlue;
            OnPropertyChanged(nameof(CurrentProjectDirectory));
            OnPropertyChanged(nameof(IsWorkspaceVisible));
            RaiseSettingsValidationChanged();
            CreateTestPairCommand.RaiseCanExecuteChanged();

            RestoreExpandedPaths(root, expandedPaths);
            OpenInitialProjectFile(root, folder, selectedPath);
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
            LoadProject(Settings.ProjectRoot, skipUnsavedCheck: true);
        var createdNode = FindNodeByPath(ProjectTree.FirstOrDefault(), dialog.FileName);
        if (createdNode is not null)
            SetSelectedProjectNode(createdNode);
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
            LoadProject(Settings.ProjectRoot, skipUnsavedCheck: true);
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
            LoadProject(Settings.ProjectRoot, skipUnsavedCheck: true);
    }

    private void SaveFile()
    {
        var activeTab = _selectedEditorTab;
        if (activeTab is null || string.IsNullOrWhiteSpace(activeTab.FilePath))
            return;

        activeTab.Content = _editor.Text;
        FileService.WriteText(activeTab.FilePath, activeTab.Content);
        activeTab.IsDirty = false;
        _currentFile = activeTab.FilePath;
        AddRecentFile(activeTab.FilePath);
        RefreshRecentCollections();
        _hasUnsavedChanges = false;
        OnPropertyChanged(nameof(CurrentFileName));
        StatusText = $"Saved: {activeTab.FilePath}";
        StatusColor = Brushes.LightGreen;
    }

    private async Task RunLiveCheckAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentFile) || _isLiveChecking) return;
        if (!CanRunCompiler(showStatus: false)) return;

        _isLiveChecking = true;
        try
        {
            FileService.WriteText(_currentFile, _editor.Text);
            var runner = new JavaCompilerService(Settings);
            var result = await runner.RunFileAsync(_currentFile);

            UpdateCompilerMetadata(result.Output);
            var diagnostics = DiagnosticsParser.Parse(result.Output, _editor.Text)
                .Concat(DeadCodeAnalyzer.Analyze(_editor.Text, _hoverInfos))
                .ToList();
            Diagnostics.Clear();
            foreach (var d in diagnostics) Diagnostics.Add(d);
            OnPropertyChanged(nameof(DiagnosticsHeader));

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
        if (!CanRunCompiler(showStatus: true)) return;
        SaveFile();
        StatusText = "Running…";
        StatusColor = Brushes.LightGray;
        LastRunStatus = $"Running {Path.GetFileName(_currentFile)}";
        LastRunDurationText = "--";
        LastRunStatusBrush = Brushes.LightGray;

        var runner = new JavaCompilerService(Settings);
        var stopwatch = Stopwatch.StartNew();
        var result = await runner.RunFileAsync(_currentFile);
        stopwatch.Stop();
        UpdateCompilerMetadata(result.Output);
        var diagnostics = DiagnosticsParser.Parse(result.Output, _editor.Text)
            .Concat(DeadCodeAnalyzer.Analyze(_editor.Text, _hoverInfos))
            .ToList();
        Diagnostics.Clear();
        foreach (var d in diagnostics) Diagnostics.Add(d);
        OnPropertyChanged(nameof(DiagnosticsHeader));

        UpdateOutputs(result.Output);
        StatusText = result.Success ? "✅ Execution complete" : "❌ Execution failed";
        StatusColor = result.Success ? Brushes.LightGreen : Brushes.IndianRed;
        LastRunStatus = result.Success ? "Run completed" : "Run failed";
        LastRunDurationText = FormatDuration(stopwatch.Elapsed);
        LastRunStatusBrush = result.Success ? Brushes.LightGreen : Brushes.IndianRed;
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
        _lastFullOutput = fullOutput;
        ConstantPoolOutput = SliceSection(fullOutput,
            ["constant pool"],
            ["instructions", "type info", "source map", "vm output", "vm trace"]);

        InstructionsOutput = SliceSection(fullOutput,
            ["instructions"],
            ["type info", "source map", "vm output", "vm trace"]);

        UpdateCompilerMetadata(fullOutput);

        _vmTraceOutput = SliceSection(fullOutput,
            ["vm trace"],
            []);

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

        CopyOutputCommand.RaiseCanExecuteChanged();
        ClearOutputCommand.RaiseCanExecuteChanged();
    }

    private void CopyConsoleOutput()
    {
        if (string.IsNullOrWhiteSpace(VmOutput)) return;

        try
        {
            Clipboard.SetText(VmOutput);
            StatusText = "Console output copied.";
            StatusColor = Brushes.LightGreen;
        }
        catch (Exception ex)
        {
            StatusText = $"Copy failed: {ex.Message}";
            StatusColor = Brushes.Orange;
        }
    }

    private void ClearConsoleOutput()
    {
        _lastFullOutput = "";
        VmOutput = "";
        ConstantPoolOutput = "";
        InstructionsOutput = "";
        _typeInfoOutput = "";
        _sourceMapOutput = "";
        _hoverInfos = [];
        OutlineItems.Clear();
        OnPropertyChanged(nameof(HoverInfos));
        LastRunStatus = "Console cleared";
        LastRunDurationText = "--";
        LastRunStatusBrush = Brushes.Gray;
        StatusText = "Console cleared.";
        StatusColor = Brushes.LightGray;
        CopyOutputCommand.RaiseCanExecuteChanged();
        ClearOutputCommand.RaiseCanExecuteChanged();
    }

    private bool HasConsoleOutput() =>
        !string.IsNullOrWhiteSpace(VmOutput) ||
        !string.IsNullOrWhiteSpace(ConstantPoolOutput) ||
        !string.IsNullOrWhiteSpace(InstructionsOutput);

    private void UpdateCompilerMetadata(string fullOutput)
    {
        _typeInfoOutput = SliceSection(fullOutput,
            ["type info"],
            ["source map", "vm output", "vm trace"]);
        _sourceMapOutput = SliceSection(fullOutput,
            ["source map"],
            ["vm output", "vm trace"]);
        _hoverInfos = CompilerMetadataParser.ParseTypeInfo(_typeInfoOutput);
        RefreshOutlineItems();
        OnPropertyChanged(nameof(HoverInfos));
    }

    private void RefreshOutlineItems()
    {
        OutlineItems.Clear();
        foreach (var item in _hoverInfos
                     .Where(info => info.Kind.Contains("definition", StringComparison.OrdinalIgnoreCase))
                     .GroupBy(info => (info.Line, info.StartColumn, info.Name, info.Kind))
                     .Select(group => group.First())
                     .OrderBy(info => info.Line)
                     .ThenBy(info => info.StartColumn))
        {
            OutlineItems.Add(new FavaOutlineItem
            {
                Kind = item.Kind,
                Name = item.Name,
                Type = item.Type,
                Line = item.Line,
                Column = item.StartColumn
            });
        }
    }

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalSeconds >= 1
            ? $"{duration.TotalSeconds:0.00}s"
            : $"{duration.TotalMilliseconds:0}ms";

    private static int CountOutputLines(string output)
    {
        if (string.IsNullOrEmpty(output)) return 0;
        return output.Replace("\r\n", "\n").TrimEnd('\n').Split('\n').Length;
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
        _visualizerGlobals.Clear();
        _visualizerFrames.Clear();
        _visualizerFramePointer = -1;
        VisualizerStack.Clear();
        VisualizerGlobals.Clear();
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
        ExecuteVisualizerStep(captureTimeline: true);
    }

    private void ExecuteVisualizerStep(bool captureTimeline)
    {
        var instruction = _allVisualizerInstructions[_visualizerStepIndex];
        instruction.IsCurrent = true;
        var before = VisualizerService.StackToText(_visualizerRuntimeStack);
        var success = VisualizerService.ApplyInstruction(
            instruction,
            _visualizerRuntimeStack,
            _visualizerGlobals,
            _visualizerFrames,
            ref _visualizerFramePointer,
            instruction.Index,
            _allVisualizerConstants,
            out var note,
            out var outputLine,
            out var halted,
            out var newIp);
        var after = VisualizerService.StackToText(_visualizerRuntimeStack);
        if (captureTimeline && VisualizerTimeline.Count < MaxVisualizerTimelineEntries)
        {
            VisualizerTimeline.Add(new VisualizerTimelineEntry
            {
                Step = _visualizerStepIndex + 1,
                Instruction = instruction.Display,
                StackBefore = before,
                StackAfter = after,
                Note = note
            });
        }
        else if (captureTimeline)
        {
            VisualizerTimeline.Add(new VisualizerTimelineEntry
            {
                Step = _visualizerStepIndex + 1,
                Instruction = "(timeline truncated)",
                StackBefore = before,
                StackAfter = after,
                Note = $"Showing first {MaxVisualizerTimelineEntries} steps only."
            });
        }

        if (!string.IsNullOrWhiteSpace(outputLine))
            VisualizerRunOutput = string.IsNullOrWhiteSpace(VisualizerRunOutput) ? outputLine : $"{VisualizerRunOutput}\n{outputLine}";

        RefreshVisualizerStack();
        instruction.IsCurrent = false;
        if (!success || halted)
            _visualizerHalted = true;

        if (!_visualizerHalted)
        {
            if (newIp.HasValue)
            {
                var targetIndex = _allVisualizerInstructions.FindIndex(i => i.Index == newIp.Value);
                _visualizerStepIndex = targetIndex >= 0 ? targetIndex : _allVisualizerInstructions.Count;
            }
            else
            {
                _visualizerStepIndex++;
            }

            if (_visualizerStepIndex < _allVisualizerInstructions.Count)
                _allVisualizerInstructions[_visualizerStepIndex].IsCurrent = true;
        }

        RaiseVisualizerStateChanged();
    }

    private void VisualizerRunAll()
    {
        if (!VisualizerCanStep) return;
        var startStep = _visualizerStepIndex + 1;
        while (VisualizerCanStep)
            ExecuteVisualizerStep(captureTimeline: false);

        VisualizerTimeline.Add(new VisualizerTimelineEntry
        {
            Step = _visualizerStepIndex,
            Instruction = $"Run All from step {startStep}",
            StackBefore = "(captured in fast mode)",
            StackAfter = VisualizerService.StackToText(_visualizerRuntimeStack),
            Note = "Executed remaining instructions without per-step timeline details."
        });
    }

    private void VisualizerJumpToEnd()
    {
        if (!VisualizerCanStep) return;
        var startStep = _visualizerStepIndex + 1;
        var startStack = VisualizerService.StackToText(_visualizerRuntimeStack);
        while (VisualizerCanStep)
            ExecuteVisualizerStep(captureTimeline: false);

        VisualizerTimeline.Add(new VisualizerTimelineEntry
        {
            Step = _visualizerStepIndex,
            Instruction = $"Jumped from step {startStep}",
            StackBefore = startStack,
            StackAfter = VisualizerService.StackToText(_visualizerRuntimeStack),
            Note = "Fast-forwarded to final state."
        });
    }

    private void RefreshVisualizerStack()
    {
        VisualizerStack.Clear();
        foreach (var entry in VisualizerService.StackToEntries(_visualizerRuntimeStack))
            VisualizerStack.Add(entry);

        VisualizerGlobals.Clear();
        foreach (var entry in VisualizerService.GlobalsToEntries(_visualizerGlobals))
            VisualizerGlobals.Add(entry);
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

    // ── Breakpoint management ────────────────────────────────────────────────

    public event Action? BreakpointsChanged;

    public void ToggleBreakpoint(int lineNumber)
    {
        if (lineNumber <= 0) return;
        if (!_breakpointLines.Remove(lineNumber))
            _breakpointLines.Add(lineNumber);
        BreakpointsChanged?.Invoke();
        OnPropertyChanged(nameof(HasBreakpoints));
    }

    private void ToggleBreakpointAtCaret()
    {
        if (_editor.TextArea.Caret is null) return;
        ToggleBreakpoint(_editor.TextArea.Caret.Line);
    }

    // ── Debug mode ───────────────────────────────────────────────────────────

    private async void StartDebug()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;
        if (!CanRunCompiler(showStatus: true)) return;

        SaveFile();
        StatusText = "Starting debug session…";
        StatusColor = Brushes.LightGray;

        var runner = new JavaCompilerService(Settings);
        var result = await runner.RunFileAsync(_currentFile, includeTrace: true);
        UpdateOutputs(result.Output);
        LoadVisualizerFromSections(ConstantPoolOutput, InstructionsOutput);

        if (_allVisualizerInstructions.Count == 0)
        {
            StatusText = "Debug: no instructions to step through.";
            StatusColor = Brushes.Orange;
            return;
        }

        _sourceLineToInstructionPositions.Clear();
        var compilerMap = CompilerMetadataParser.ParseSourceMap(_sourceMapOutput, _allVisualizerInstructions);
        var sourceMap = compilerMap.Count > 0
            ? compilerMap
            : DebugSourceMapService.BuildLineToInstructionPositions(_editor.Text, _allVisualizerInstructions);
        foreach (var kvp in sourceMap)
            _sourceLineToInstructionPositions[kvp.Key] = kvp.Value;
        RebuildInstructionToSourceLineMap();
        PopulateDebugTraceInstructionPositions();
        ApplyTraceHintsToInstructionToLineMap();

        _debugHistory.Clear();
        IsDebugging = true;
        var firstBreakpointPosition = DebugSourceMapService.FindNextInstructionPositionForBreakpoints(
            _breakpointLines,
            _sourceLineToInstructionPositions,
            0);

        RunToInitialDebugStop();
        UpdateDebugPanel();
        RaiseDebugStateChanged();
        StatusText = firstBreakpointPosition.HasValue
            ? $"🔴 Debug mode — paused at first breakpoint (instruction {firstBreakpointPosition.Value + 1})"
            : "🔴 Debug mode — no breakpoints found, executed to completion";
        StatusColor = Brushes.IndianRed;
    }

    private void DebugStep()
    {
        if (!CanDebugStep) return;
        SaveDebugSnapshot();
        ExecuteVisualizerStep(captureTimeline: true);
        UpdateDebugPanel();
        RaiseDebugStateChanged();
    }

    private void DebugContinue()
    {
        if (!CanDebugContinue) return;
        SaveDebugSnapshot();

        // Run until a mapped breakpoint instruction is reached, or until end if none.
        if (_breakpointLines.Count == 0)
        {
            while (VisualizerCanStep)
                ExecuteVisualizerStep(captureTimeline: false);
        }
        else
        {
            // Always advance at least one step past the current position so we don't
            // immediately re-trigger the breakpoint we're already sitting on.
            ExecuteVisualizerStep(captureTimeline: false);

            var targetPosition = DebugSourceMapService.FindNextInstructionPositionForBreakpoints(
                _breakpointLines,
                _sourceLineToInstructionPositions,
                _visualizerStepIndex);

            if (targetPosition.HasValue)
            {
                while (VisualizerCanStep && _visualizerStepIndex < targetPosition.Value)
                    ExecuteVisualizerStep(captureTimeline: false);
            }
            else
            {
                while (VisualizerCanStep)
                    ExecuteVisualizerStep(captureTimeline: false);
            }
        }

        UpdateDebugPanel();
        RaiseDebugStateChanged();
    }

    private void RunToInitialDebugStop()
    {
        if (_breakpointLines.Count == 0)
        {
            while (VisualizerCanStep)
                ExecuteVisualizerStep(captureTimeline: false);
            return;
        }

        var targetPosition = DebugSourceMapService.FindNextInstructionPositionForBreakpoints(
            _breakpointLines,
            _sourceLineToInstructionPositions,
            _visualizerStepIndex);

        if (!targetPosition.HasValue)
        {
            while (VisualizerCanStep)
                ExecuteVisualizerStep(captureTimeline: false);
            return;
        }

        while (VisualizerCanStep && _visualizerStepIndex < targetPosition.Value)
            ExecuteVisualizerStep(captureTimeline: false);
    }

    private void RebuildInstructionToSourceLineMap()
    {
        _instructionPositionToSourceLine.Clear();
        foreach (var (line, positions) in _sourceLineToInstructionPositions.OrderBy(kvp => kvp.Key))
        {
            foreach (var position in positions)
            {
                if (!_instructionPositionToSourceLine.ContainsKey(position))
                    _instructionPositionToSourceLine[position] = line;
            }
        }
    }

    private void DebugJumpToCall()
    {
        if (!CanDebugJumpToCall) return;
        SaveDebugSnapshot();

        // Step until we hit (and execute) a call / ret / retval instruction, then pause.
        while (VisualizerCanStep)
        {
            var instr = _allVisualizerInstructions[_visualizerStepIndex];
            ExecuteVisualizerStep(captureTimeline: false);
            if (instr.Opcode is "call" or "ret" or "retval")
                break;
        }

        UpdateDebugPanel();
        RaiseDebugStateChanged();
    }

    private void DebugStepBack()
    {
        if (!CanDebugBack) return;
        RestoreDebugSnapshot(_debugHistory[^1]);
        _debugHistory.RemoveAt(_debugHistory.Count - 1);
        UpdateDebugPanel();
        RaiseDebugStateChanged();
    }

    private void DebugStop()
    {
        IsDebugging = false;
        _debugHistory.Clear();
        _sourceLineToInstructionPositions.Clear();
        _instructionPositionToSourceLine.Clear();
        _debugTraceInstructionPositions.Clear();
        UpdateDebugPanel();
        RaiseDebugStateChanged();
        StatusText = "Debug session stopped.";
        StatusColor = Brushes.LightGray;
    }

    private void SaveDebugSnapshot()
    {
        _debugHistory.Add(new DebugSnapshot
        {
            Stack = _visualizerRuntimeStack.Select(v => new VisualizerValue { Type = v.Type, Value = v.Value }).ToList(),
            Globals = _visualizerGlobals.Select(g => g is null ? null : new VisualizerValue { Type = g.Type, Value = g.Value }).ToList<VisualizerValue?>(),
            Frames = _visualizerFrames.Select(f => new VisualizerFrameState { FramePointer = f.FramePointer, LocalCount = f.LocalCount }).ToList(),
              FramePointer = _visualizerFramePointer,
            StepIndex = _visualizerStepIndex,
            Halted = _visualizerHalted,
            RunOutput = _visualizerRunOutput
        });
    }

    private void RestoreDebugSnapshot(DebugSnapshot snapshot)
    {
        _visualizerRuntimeStack.Clear();
        _visualizerRuntimeStack.AddRange(snapshot.Stack);
        _visualizerGlobals.Clear();
        _visualizerGlobals.AddRange(snapshot.Globals);
        _visualizerFrames.Clear();
        _visualizerFrames.AddRange(snapshot.Frames);
        _visualizerFramePointer = snapshot.FramePointer;
        _visualizerStepIndex = snapshot.StepIndex;
        _visualizerHalted = snapshot.Halted;
        _visualizerRunOutput = snapshot.RunOutput;
        OnPropertyChanged(nameof(VisualizerRunOutput));

        foreach (var instr in _allVisualizerInstructions)
            instr.IsCurrent = false;
        if (_visualizerStepIndex < _allVisualizerInstructions.Count)
            _allVisualizerInstructions[_visualizerStepIndex].IsCurrent = true;

        RefreshVisualizerStack();
        RaiseVisualizerStateChanged();
    }

    private void UpdateDebugPanel()
    {
        if (!_isDebugging)
        {
            DebugCurrentInstruction = "";
            DebugCurrentNote = "";
            DebugStepStatus = "";
            DebugCurrentSourceLine = null;
            DebugStack.Clear();
            DebugInlineValueHints.Clear();
            return;
        }

        var total = _allVisualizerInstructions.Count;
        var current = Math.Min(_visualizerStepIndex, total);

        if (_visualizerHalted)
        {
            DebugCurrentInstruction = "— Halted —";
            DebugCurrentNote = "Execution has stopped (halt instruction or error).";
            DebugStepStatus = $"Halted at step {current} / {total}";
            DebugCurrentSourceLine = null;
            DebugInlineValueHints.Clear();
        }
        else if (_visualizerStepIndex >= total)
        {
            DebugCurrentInstruction = "— End of program —";
            DebugCurrentNote = "All instructions executed.";
            DebugStepStatus = $"Finished  ({total} / {total})";
            DebugCurrentSourceLine = null;
            DebugInlineValueHints.Clear();
        }
        else
        {
            var instr = _allVisualizerInstructions[_visualizerStepIndex];
            DebugCurrentInstruction = instr.Display;
            DebugCurrentNote = instr.Description;
            DebugStepStatus = $"Step {_visualizerStepIndex + 1} / {total}";
            DebugCurrentSourceLine = ResolveCurrentDebugSourceLine(_visualizerStepIndex);
            RefreshDebugInlineValueHints(DebugCurrentSourceLine);
        }

        DebugStack.Clear();
        var entries = VisualizerService.StackToEntries(_visualizerRuntimeStack);
        for (var i = 0; i < entries.Count; i++)
            DebugStack.Add(new DebugStackEntry
            {
                Depth = entries[i].Depth,
                Type = entries[i].Type,
                Value = entries[i].Value,
                IsTop = i == 0
            });
    }

    private void RefreshDebugInlineValueHints(int? sourceLine)
    {
        DebugInlineValueHints.Clear();
        if (sourceLine is not int line || line <= 0)
            return;

        var values = _hoverInfos
            .Where(info => info.Line == line &&
                           !info.IsDefinition &&
                           !string.IsNullOrWhiteSpace(info.Name) &&
                           (info.Kind.Contains("variable", StringComparison.OrdinalIgnoreCase) ||
                            info.Kind.Contains("argument", StringComparison.OrdinalIgnoreCase) ||
                            info.Kind.Contains("global", StringComparison.OrdinalIgnoreCase)))
            .GroupBy(info => info.Name)
            .Select(group => group.First())
            .Select(ReadInlineValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .Take(4)
            .ToList();

        if (values.Count > 0)
            DebugInlineValueHints.Add(new InlineValueHint { Line = line, Text = string.Join("   ", values) });
    }

    private string? ReadInlineValue(FavaHoverInfo reference)
    {
        var symbol = FindBestSymbolDefinition(reference);
        if (symbol is null || symbol.Address < 0)
            return null;

        VisualizerValue? value = null;
        if (symbol.Kind.Contains("global", StringComparison.OrdinalIgnoreCase))
        {
            if (symbol.Address < _visualizerGlobals.Count)
                value = _visualizerGlobals[symbol.Address];
        }
        else if (_visualizerFramePointer >= 0)
        {
            var index = _visualizerFramePointer + symbol.Address;
            if (index >= 0 && index < _visualizerRuntimeStack.Count)
                value = _visualizerRuntimeStack[index];
        }

        if (value is null || string.Equals(value.Type, "null", StringComparison.OrdinalIgnoreCase))
            return null;

        return $"{reference.Name} = {FormatInlineValue(value)}";
    }

    private FavaHoverInfo? FindBestSymbolDefinition(FavaHoverInfo reference) =>
        _hoverInfos
            .Where(info => info.IsDefinition &&
                           string.Equals(info.Name, reference.Name, StringComparison.Ordinal) &&
                           IsCompatibleSymbolKind(info, reference))
            .OrderBy(info => Math.Abs(info.Line - reference.Line))
            .FirstOrDefault();

    private static bool IsCompatibleSymbolKind(FavaHoverInfo definition, FavaHoverInfo reference)
    {
        if (definition.Kind.Contains("global", StringComparison.OrdinalIgnoreCase))
            return reference.Kind.Contains("global", StringComparison.OrdinalIgnoreCase);
        if (definition.Kind.Contains("argument", StringComparison.OrdinalIgnoreCase))
            return reference.Kind.Contains("argument", StringComparison.OrdinalIgnoreCase);
        if (definition.Kind.Contains("local", StringComparison.OrdinalIgnoreCase))
            return reference.Kind.Contains("local", StringComparison.OrdinalIgnoreCase) ||
                   reference.Kind.Contains("variable", StringComparison.OrdinalIgnoreCase);
        return false;
    }

    private static string FormatInlineValue(VisualizerValue value)
    {
        if (value.Value is List<VisualizerValue?> array)
        {
            var preview = string.Join(", ", array.Take(4).Select(item => item is null ? "NULL" : FormatInlineValue(item)));
            if (array.Count > 4)
                preview += ", ...";
            return $"[{preview}]";
        }

        return value.Value?.ToString() ?? "NULL";
    }

    private void RaiseDebugStateChanged()
    {
        OnPropertyChanged(nameof(CanDebugStep));
        OnPropertyChanged(nameof(CanDebugBack));
        OnPropertyChanged(nameof(CanDebugContinue));
        OnPropertyChanged(nameof(CanDebugJumpToCall));
        OnPropertyChanged(nameof(CanDebugStop));
        StartDebugCommand.RaiseCanExecuteChanged();
        DebugStepCommand.RaiseCanExecuteChanged();
        DebugContinueCommand.RaiseCanExecuteChanged();
        DebugJumpToCallCommand.RaiseCanExecuteChanged();
        DebugStepBackCommand.RaiseCanExecuteChanged();
        DebugStopCommand.RaiseCanExecuteChanged();
        RaiseVisualizerStateChanged();
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
        var selectedName = SelectedTestResult?.Name;
        RefreshTestSuiteCases(selectedName);
        if (TestResults.Count == 0)
        {
            TestSummary = "No tests found. Use 'New Test' to create one.";
            StatusColor = Brushes.Orange;
            return;
        }

        SelectedTestResult = null;
        TestSummary = "Running tests…";
        StatusColor = Brushes.LightGray;

        var runner = new TestRunnerService(Settings);
        var results = await runner.RunAllTestsAsync();
        TestResults.Clear();
        foreach (var r in results.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
            TestResults.Add(r);
        if (!string.IsNullOrWhiteSpace(selectedName))
            SelectedTestResult = TestResults.FirstOrDefault(t => t.Name == selectedName);
        SelectedTestResult ??= TestResults.FirstOrDefault();

        var passed = results.Count(r => r.Passed);
        var total = results.Count;
        TestSummary = passed == total ? $"✅ ALL TESTS PASSED ({passed}/{total})" : $"❌ {passed}/{total} tests passed";
        StatusColor = passed == total ? Brushes.LightGreen : Brushes.IndianRed;
        RaiseTestStatsChanged();
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
        RaiseTestStatsChanged();
    }

    private void EnsureTestFoldersConfigured(bool createIfMissing)
    {
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            return;

        var changed = false;
        if (string.IsNullOrWhiteSpace(Settings.InputsDir))
        {
            Settings.InputsDir = Path.Combine(Settings.ProjectRoot, "tests", "inputs");
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(Settings.OutputsDir))
        {
            Settings.OutputsDir = Path.Combine(Settings.ProjectRoot, "tests", "outputs");
            changed = true;
        }

        if (createIfMissing)
        {
            Directory.CreateDirectory(Settings.InputsDir);
            Directory.CreateDirectory(Settings.OutputsDir);
        }

        if (changed)
            Settings.Save();
    }

    private void RefreshTestSuiteCases(string? selectedName = null)
    {
        EnsureTestFoldersConfigured(createIfMissing: false);
        TestResults.Clear();

        if (string.IsNullOrWhiteSpace(Settings.InputsDir) || !Directory.Exists(Settings.InputsDir))
        {
            TestSummary = "No tests configured yet. Use 'New Test' to create one.";
            SelectedTestResult = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(Settings.OutputsDir))
        {
            TestSummary = "Configure outputs folder in Settings or use 'New Test'.";
            SelectedTestResult = null;
            return;
        }

        var testNames = Directory.GetFiles(Settings.InputsDir, "*.fava")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OfType<string>()
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var name in testNames)
        {
            var input = Path.Combine(Settings.InputsDir, $"{name}.fava");
            var expected = Path.Combine(Settings.OutputsDir, $"{name}.txt");
            TestResults.Add(new TestResult
            {
                Name = name,
                InputFile = input,
                ExpectedOutputFile = expected,
                HasRun = false,
                Passed = false,
                Message = $"Input: {input}\nExpected output: {expected}"
            });
        }
        RaiseTestStatsChanged();

        TestSummary = testNames.Count == 0
            ? "No tests found. Use 'New Test' to create one."
            : $"Discovered {testNames.Count} test(s).";

        _suppressTestSelectionOpen = true;
        try
        {
            if (string.IsNullOrWhiteSpace(selectedName))
                SelectedTestResult = TestResults.FirstOrDefault();
            else
                SelectedTestResult = TestResults.FirstOrDefault(t => t.Name == selectedName) ?? TestResults.FirstOrDefault();
        }
        finally
        {
            _suppressTestSelectionOpen = false;
        }
    }

    private void CreateTestPairFromSuite()
    {
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            return;

        EnsureTestFoldersConfigured(createIfMissing: true);

        var dialog = new SaveFileDialog
        {
            Title = "Create Input Test File",
            InitialDirectory = Settings.InputsDir,
            Filter = "Fava file (*.fava)|*.fava",
            DefaultExt = ".fava",
            AddExtension = true,
            FileName = "test"
        };

        if (dialog.ShowDialog() != true)
            return;

        var inputPath = dialog.FileName;
        var testName = Path.GetFileNameWithoutExtension(inputPath);
        if (string.IsNullOrWhiteSpace(testName))
            return;

        var outputPath = Path.Combine(Settings.OutputsDir, $"{testName}.txt");
        var inputDirectory = Path.GetDirectoryName(inputPath);
        if (string.IsNullOrWhiteSpace(inputDirectory))
        {
            StatusText = "Failed to create test: invalid input file path.";
            StatusColor = Brushes.IndianRed;
            return;
        }

        Directory.CreateDirectory(inputDirectory);
        Directory.CreateDirectory(Settings.OutputsDir);
        if (!File.Exists(inputPath))
            FileService.WriteText(inputPath, "");
        if (!File.Exists(outputPath))
            FileService.WriteText(outputPath, "");

        if (!string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            LoadProject(Settings.ProjectRoot, skipUnsavedCheck: true);
        RefreshTestSuiteCases(testName);
        StatusText = $"Created test pair: {testName}.fava + {testName}.txt";
        StatusColor = Brushes.LightGreen;
    }

    private void OpenSelectedTestInputFile()
    {
        var inputPath = SelectedTestResult?.InputFile;
        if (string.IsNullOrWhiteSpace(inputPath))
            return;
        if (!File.Exists(inputPath))
            return;
        OpenFileInEditorTab(inputPath, focus: true);
    }

    private void OpenSelectedTestExpectedOutputFile()
    {
        var expectedPath = SelectedTestResult?.ExpectedOutputFile;
        if (string.IsNullOrWhiteSpace(expectedPath))
            return;

        var directory = Path.GetDirectoryName(expectedPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        if (!File.Exists(expectedPath))
            FileService.WriteText(expectedPath, "");

        OpenFileInEditorTab(expectedPath, focus: true);
    }

    private void OpenSelectedTestFilesInTabs(TestResult selectedTest)
    {
        if (string.IsNullOrWhiteSpace(selectedTest.InputFile))
            return;
        if (!File.Exists(selectedTest.InputFile))
            return;

        var testPaths = new List<string> { selectedTest.InputFile };

        if (string.IsNullOrWhiteSpace(selectedTest.ExpectedOutputFile))
        {
            if (!ClosePreviousTestTabs(testPaths))
                return;

            _currentTestTabPaths.Clear();
            _currentTestTabPaths.Add(selectedTest.InputFile);
            OpenFileInEditorTab(selectedTest.InputFile, focus: true);
            return;
        }

        var outputPath = selectedTest.ExpectedOutputFile;
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        if (!File.Exists(outputPath))
            FileService.WriteText(outputPath, "");

        testPaths.Add(outputPath);
        if (!ClosePreviousTestTabs(testPaths))
            return;

        _currentTestTabPaths.Clear();
        foreach (var path in testPaths)
            _currentTestTabPaths.Add(path);

        OpenFileInEditorTab(selectedTest.InputFile, focus: true);
        OpenFileInEditorTab(outputPath, focus: false);
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
            JavaPath = dialog.FileName;
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
            AntlrJar = dialog.FileName;
        }
    }

    private void BrowseFolder(Action<string> setter, string title)
    {
        var dialog = new OpenFolderDialog { Title = $"Select {title}" };
        if (dialog.ShowDialog() == true)
        {
            setter(dialog.FolderName);
            OnPropertyChanged(nameof(Settings));
            RaiseSettingsValidationChanged();
        }
    }

    private void SaveSettings()
    {
        Settings.Save();
        StatusText = "Settings saved.";
        StatusColor = Brushes.LightGreen;
        RaiseSettingsValidationChanged();
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
        var skippingTypeInfo = false;
        var skippingSourceMap = false;

        foreach (var line in lines)
        {
            var normalized = NormalizeHeader(line);

            if (normalized == "constant pool")
            {
                skippingConstant = true;
                skippingInstructions = false;
                skippingTypeInfo = false;
                skippingSourceMap = false;
                continue;
            }

            if (normalized == "instructions")
            {
                skippingInstructions = true;
                skippingConstant = false;
                skippingTypeInfo = false;
                skippingSourceMap = false;
                continue;
            }

            if (normalized == "type info")
            {
                skippingTypeInfo = true;
                skippingConstant = false;
                skippingInstructions = false;
                skippingSourceMap = false;
                continue;
            }

            if (normalized == "source map")
            {
                skippingSourceMap = true;
                skippingConstant = false;
                skippingInstructions = false;
                skippingTypeInfo = false;
                continue;
            }

            if (normalized == "vm output")
            {
                skippingConstant = false;
                skippingInstructions = false;
                skippingTypeInfo = false;
                skippingSourceMap = false;
                continue;
            }

            if (!skippingConstant && !skippingInstructions && !skippingTypeInfo && !skippingSourceMap)
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

    private void PopulateDebugTraceInstructionPositions()
    {
        _debugTraceInstructionPositions.Clear();
        if (string.IsNullOrWhiteSpace(_vmTraceOutput))
            return;

        var addressToPosition = _allVisualizerInstructions
            .Select((instruction, position) => new { instruction.Index, position })
            .ToDictionary(item => item.Index, item => item.position);

        foreach (var address in VisualizerService.ParseTraceInstructionAddresses(_vmTraceOutput))
        {
            if (addressToPosition.TryGetValue(address, out var position))
                _debugTraceInstructionPositions.Add(position);
        }
    }

    private void ApplyTraceHintsToInstructionToLineMap()
    {
        if (_debugTraceInstructionPositions.Count == 0 || _instructionPositionToSourceLine.Count == 0)
            return;

        int? activeLine = null;
        foreach (var position in _debugTraceInstructionPositions)
        {
            if (_instructionPositionToSourceLine.TryGetValue(position, out var mappedLine))
            {
                activeLine = mappedLine;
                continue;
            }

            if (activeLine.HasValue)
                _instructionPositionToSourceLine[position] = activeLine.Value;
        }
    }

    private int? ResolveCurrentDebugSourceLine(int instructionPosition)
    {
        if (_instructionPositionToSourceLine.TryGetValue(instructionPosition, out var directLine))
            return directLine;

        var previous = _instructionPositionToSourceLine
            .Where(kvp => kvp.Key < instructionPosition)
            .OrderByDescending(kvp => kvp.Key)
            .Select(kvp => (int?)kvp.Value)
            .FirstOrDefault();

        if (previous.HasValue)
            return previous.Value;

        return _instructionPositionToSourceLine
            .Where(kvp => kvp.Key > instructionPosition)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => (int?)kvp.Value)
            .FirstOrDefault();
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

    private void OpenRecentProject(string? projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath)) return;
        if (!Directory.Exists(projectPath))
        {
            StatusText = $"Recent project not found: {projectPath}";
            StatusColor = Brushes.Orange;
            Settings.RecentProjects.RemoveAll(p => string.Equals(p, projectPath, StringComparison.OrdinalIgnoreCase));
            Settings.Save();
            RefreshRecentCollections();
            return;
        }

        LoadProject(projectPath);
    }

    private void OpenRecentFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;
        if (!File.Exists(filePath))
        {
            StatusText = $"Recent file not found: {filePath}";
            StatusColor = Brushes.Orange;
            Settings.RecentFiles.RemoveAll(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));
            Settings.Save();
            RefreshRecentCollections();
            return;
        }

        var projectRoot = Settings.ProjectRoot;
        var fileDirectory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(projectRoot) || string.IsNullOrWhiteSpace(fileDirectory) || !filePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(fileDirectory) || !Directory.Exists(fileDirectory))
                return;
            LoadProject(fileDirectory);
            if (string.IsNullOrWhiteSpace(Settings.ProjectRoot))
                return;
        }

        var node = FindNodeByPath(ProjectTree.FirstOrDefault(), filePath);
        if (node is not null)
            SetSelectedProjectNode(node);
    }

    private void AddRecentProject(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath)) return;
        Settings.RecentProjects.RemoveAll(p => string.Equals(p, projectPath, StringComparison.OrdinalIgnoreCase));
        Settings.RecentProjects.Insert(0, projectPath);
    }

    private void AddRecentFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;
        Settings.RecentFiles.RemoveAll(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));
        Settings.RecentFiles.Insert(0, filePath);
        Settings.Save();
    }

    private void RefreshRecentCollections()
    {
        RecentProjects.Clear();
        foreach (var project in Settings.RecentProjects.Where(Directory.Exists))
            RecentProjects.Add(project);

        WelcomeRecentProjects.Clear();
        foreach (var project in RecentProjects)
            WelcomeRecentProjects.Add(new RecentProjectItem(project));

        RecentFiles.Clear();
        foreach (var file in Settings.RecentFiles.Where(File.Exists))
            RecentFiles.Add(file);

        OnPropertyChanged(nameof(RecentSummary));
    }

    private void OpenQuickOpen()
    {
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot)) return;
        IsQuickOpenVisible = true;
        QuickOpenQuery = "";
        ApplyQuickOpenFilter();
    }

    private void CloseQuickOpen()
    {
        IsQuickOpenVisible = false;
        QuickOpenQuery = "";
        SelectedQuickOpenFile = null;
    }

    private void OpenSelectedQuickOpenFile()
    {
        if (string.IsNullOrWhiteSpace(SelectedQuickOpenFile)) return;
        OpenRecentFile(SelectedQuickOpenFile);
        CloseQuickOpen();
    }

    private void ApplyQuickOpenFilter()
    {
        QuickOpenResults.Clear();
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot) || !Directory.Exists(Settings.ProjectRoot))
            return;

        var query = (QuickOpenQuery ?? "").Trim();
        var files = Directory.GetFiles(Settings.ProjectRoot, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".fava", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            .Where(f => string.IsNullOrWhiteSpace(query) || f.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(100)
            .ToList();

        foreach (var file in files)
            QuickOpenResults.Add(file);

        SelectedQuickOpenFile = QuickOpenResults.FirstOrDefault();
    }

    private void BackToEditor()
    {
        IsWelcomeViewVisible = false;
        IsSettingsViewVisible = false;
        IsToolsViewVisible = false;
        IsVisualizerViewVisible = false;
    }

    private bool TryResolveUnsavedChanges()
    {
        var dirtyTabs = OpenEditorTabs.Where(tab => tab.IsDirty).ToList();
        if (dirtyTabs.Count == 0)
            return true;

        var answer = MessageBox.Show(
            dirtyTabs.Count == 1
                ? $"You have unsaved changes in '{dirtyTabs[0].FileName}'. Save before continuing?"
                : $"You have unsaved changes in {dirtyTabs.Count} open files. Save before continuing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (answer == MessageBoxResult.Cancel)
            return false;
        if (answer == MessageBoxResult.Yes)
        {
            foreach (var tab in dirtyTabs)
            {
                FileService.WriteText(tab.FilePath, tab.Content);
                tab.IsDirty = false;
            }
        }
        if (answer == MessageBoxResult.No)
        {
            foreach (var tab in dirtyTabs)
            {
                tab.Content = FileService.ReadText(tab.FilePath);
                tab.IsDirty = false;
            }
        }

        if (_selectedEditorTab is not null)
        {
            _hasUnsavedChanges = _selectedEditorTab.IsDirty;
            _suppressDirtyTracking = true;
            _editor.Text = _selectedEditorTab.Content;
            _suppressDirtyTracking = false;
        }
        OnPropertyChanged(nameof(CurrentFileName));
        return true;
    }

    private bool CanRunCompiler(bool showStatus)
    {
        var hasJava = !string.IsNullOrWhiteSpace(Settings.JavaPath);
        var hasCompilerRoot = !string.IsNullOrWhiteSpace(Settings.CompilerRoot) && Directory.Exists(Settings.CompilerRoot);
        var hasAntlr = !string.IsNullOrWhiteSpace(Settings.AntlrJar) && File.Exists(Settings.AntlrJar);
        if (hasJava && hasCompilerRoot && hasAntlr)
            return true;

        if (showStatus)
        {
            StatusText = "Configure Java path, compiler root and ANTLR jar in Settings before running.";
            StatusColor = Brushes.Orange;
        }

        return false;
    }

    private static HashSet<string> CaptureExpandedPaths(ProjectNode? root)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CaptureExpandedPathsRecursive(root, result);
        return result;
    }

    private static void CaptureExpandedPathsRecursive(ProjectNode? node, HashSet<string> result)
    {
        if (node is null) return;
        if (node.IsExpanded && !string.IsNullOrWhiteSpace(node.FullPath))
            result.Add(node.FullPath);

        foreach (var child in node.Children)
            CaptureExpandedPathsRecursive(child, result);
    }

    private static void RestoreExpandedPaths(ProjectNode? root, HashSet<string> expandedPaths)
    {
        if (root is null) return;
        root.IsExpanded = expandedPaths.Contains(root.FullPath);
        foreach (var child in root.Children)
            RestoreExpandedPaths(child, expandedPaths);
    }

    private void OpenInitialProjectFile(ProjectNode root, string folder, string? previousSelectedPath)
    {
        ProjectNode? nodeToOpen = null;
        if (!string.IsNullOrWhiteSpace(previousSelectedPath))
            nodeToOpen = FindNodeByPath(root, previousSelectedPath);

        if (nodeToOpen is null)
        {
            var recentInProject = Settings.RecentFiles
                .FirstOrDefault(path =>
                    path.EndsWith(".fava", StringComparison.OrdinalIgnoreCase)
                    && File.Exists(path)
                    && IsPathInside(path, folder));
            if (!string.IsNullOrWhiteSpace(recentInProject))
                nodeToOpen = FindNodeByPath(root, recentInProject);
        }

        if (nodeToOpen is null)
            nodeToOpen = FindFirstFileNode(root, ".fava");
        if (nodeToOpen is null)
            nodeToOpen = FindFirstFileNode(root, ".txt");

        if (nodeToOpen is not null)
        {
            SetSelectedProjectNode(nodeToOpen);
            return;
        }

        OpenEditorTabs.Clear();
        _currentTestTabPaths.Clear();
        _selectedEditorTab = null;
        _currentFile = null;
        _suppressDirtyTracking = true;
        _editor.Text = "";
        _suppressDirtyTracking = false;
        _hasUnsavedChanges = false;
        OnPropertyChanged(nameof(SelectedEditorTab));
        OnPropertyChanged(nameof(CurrentFileName));
        RunCurrentCommand.RaiseCanExecuteChanged();
        StartDebugCommand.RaiseCanExecuteChanged();
    }

    private static bool IsPathInside(string path, string rootPath)
    {
        var normalizedPath = Path.GetFullPath(path);
        var normalizedRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static ProjectNode? FindFirstFileNode(ProjectNode? node, string extension)
    {
        if (node is null) return null;
        if (!node.IsDirectory && node.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            return node;

        foreach (var child in node.Children)
        {
            var found = FindFirstFileNode(child, extension);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static ProjectNode? FindNodeByPath(ProjectNode? node, string fullPath)
    {
        if (node is null) return null;
        if (string.Equals(node.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
            return node;

        foreach (var child in node.Children)
        {
            var found = FindNodeByPath(child, fullPath);
            if (found is not null)
                return found;
        }

        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void RaiseSettingsValidationChanged()
    {
        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(SettingsProjectName));
        OnPropertyChanged(nameof(SettingsProjectPath));
        OnPropertyChanged(nameof(CompilerStatusText));
        OnPropertyChanged(nameof(CompilerStatusBrush));
        OnPropertyChanged(nameof(JavaStatusText));
        OnPropertyChanged(nameof(JavaStatusBrush));
        OnPropertyChanged(nameof(CompilerRootStatusText));
        OnPropertyChanged(nameof(CompilerRootStatusBrush));
        OnPropertyChanged(nameof(AntlrStatusText));
        OnPropertyChanged(nameof(AntlrStatusBrush));
        OnPropertyChanged(nameof(TestFoldersStatusText));
        OnPropertyChanged(nameof(TestFoldersStatusBrush));
        OnPropertyChanged(nameof(IsCompilerConfigured));
        OnPropertyChanged(nameof(RecentSummary));
    }

    private void RaiseTestStatsChanged()
    {
        OnPropertyChanged(nameof(TestTotalCount));
        OnPropertyChanged(nameof(TestRunCount));
        OnPropertyChanged(nameof(TestPassedCount));
        OnPropertyChanged(nameof(TestFailedCount));
        OnPropertyChanged(nameof(TestPassPercent));
        OnPropertyChanged(nameof(TestPassPercentText));
        OnPropertyChanged(nameof(SelectedTestExpectedOutput));
        OnPropertyChanged(nameof(SelectedTestActualOutput));
        OnPropertyChanged(nameof(SelectedTestDiffOutput));
        OnPropertyChanged(nameof(SelectedTestDurationText));
    }
}

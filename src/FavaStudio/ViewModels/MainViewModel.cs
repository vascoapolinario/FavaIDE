using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using FavaStudio.Models;
using FavaStudio.Services;
using Microsoft.Win32;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace FavaStudio.ViewModels;

public sealed record DiscordPresenceOption(string Key, string Label);
internal sealed record ProjectTemplateOption(string Key, string Label, string Description);
internal sealed record ProjectCreationRequest(string ParentFolder, string FolderName, string Title, string Description, string TemplateKey);

public class MainViewModel : INotifyPropertyChanged
{
    private readonly TextEditor _editor;
    private bool _isLiveChecking;
    private readonly DispatcherTimer _liveCheckTimer;
    private const int LiveCheckDebounceMilliseconds = 3000;
    private int _liveCheckRevision;
    private string? _lastLiveCheckedFile;
    private string? _lastLiveCheckedText;
    private string? _currentFile;
    private ProjectNode? _selectedProjectNode;
    private bool _suppressDirtyTracking;
    private bool _hasUnsavedChanges;
    private Brush _statusColor = Brushes.LightGray;
    private string _statusText = "Ready.";
    private string _vmOutput = "";
    private string _constantPoolOutput = "";
    private string _instructionsOutput = "";
    private string _consoleInput = "";
    private Func<string, Task>? _consoleInputWriter;
    private bool _isConsoleAcceptingInput;
    private string _typeInfoOutput = "";
    private string _sourceMapOutput = "";
    private string _lastFullOutput = "";
    private string _lastRunStatus = "No run yet";
    private string _lastRunDurationText = "--";
    private Brush _lastRunStatusBrush = Brushes.Gray;
    private CancellationTokenSource? _executionCancellationSource;
    private bool _isExecutionRunning;
    private bool _showDiagnostics = true;
    private bool _isSettingsViewVisible;
    private bool _isToolsViewVisible;
    private bool _isVisualizerViewVisible;
    private bool _isWelcomeViewVisible;
    private bool _isQuickOpenVisible;
    private string _quickOpenQuery = "";
    private string? _selectedQuickOpenFile;
    private bool _isProjectDeleteDialogVisible;
    private string _pendingDeleteProjectPath = "";
    private string _pendingDeleteProjectName = "";
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
    public IReadOnlyList<DiscordPresenceOption> DiscordPresenceOptions { get; } =
    [
        new("file", "Current file"),
        new("project", "Project name"),
        new("projectStatus", "Project + run status"),
        new("status", "Run status"),
        new("view", "Current IDE view"),
        new("custom", "Custom text"),
        new("hidden", "Do not show")
    ];
    public IReadOnlyList<DiscordPresenceOption> SyntaxColorModeOptions { get; } =
    [
        new("default", "Default Fava syntax"),
        new("theme", "Theme colors"),
        new("simple", "Theme colors simplified")
    ];
    private static readonly IReadOnlyList<ProjectTemplateOption> ProjectTemplateOptions =
    [
        new("tutorial", "Create project with tutorial file", "Creates main.fava with a runnable tour of Fava's language features."),
        new("hello", "Create project with main.fava", "Creates a clean Hello World main.fava file."),
        new("empty", "Create empty project", "Creates only the project folder and metadata.")
    ];

    public SettingsService Settings { get; } = SettingsService.Load();
    public event Action? StyleSettingsChanged;

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
        : GetProjectDisplayName(Settings.ProjectRoot);
    public string SettingsProjectDescription => string.IsNullOrWhiteSpace(Settings.ProjectRoot)
        ? "Create or open a project to edit its title and description."
        : GetProjectMetadata(Settings.ProjectRoot)?.Description ?? "No description yet.";
    public string SettingsProjectPath => string.IsNullOrWhiteSpace(Settings.ProjectRoot) ? "Open a project to enable project-local workflows." : Settings.ProjectRoot;
    public string ProjectTitle
    {
        get => string.IsNullOrWhiteSpace(Settings.ProjectRoot) ? "" : GetProjectDisplayName(Settings.ProjectRoot);
        set => SetCurrentProjectMetadata(title: value, description: null);
    }
    public string ProjectDescription
    {
        get => string.IsNullOrWhiteSpace(Settings.ProjectRoot) ? "" : GetProjectMetadata(Settings.ProjectRoot)?.Description ?? "";
        set => SetCurrentProjectMetadata(title: null, description: value);
    }
    public string IdeDataLocation => SettingsService.DataDirectory;
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
    public string CompilerStatusText => IsCompilerConfigured ? "Ready to run" : "Needs configuration";
    public Brush CompilerStatusBrush => IsCompilerConfigured ? Brushes.LightGreen : Brushes.Orange;
    public string JavaStatusText => string.IsNullOrWhiteSpace(Settings.JavaPath) ? "Required" : "Configured";
    public Brush JavaStatusBrush => string.IsNullOrWhiteSpace(Settings.JavaPath) ? Brushes.Orange : Brushes.LightGreen;
    public string CompilerRootStatusText => Directory.Exists(Settings.CompilerRoot) ? "Folder found" : "Missing folder";
    public Brush CompilerRootStatusBrush => Directory.Exists(Settings.CompilerRoot) ? Brushes.LightGreen : Brushes.Orange;
    public bool IsCompilerConfigured =>
        !string.IsNullOrWhiteSpace(Settings.JavaPath) &&
        Directory.Exists(Settings.CompilerRoot) &&
        File.Exists(Settings.ResolveAntlrJar());
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
    public string ConsoleInput { get => _consoleInput; set { _consoleInput = value; OnPropertyChanged(); SendConsoleInputCommand.RaiseCanExecuteChanged(); } }
    public bool IsConsoleAcceptingInput
    {
        get => _isConsoleAcceptingInput;
        private set
        {
            if (_isConsoleAcceptingInput == value)
                return;
            _isConsoleAcceptingInput = value;
            OnPropertyChanged();
        }
    }
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
            OnPropertyChanged(nameof(CanStopExecution));
        }
    }
    public bool IsSettingsViewVisible { get => _isSettingsViewVisible; set { _isSettingsViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsToolsViewVisible { get => _isToolsViewVisible; set { _isToolsViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsVisualizerViewVisible { get => _isVisualizerViewVisible; set { _isVisualizerViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsWelcomeViewVisible { get => _isWelcomeViewVisible; set { _isWelcomeViewVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkspaceVisible)); } }
    public bool IsWorkspaceVisible => !IsSettingsViewVisible && !IsToolsViewVisible && !IsVisualizerViewVisible && !IsWelcomeViewVisible;
    public bool IsQuickOpenVisible { get => _isQuickOpenVisible; set { _isQuickOpenVisible = value; OnPropertyChanged(); } }
    public bool IsProjectDeleteDialogVisible { get => _isProjectDeleteDialogVisible; set { _isProjectDeleteDialogVisible = value; OnPropertyChanged(); } }
    public string PendingDeleteProjectPath { get => _pendingDeleteProjectPath; set { _pendingDeleteProjectPath = value; OnPropertyChanged(); } }
    public string PendingDeleteProjectName { get => _pendingDeleteProjectName; set { _pendingDeleteProjectName = value; OnPropertyChanged(); } }
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
    public string UiBackgroundColor { get => Settings.UiBackgroundColor; set => SetStyleSetting(Settings.UiBackgroundColor, value, v => Settings.UiBackgroundColor = v); }
    public string UiPanelColor { get => Settings.UiPanelColor; set => SetStyleSetting(Settings.UiPanelColor, value, v => Settings.UiPanelColor = v); }
    public string UiPanelAltColor { get => Settings.UiPanelAltColor; set => SetStyleSetting(Settings.UiPanelAltColor, value, v => Settings.UiPanelAltColor = v); }
    public string UiTextColor { get => Settings.UiTextColor; set => SetStyleSetting(Settings.UiTextColor, value, v => Settings.UiTextColor = v); }
    public string UiMutedTextColor { get => Settings.UiMutedTextColor; set => SetStyleSetting(Settings.UiMutedTextColor, value, v => Settings.UiMutedTextColor = v); }
    public string UiAccentColor { get => Settings.UiAccentColor; set => SetStyleSetting(Settings.UiAccentColor, value, v => Settings.UiAccentColor = v); }
    public string UiBorderColor { get => Settings.UiBorderColor; set => SetStyleSetting(Settings.UiBorderColor, value, v => Settings.UiBorderColor = v); }
    public string EditorBackgroundColor { get => Settings.EditorBackgroundColor; set => SetStyleSetting(Settings.EditorBackgroundColor, value, v => Settings.EditorBackgroundColor = v); }
    public string ConsoleBackgroundColor { get => Settings.ConsoleBackgroundColor; set => SetStyleSetting(Settings.ConsoleBackgroundColor, value, v => Settings.ConsoleBackgroundColor = v); }
    public string EditorFontFamily { get => Settings.EditorFontFamily; set => SetStyleSetting(Settings.EditorFontFamily, value, v => Settings.EditorFontFamily = v); }
    public double EditorFontSize { get => Settings.EditorFontSize; set => SetStyleNumber(Settings.EditorFontSize, value, v => Settings.EditorFontSize = v); }
    public double ConsoleFontSize { get => Settings.ConsoleFontSize; set => SetStyleNumber(Settings.ConsoleFontSize, value, v => Settings.ConsoleFontSize = v); }
    public string SyntaxColorMode { get => Settings.SyntaxColorMode; set => SetStyleSetting(Settings.SyntaxColorMode, value, v => Settings.SyntaxColorMode = v); }
    public bool DiscordPresenceEnabled
    {
        get => Settings.DiscordPresenceEnabled;
        set
        {
            if (Settings.DiscordPresenceEnabled == value) return;
            Settings.DiscordPresenceEnabled = value;
            OnPropertyChanged();
        }
    }
    public string DiscordDetailsMode
    {
        get => Settings.DiscordDetailsMode;
        set => SetPlainSetting(Settings.DiscordDetailsMode, value, v => Settings.DiscordDetailsMode = v);
    }
    public string DiscordStateMode
    {
        get => Settings.DiscordStateMode;
        set => SetPlainSetting(Settings.DiscordStateMode, value, v => Settings.DiscordStateMode = v);
    }
    public string DiscordCustomDetails
    {
        get => Settings.DiscordCustomDetails;
        set => SetPlainSetting(Settings.DiscordCustomDetails, value, v => Settings.DiscordCustomDetails = v);
    }
    public string DiscordCustomState
    {
        get => Settings.DiscordCustomState;
        set => SetPlainSetting(Settings.DiscordCustomState, value, v => Settings.DiscordCustomState = v);
    }
    public string LastRunStatus { get => _lastRunStatus; set { _lastRunStatus = value; OnPropertyChanged(); } }
    public string LastRunDurationText { get => _lastRunDurationText; set { _lastRunDurationText = value; OnPropertyChanged(); } }
    public Brush LastRunStatusBrush { get => _lastRunStatusBrush; set { _lastRunStatusBrush = value; OnPropertyChanged(); } }
    public bool IsCurrentFavaFile => IsFavaFile(_currentFile);
    public bool IsExecutionRunning
    {
        get => _isExecutionRunning;
        private set
        {
            if (_isExecutionRunning == value)
                return;
            _isExecutionRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanStopExecution));
            RunCurrentCommand.RaiseCanExecuteChanged();
            StartDebugCommand.RaiseCanExecuteChanged();
            StopExecutionCommand.RaiseCanExecuteChanged();
            SendConsoleInputCommand.RaiseCanExecuteChanged();
            DebugStopCommand.RaiseCanExecuteChanged();
            if (!value)
                IsConsoleAcceptingInput = false;
        }
    }
    public bool CanStopExecution => IsExecutionRunning || CanDebugStop;
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
    public RelayCommand CloseProjectCommand { get; }
    public RelayCommand DeleteProjectCommand { get; }
    public RelayCommand RemoveRecentProjectCommand { get; }
    public RelayCommand DeleteRecentProjectCommand { get; }
    public RelayCommand NewFavaFileCommand { get; }
    public RelayCommand NewTextFileCommand { get; }
    public RelayCommand NewDirectoryCommand { get; }
    public RelayCommand DeleteNodeCommand { get; }
    public RelayCommand OpenNodeInExplorerCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand RunCurrentCommand { get; }
    public RelayCommand StopExecutionCommand { get; }
    public RelayCommand SendConsoleInputCommand { get; }
    public RelayCommand CopyOutputCommand { get; }
    public RelayCommand ClearOutputCommand { get; }
    public RelayCommand ToggleDiagnosticsCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }
    public RelayCommand BackToEditorCommand { get; }
    public RelayCommand BrowseJavaPathCommand { get; }
    public RelayCommand BrowseCompilerRootCommand { get; }
    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand ApplyStylePresetCommand { get; }
    public RelayCommand ChooseStyleColorCommand { get; }
    public RelayCommand ClearRecentProjectsCommand { get; }
    public RelayCommand ClearRecentFilesCommand { get; }
    public RelayCommand ResetIdeDataCommand { get; }
    public RelayCommand CancelProjectDeleteCommand { get; }
    public RelayCommand RemoveProjectReferenceConfirmCommand { get; }
    public RelayCommand DeleteProjectFolderConfirmCommand { get; }
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

        _liveCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LiveCheckDebounceMilliseconds) };
        _liveCheckTimer.Tick += async (_, _) =>
        {
            _liveCheckTimer.Stop();
            await RunLiveCheckAsync();
        };
        _editor.TextChanged += (_, _) =>
        {
            if (_suppressDirtyTracking)
                return;

            if (_selectedEditorTab is not null)
            {
                _selectedEditorTab.Content = _editor.Text;
                _selectedEditorTab.IsDirty = true;
                _currentFile = _selectedEditorTab.FilePath;
                _hasUnsavedChanges = true;
                OnPropertyChanged(nameof(CurrentFileName));
            }

            _liveCheckRevision++;
            ScheduleLiveCheck();
        };

        OpenProjectCommand = new RelayCommand(_ => OpenProject());
        CreateProjectCommand = new RelayCommand(_ => CreateProject());
        CloseProjectCommand = new RelayCommand(_ => CloseProject(), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        DeleteProjectCommand = new RelayCommand(p => DeleteProject(p as string), p => !string.IsNullOrWhiteSpace((p as string) ?? Settings.ProjectRoot));
        RemoveRecentProjectCommand = new RelayCommand(p => RemoveProjectReference(p as string), p => !string.IsNullOrWhiteSpace(p as string));
        DeleteRecentProjectCommand = new RelayCommand(p => DeleteProject(p as string), p => !string.IsNullOrWhiteSpace(p as string));
        NewFavaFileCommand = new RelayCommand(node => NewFile(".fava", node as ProjectNode), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        NewTextFileCommand = new RelayCommand(node => NewFile(".txt", node as ProjectNode), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        NewDirectoryCommand = new RelayCommand(node => NewDirectory(node as ProjectNode), _ => !string.IsNullOrWhiteSpace(Settings.ProjectRoot));
        DeleteNodeCommand = new RelayCommand(n => DeleteNode(n as ProjectNode), n => n is ProjectNode);
        OpenNodeInExplorerCommand = new RelayCommand(n => OpenNodeInExplorer(n as ProjectNode), n => n is ProjectNode);
        SaveFileCommand = new RelayCommand(_ => SaveFile());
        RunCurrentCommand = new RelayCommand(_ => RunCurrentFile(), _ => IsCurrentFavaFile && !IsExecutionRunning);
        StopExecutionCommand = new RelayCommand(_ => StopExecution(), _ => CanStopExecution);
        SendConsoleInputCommand = new RelayCommand(_ => SendConsoleInput(), _ => IsExecutionRunning && _consoleInputWriter != null);
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
        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        ApplyStylePresetCommand = new RelayCommand(p => ApplyStylePreset(p as string));
        ChooseStyleColorCommand = new RelayCommand(p => ChooseStyleColor(p as string), p => !string.IsNullOrWhiteSpace(p as string));
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
        ResetIdeDataCommand = new RelayCommand(_ => ResetIdeData());
        CancelProjectDeleteCommand = new RelayCommand(_ => HideProjectDeleteDialog());
        RemoveProjectReferenceConfirmCommand = new RelayCommand(_ => ConfirmRemoveProjectReference(), _ => IsProjectDeleteDialogVisible);
        DeleteProjectFolderConfirmCommand = new RelayCommand(_ => ConfirmDeleteProjectFolder(), _ => IsProjectDeleteDialogVisible);

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

        StartDebugCommand = new RelayCommand(_ => StartDebug(), _ => IsCurrentFavaFile && !IsExecutionRunning);
        DebugStepCommand = new RelayCommand(_ => DebugStep(), _ => CanDebugStep);
        DebugContinueCommand = new RelayCommand(_ => DebugContinue(), _ => CanDebugContinue);
        DebugJumpToCallCommand = new RelayCommand(_ => DebugJumpToCall(), _ => CanDebugJumpToCall);
        DebugStepBackCommand = new RelayCommand(_ => DebugStepBack(), _ => CanDebugBack);
        DebugStopCommand = new RelayCommand(_ => StopExecution(), _ => CanStopExecution);
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
                if (!TryWriteTextToExistingPath(tab.FilePath, tab.Content, showDialog: true))
                    return false;
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

    private bool CloseTabsForDeletedPath(string path)
    {
        if (_selectedEditorTab is not null)
        {
            _selectedEditorTab.Content = _editor.Text;
            _selectedEditorTab.IsDirty = _hasUnsavedChanges;
        }

        var tabsToClose = OpenEditorTabs
            .Where(tab => IsSameOrInsidePath(tab.FilePath, path))
            .ToList();

        if (tabsToClose.Count == 0)
            return true;

        var dirtyTabs = tabsToClose.Where(tab => tab.IsDirty).ToList();
        if (dirtyTabs.Count > 0)
        {
            var answer = MessageBox.Show(
                dirtyTabs.Count == 1
                    ? $"Discard unsaved changes in '{dirtyTabs[0].FileName}' before deleting?"
                    : $"Discard unsaved changes in {dirtyTabs.Count} open files before deleting?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
                return false;
        }

        foreach (var tab in tabsToClose)
            RemoveEditorTabWithoutPrompt(tab);

        return true;
    }

    private void RemoveEditorTabWithoutPrompt(EditorTab tab)
    {
        var removedIndex = OpenEditorTabs.IndexOf(tab);
        if (removedIndex < 0)
            return;

        var wasSelected = ReferenceEquals(_selectedEditorTab, tab);
        OpenEditorTabs.Remove(tab);
        _currentTestTabPaths.Remove(tab.FilePath);

        if (!wasSelected)
            return;

        _hasUnsavedChanges = false;
        var newSelectedTab = OpenEditorTabs.Count == 0
            ? null
            : OpenEditorTabs[Math.Clamp(removedIndex, 0, OpenEditorTabs.Count - 1)];

        SelectEditorTab(newSelectedTab);
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
        _liveCheckTimer.Stop();

        _suppressDirtyTracking = true;
        _editor.Text = tab?.Content ?? "";
        _suppressDirtyTracking = false;

        OnPropertyChanged(nameof(SelectedEditorTab));
        OnPropertyChanged(nameof(CurrentFileName));
        OnPropertyChanged(nameof(IsCurrentFavaFile));
        if (!IsCurrentFavaFile)
        {
            Diagnostics.Clear();
            OnPropertyChanged(nameof(DiagnosticsHeader));
        }
        RunCurrentCommand.RaiseCanExecuteChanged();
        StartDebugCommand.RaiseCanExecuteChanged();
        CloseProjectCommand.RaiseCanExecuteChanged();
        DeleteProjectCommand.RaiseCanExecuteChanged();
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
        var request = ShowProjectCreationDialog();
        if (request is null)
            return;

        var baseFolder = request.ParentFolder;
        if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder)) return;

        var projectRoot = Path.Combine(request.ParentFolder, request.FolderName);
        if (Directory.Exists(projectRoot))
        {
            StatusText = $"Project already exists: {projectRoot}";
            StatusColor = Brushes.Orange;
            return;
        }

        if (HasProjectNameConflict(projectRoot, out var conflictingProject))
        {
            StatusText = $"Project name '{GetProjectName(projectRoot)}' is already used by {conflictingProject}.";
            StatusColor = Brushes.Orange;
            return;
        }

        Directory.CreateDirectory(projectRoot);
        Settings.ProjectMetadata[projectRoot] = new ProjectMetadata
        {
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        CreateStarterProjectFiles(projectRoot, request.Title, request.TemplateKey);
        Settings.Save();
        LoadProject(projectRoot);
        StatusText = $"Created project: {request.Title}";
        StatusColor = Brushes.LightBlue;
    }

    private static ProjectCreationRequest? ShowProjectCreationDialog()
    {
        var parentFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FavaStudio", "Projects");
        var titleBox = new TextBox { Text = "My Fava Project", Margin = new Thickness(0, 4, 0, 10) };
        var folderBox = new TextBox { Text = "FavaProject", Margin = new Thickness(0, 4, 0, 10), FontFamily = new FontFamily("Consolas") };
        var parentFolderBox = new TextBox { Text = parentFolder, Margin = new Thickness(0, 4, 0, 10), FontFamily = new FontFamily("Consolas") };
        var templateBox = new ComboBox
        {
            ItemsSource = ProjectTemplateOptions,
            DisplayMemberPath = nameof(ProjectTemplateOption.Label),
            SelectedValuePath = nameof(ProjectTemplateOption.Key),
            SelectedIndex = 0,
            Margin = new Thickness(0, 4, 0, 4)
        };
        var templateDescription = new TextBlock
        {
            Text = ProjectTemplateOptions[0].Description,
            Foreground = Brushes.Gray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var descriptionBox = new TextBox
        {
            Text = "A new Fava project.",
            Margin = new Thickness(0, 4, 0, 0),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 82,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var errorText = new TextBlock
        {
            Foreground = Brushes.IndianRed,
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };

        titleBox.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(titleBox.Text) && folderBox.Text == "FavaProject")
                folderBox.Text = SanitizeProjectFolderName(titleBox.Text);
        };
        templateBox.SelectionChanged += (_, _) =>
        {
            if (templateBox.SelectedItem is ProjectTemplateOption option)
                templateDescription.Text = option.Description;
        };

        var panel = new StackPanel { Margin = new Thickness(18) };
        panel.Children.Add(new TextBlock { Text = "Create Project", FontSize = 20, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 12) });
        panel.Children.Add(new TextBlock { Text = "Project title", Foreground = Brushes.LightGray });
        panel.Children.Add(titleBox);
        panel.Children.Add(new TextBlock { Text = "Folder name", Foreground = Brushes.LightGray });
        panel.Children.Add(folderBox);
        panel.Children.Add(new TextBlock { Text = "Create inside", Foreground = Brushes.LightGray });
        var parentPanel = new DockPanel { Margin = new Thickness(0, 4, 0, 10) };
        var browseParent = new Button { Content = "Browse", Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(8, 0, 0, 0) };
        DockPanel.SetDock(browseParent, Dock.Right);
        parentPanel.Children.Add(browseParent);
        parentPanel.Children.Add(parentFolderBox);
        panel.Children.Add(parentPanel);
        panel.Children.Add(new TextBlock { Text = "Starter files", Foreground = Brushes.LightGray });
        panel.Children.Add(templateBox);
        panel.Children.Add(templateDescription);
        panel.Children.Add(new TextBlock { Text = "Description", Foreground = Brushes.LightGray });
        panel.Children.Add(descriptionBox);
        panel.Children.Add(errorText);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
        var create = new Button { Content = "Create", MinWidth = 86, Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Cancel", MinWidth = 86, Padding = new Thickness(14, 7, 14, 7) };
        buttons.Children.Add(create);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);

        var dialog = new Window
        {
            Title = "Create Fava Project",
            Content = panel,
            Width = 460,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = new SolidColorBrush(Color.FromRgb(30, 31, 34)),
            Foreground = Brushes.White,
            Owner = Application.Current.MainWindow
        };

        browseParent.Click += (_, _) =>
        {
            var folderDialog = new OpenFolderDialog { Title = "Select Parent Folder for New Project" };
            if (Directory.Exists(parentFolderBox.Text))
                folderDialog.InitialDirectory = parentFolderBox.Text;
            if (folderDialog.ShowDialog() == true)
                parentFolderBox.Text = folderDialog.FolderName;
        };

        create.Click += (_, _) =>
        {
            var title = titleBox.Text.Trim();
            var folder = SanitizeProjectFolderName(folderBox.Text);
            var selectedParent = string.IsNullOrWhiteSpace(parentFolderBox.Text)
                ? parentFolder
                : parentFolderBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                errorText.Text = "Project title is required.";
                return;
            }
            if (string.IsNullOrWhiteSpace(folder))
            {
                errorText.Text = "Folder name is required.";
                return;
            }
            if (!string.Equals(folder, folderBox.Text.Trim(), StringComparison.Ordinal))
            {
                folderBox.Text = folder;
                errorText.Text = "Folder name was cleaned up. Press Create again if it looks right.";
                return;
            }

            try
            {
                Directory.CreateDirectory(selectedParent);
            }
            catch (Exception ex)
            {
                errorText.Text = $"Could not create parent folder: {ex.Message}";
                return;
            }

            var templateKey = (templateBox.SelectedValue as string) ?? "tutorial";
            dialog.Tag = new ProjectCreationRequest(selectedParent, folder, title, descriptionBox.Text.Trim(), templateKey);
            dialog.DialogResult = true;
        };
        cancel.Click += (_, _) => dialog.DialogResult = false;

        return dialog.ShowDialog() == true ? dialog.Tag as ProjectCreationRequest : null;
    }

    private static string SanitizeProjectFolderName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var cleaned = new string(value.Trim().Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        cleaned = Regex.Replace(cleaned, @"\s+", "-");
        cleaned = Regex.Replace(cleaned, @"-+", "-").Trim('-', '.', ' ');
        return string.IsNullOrWhiteSpace(cleaned) ? "FavaProject" : cleaned;
    }

    private static void CreateStarterProjectFiles(string projectRoot, string projectTitle, string templateKey)
    {
        var safeTitle = projectTitle.Replace("\"", "'");
        var mainPath = Path.Combine(projectRoot, "main.fava");
        var notesPath = Path.Combine(projectRoot, "README.txt");

        if (templateKey == "tutorial")
            File.WriteAllText(mainPath, BuildFeatureShowcaseSource(safeTitle));
        else if (templateKey == "hello")
            File.WriteAllText(mainPath, """
function main() {
    print("Hello World!");
}
""");

        File.WriteAllText(notesPath,
            $"# {safeTitle}{Environment.NewLine}{Environment.NewLine}" +
            (templateKey == "tutorial"
                ? "Start with main.fava. It demonstrates Fava variables, arrays, strings, functions, loops, file calls, time calls, casting, and input helpers."
                : templateKey == "hello"
                    ? "Start with main.fava. It contains a minimal Hello World program."
                    : "This project starts empty. Create a .fava file when you are ready."));
    }

    private static string BuildFeatureShowcaseSource(string projectTitle) => $$"""
// Fava language feature showcase for "{{projectTitle}}".
// The interactiveDemo function shows Read(...) without pausing the normal run.

function add(integer left, integer right) -> integer {
    return left + right;
}

function describeScore(integer score) -> string {
    if (score >= 90) {
        return "excellent";
    } else {
        if (score >= 70) {
            return "solid";
        } else {
            return "practice";
        }
    }
}

function interactiveDemo() {
    string name := Read("Type your name: ");
    string ageText := Read("Type your age: ");
    integer age := ToInteger(ageText);
    print("Hello " || name || ", next year you will be " || (age + 1));
}

function main() {
    print("== Fava feature showcase ==");

    integer count := 3;
    real price := 2.5 + count;
    bool ready := true and not false;
    string word := "Fava";
    print("numbers: " || count || ", " || price || ", " || ready);

    integer[] scores := new integer[3];
    scores[0] := 95;
    scores[1] := 82;
    scores[2] := 64;
    print("scores length: " || Length(scores));
    print("word length: " || Length(word));
    print("first character: " || word[0]);

    for (integer i := 0; i < Length(scores); i++) {
        print("score " || i || " is " || scores[i] || " -> " || describeScore(scores[i]));
    }

    for letter in word {
        print("letter: " || letter);
    }

    integer total := 0;
    integer index := 0;
    while (index < Length(scores)) {
        total := total + scores[index];
        index := index + 1;
    }
    print("total score: " || total);
    print("add helper: " || add(10, 20));

    string message := "Fava Studio";
    print("substring: " || Substring(message, 0, 4));
    print("replace: " || Replace(message, "Studio", "Language"));
    print("concat: " || ("Project: " || "{{projectTitle}}"));

    integer parsedInteger := ToInteger("42");
    real parsedReal := ToReal("3.14");
    bool parsedBool := ToBool("true");
    real promoted := ToReal(parsedInteger);
    integer rounded := ToInteger(9.8);
    print("casts: " || parsedInteger || ", " || parsedReal || ", " || parsedBool || ", " || promoted || ", " || rounded);

    print("random 1..6: " || RandomInt(1, 6));
    print("utc date: " || Now("date"));
    print("utc time: " || Now("time"));
    Sleep(10);

    string fileName := "showcase_output.txt";
    CreateFile(fileName);
    WriteFile(fileName, "Created by Fava.\n");
    AppendFile(fileName, "Total score: " || total || "\n");
    if (FileExists(fileName)) {
        print("file contents:");
        print(ReadFile(fileName));
    }
    DeleteFile(fileName);

    print("== done ==");
}
""";

    private void LoadProject(string folder, bool skipUnsavedCheck = false)
    {
        if (HasProjectNameConflict(folder, out var conflictingProject))
        {
            StatusText = $"Project name '{GetProjectName(folder)}' is already used by {conflictingProject}.";
            StatusColor = Brushes.Orange;
            return;
        }

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
            var root = BuildNode(folder, isRoot: true);
            ProjectTree.Add(root);
            Settings.ProjectRoot = folder;
            AddRecentProject(folder);
            ConfigureProjectTestFolders(folder, createIfMissing: false);
            RefreshRecentCollections();
            RefreshTestSuiteCases();
            IsWelcomeViewVisible = false;
            BackToEditor();
            StatusText = $"Loaded project: {folder}";
            StatusColor = Brushes.LightBlue;
            OnPropertyChanged(nameof(CurrentProjectDirectory));
            OnPropertyChanged(nameof(IsWorkspaceVisible));
            RaiseSettingsValidationChanged();
            CreateTestPairCommand.RaiseCanExecuteChanged();
            CloseProjectCommand.RaiseCanExecuteChanged();
            DeleteProjectCommand.RaiseCanExecuteChanged();

            RestoreExpandedPaths(root, expandedPaths);
            OpenInitialProjectFile(root, folder, selectedPath);
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to load project: {ex.Message}";
            StatusColor = Brushes.IndianRed;
        }
    }

    private ProjectNode BuildNode(string path, bool isRoot = false)
    {
        var isDirectory = Directory.Exists(path);
        var name = Path.GetFileName(path);
        var node = new ProjectNode
        {
            Name = isRoot ? GetProjectDisplayName(path) : string.IsNullOrWhiteSpace(name) ? path : name,
            FullPath = path,
            IsDirectory = isDirectory,
            IsRoot = isRoot
        };

        if (!isDirectory) return node;

        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
            node.Children.Add(BuildNode(dir));

        foreach (var file in Directory.GetFiles(path).OrderBy(f => f))
            node.Children.Add(BuildNode(file));

        return node;
    }

    private void RefreshProjectTree(string? selectedPath = null, string? parentPath = null)
    {
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot) || !Directory.Exists(Settings.ProjectRoot))
            return;

        var expandedPaths = CaptureExpandedPaths(ProjectTree.FirstOrDefault());
        AddAncestorPaths(expandedPaths, selectedPath);
        AddAncestorPaths(expandedPaths, parentPath);
        ProjectTree.Clear();
        var root = BuildNode(Settings.ProjectRoot, isRoot: true);
        ProjectTree.Add(root);
        RestoreExpandedPaths(root, expandedPaths, expandRoot: true);
    }

    private bool AddProjectNodeInPlace(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        var parentNode = string.IsNullOrWhiteSpace(parentPath)
            ? null
            : FindNodeByPath(ProjectTree.FirstOrDefault(), parentPath);
        if (parentNode is null || !parentNode.IsDirectory)
            return false;

        if (FindNodeByPath(parentNode, path) is not null)
            return true;

        var newNode = BuildNode(path);
        var insertIndex = 0;
        while (insertIndex < parentNode.Children.Count && ShouldSortBefore(parentNode.Children[insertIndex], newNode))
            insertIndex++;

        parentNode.Children.Insert(insertIndex, newNode);
        parentNode.IsExpanded = true;
        return true;
    }

    private bool RemoveProjectNodeInPlace(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        var parentNode = string.IsNullOrWhiteSpace(parentPath)
            ? null
            : FindNodeByPath(ProjectTree.FirstOrDefault(), parentPath);
        if (parentNode is null)
            return false;

        var node = parentNode.Children.FirstOrDefault(child => string.Equals(child.FullPath, path, StringComparison.OrdinalIgnoreCase));
        if (node is null)
            return false;

        parentNode.Children.Remove(node);
        return true;
    }

    private static bool ShouldSortBefore(ProjectNode existing, ProjectNode incoming)
    {
        if (existing.IsDirectory != incoming.IsDirectory)
            return existing.IsDirectory;
        return string.Compare(existing.Name, incoming.Name, StringComparison.OrdinalIgnoreCase) < 0;
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
        if (!AddProjectNodeInPlace(dialog.FileName))
            RefreshProjectTree(dialog.FileName, parentPath: Path.GetDirectoryName(dialog.FileName));
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
        if (!AddProjectNodeInPlace(candidate))
            RefreshProjectTree(candidate, parentPath: candidate);
        var createdNode = FindNodeByPath(ProjectTree.FirstOrDefault(), candidate);
        if (createdNode is not null)
            createdNode.IsExpanded = true;
        StatusText = $"Created directory: {Path.GetFileName(candidate)}";
        StatusColor = Brushes.LightGreen;
    }

    private void DeleteNode(ProjectNode? node)
    {
        if (node is null || string.IsNullOrWhiteSpace(node.FullPath)) return;
        if (node.FullPath == Settings.ProjectRoot)
        {
            DeleteProject(Settings.ProjectRoot);
            return;
        }

        var answer = MessageBox.Show($"Delete '{node.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;
        if (!CloseTabsForDeletedPath(node.FullPath)) return;

        if (node.IsDirectory && Directory.Exists(node.FullPath))
            Directory.Delete(node.FullPath, true);
        else if (File.Exists(node.FullPath))
            File.Delete(node.FullPath);

        if (!RemoveProjectNodeInPlace(node.FullPath))
            RefreshProjectTree(parentPath: Path.GetDirectoryName(node.FullPath));
    }

    private void OpenNodeInExplorer(ProjectNode? node)
    {
        if (node is null || string.IsNullOrWhiteSpace(node.FullPath))
            return;

        var path = node.FullPath;
        var psi = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            UseShellExecute = true
        };

        if (File.Exists(path))
            psi.Arguments = $"/select,\"{path}\"";
        else if (Directory.Exists(path))
            psi.Arguments = $"\"{path}\"";
        else
            return;

        Process.Start(psi);
    }

    private void CloseProject()
    {
        if (!TryResolveUnsavedChanges())
            return;

        ClearLoadedProject();
        IsWelcomeViewVisible = true;
        StatusText = "Project closed.";
        StatusColor = Brushes.LightGray;
    }

    private void DeleteProject(string? projectPath)
    {
        projectPath = string.IsNullOrWhiteSpace(projectPath) ? Settings.ProjectRoot : projectPath;
        if (string.IsNullOrWhiteSpace(projectPath))
            return;
        if (!Directory.Exists(projectPath))
        {
            RemoveProjectReference(projectPath);
            return;
        }

        PendingDeleteProjectPath = projectPath;
        PendingDeleteProjectName = GetProjectName(projectPath);
        IsProjectDeleteDialogVisible = true;
        RemoveProjectReferenceConfirmCommand.RaiseCanExecuteChanged();
        DeleteProjectFolderConfirmCommand.RaiseCanExecuteChanged();
    }

    private void HideProjectDeleteDialog()
    {
        IsProjectDeleteDialogVisible = false;
        PendingDeleteProjectPath = "";
        PendingDeleteProjectName = "";
        RemoveProjectReferenceConfirmCommand.RaiseCanExecuteChanged();
        DeleteProjectFolderConfirmCommand.RaiseCanExecuteChanged();
    }

    private void ConfirmRemoveProjectReference()
    {
        var projectPath = PendingDeleteProjectPath;
        HideProjectDeleteDialog();
        RemoveProjectReference(projectPath);
    }

    private void ConfirmDeleteProjectFolder()
    {
        var projectPath = PendingDeleteProjectPath;
        var projectName = PendingDeleteProjectName;
        HideProjectDeleteDialog();
        if (string.IsNullOrWhiteSpace(projectPath))
            return;

        try
        {
            var fullProjectPath = Path.GetFullPath(projectPath);
            var wasCurrentProject = IsSamePath(fullProjectPath, Settings.ProjectRoot);

            if (wasCurrentProject)
            {
                if (!TryResolveUnsavedChanges())
                    return;
                ClearLoadedProject();
            }

            Directory.Delete(fullProjectPath, true);
            RemoveProjectData(fullProjectPath);
            Settings.Save();
            RefreshRecentCollections();
            IsWelcomeViewVisible = true;
            StatusText = $"Deleted project: {projectName}";
            StatusColor = Brushes.Orange;
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to delete project: {ex.Message}";
            StatusColor = Brushes.IndianRed;
        }
    }

    private void RemoveProjectReference(string? projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            return;

        var wasCurrentProject = !string.IsNullOrWhiteSpace(Settings.ProjectRoot)
            && string.Equals(Path.GetFullPath(projectPath), Path.GetFullPath(Settings.ProjectRoot), StringComparison.OrdinalIgnoreCase);

        if (wasCurrentProject)
        {
            if (!TryResolveUnsavedChanges())
                return;
            ClearLoadedProject();
            IsWelcomeViewVisible = true;
        }

        RemoveProjectData(projectPath);
        Settings.Save();
        RefreshRecentCollections();
        StatusText = $"Removed project from Fava Studio: {GetProjectName(projectPath)}";
        StatusColor = Brushes.LightGray;
    }

    private void RemoveProjectData(string projectPath)
    {
        Settings.RecentProjects.RemoveAll(path => IsSamePath(path, projectPath));
        Settings.RecentFiles.RemoveAll(path => IsSameOrInsidePathSafe(path, projectPath));

        foreach (var key in Settings.ProjectMetadata.Keys.Where(key => IsSamePath(key, projectPath)).ToList())
            Settings.ProjectMetadata.Remove(key);
    }

    private void ClearLoadedProject()
    {
        ProjectTree.Clear();
        TestResults.Clear();
        ToolTestPairs.Clear();
        OpenEditorTabs.Clear();
        QuickOpenResults.Clear();
        SelectedProjectNode = null;
        SelectedTestResult = null;
        SelectedToolTestPair = null;
        _currentTestTabPaths.Clear();
        _selectedEditorTab = null;
        _currentFile = null;
        _hasUnsavedChanges = false;
        _suppressDirtyTracking = true;
        _editor.Text = "";
        _suppressDirtyTracking = false;
        Settings.ProjectRoot = "";
        Settings.InputsDir = "";
        Settings.OutputsDir = "";
        Settings.Save();
        TestSummary = "No tests run yet.";
        OnPropertyChanged(nameof(SelectedEditorTab));
        OnPropertyChanged(nameof(CurrentFileName));
        OnPropertyChanged(nameof(CurrentProjectDirectory));
        RaiseSettingsValidationChanged();
        RunCurrentCommand.RaiseCanExecuteChanged();
        StartDebugCommand.RaiseCanExecuteChanged();
        CloseProjectCommand.RaiseCanExecuteChanged();
        DeleteProjectCommand.RaiseCanExecuteChanged();
        CreateTestPairCommand.RaiseCanExecuteChanged();
    }

    private void SaveFile()
    {
        var activeTab = _selectedEditorTab;
        if (activeTab is null || string.IsNullOrWhiteSpace(activeTab.FilePath))
            return;

        activeTab.Content = _editor.Text;
        if (!TryWriteTextToExistingPath(activeTab.FilePath, activeTab.Content, showDialog: true))
            return;
        activeTab.IsDirty = false;
        _currentFile = activeTab.FilePath;
        AddRecentFile(activeTab.FilePath);
        RefreshRecentCollections();
        _hasUnsavedChanges = false;
        OnPropertyChanged(nameof(CurrentFileName));
        StatusText = $"Saved: {activeTab.FilePath}";
        StatusColor = Brushes.LightGreen;
    }

    private bool TryWriteTextToExistingPath(string path, string content, bool showDialog)
    {
        if (!CanWriteToExistingFolder(path, out var message))
        {
            StatusText = message;
            StatusColor = Brushes.IndianRed;
            if (showDialog)
                MessageBox.Show(message, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        try
        {
            FileService.WriteText(path, content);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var error = $"Could not save '{Path.GetFileName(path)}': {ex.Message}";
            StatusText = error;
            StatusColor = Brushes.IndianRed;
            if (showDialog)
                MessageBox.Show(error, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private static bool CanWriteToExistingFolder(string path, out string message)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            message = $"Cannot save '{Path.GetFileName(path)}' because its folder no longer exists.";
            return false;
        }

        message = "";
        return true;
    }

    private void ScheduleLiveCheck()
    {
        _liveCheckTimer.Stop();
        if (IsCurrentFavaFile)
            _liveCheckTimer.Start();
    }

    private async Task RunLiveCheckAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;
        if (_isLiveChecking)
        {
            ScheduleLiveCheck();
            return;
        }

        if (!IsCurrentFavaFile)
        {
            Diagnostics.Clear();
            OnPropertyChanged(nameof(DiagnosticsHeader));
            return;
        }
        if (!CanRunCompiler(showStatus: false)) return;

        var checkFile = _currentFile;
        var checkText = _editor.Text;
        var checkRevision = _liveCheckRevision;

        if (string.Equals(_lastLiveCheckedFile, checkFile, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_lastLiveCheckedText, checkText, StringComparison.Ordinal))
            return;

        _isLiveChecking = true;
        try
        {
            if (!File.Exists(checkFile) || !CanWriteToExistingFolder(checkFile, out _))
            {
                Diagnostics.Clear();
                OnPropertyChanged(nameof(DiagnosticsHeader));
                StatusText = "Live check skipped: the current file no longer exists.";
                StatusColor = Brushes.Orange;
                return;
            }

            if (!TryWriteTextToExistingPath(checkFile, checkText, showDialog: false))
                return;

            var runner = new JavaCompilerService(Settings);
            var result = await runner.RunFileAsync(checkFile, checkOnly: true);

            UpdateCompilerMetadata(result.Output);
            var diagnostics = DiagnosticsParser.Parse(result.Output, checkText)
                .Concat(DeadCodeAnalyzer.Analyze(checkText, _hoverInfos))
                .ToList();

            if (checkRevision != _liveCheckRevision ||
                !string.Equals(checkFile, _currentFile, StringComparison.OrdinalIgnoreCase))
                return;

            Diagnostics.Clear();
            foreach (var d in diagnostics) Diagnostics.Add(d);
            OnPropertyChanged(nameof(DiagnosticsHeader));
            _lastLiveCheckedFile = checkFile;
            _lastLiveCheckedText = checkText;

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
        catch (IOException ex)
        {
            Diagnostics.Clear();
            OnPropertyChanged(nameof(DiagnosticsHeader));
            StatusText = $"Live check skipped: {ex.Message}";
            StatusColor = Brushes.Orange;
        }
        catch (UnauthorizedAccessException ex)
        {
            Diagnostics.Clear();
            OnPropertyChanged(nameof(DiagnosticsHeader));
            StatusText = $"Live check skipped: {ex.Message}";
            StatusColor = Brushes.Orange;
        }
        finally
        {
            _isLiveChecking = false;
            if (checkRevision != _liveCheckRevision)
                ScheduleLiveCheck();
        }
    }

    private async void RunCurrentFile()
    {
        if (string.IsNullOrWhiteSpace(_currentFile)) return;
        if (!IsCurrentFavaFile)
        {
            StatusText = "Only .fava files can be run.";
            StatusColor = Brushes.Orange;
            return;
        }
        if (!CanRunCompiler(showStatus: true)) return;
        SaveFile();
        _executionCancellationSource?.Dispose();
        _executionCancellationSource = new CancellationTokenSource();
        var cancellationToken = _executionCancellationSource.Token;
        IsExecutionRunning = true;
        StatusText = "Running…";
        StatusColor = Brushes.LightGray;
        LastRunStatus = $"Running {Path.GetFileName(_currentFile)}";
        LastRunDurationText = "--";
        LastRunStatusBrush = Brushes.LightGray;
        _consoleInputWriter = null;
        IsConsoleAcceptingInput = false;
        SendConsoleInputCommand.RaiseCanExecuteChanged();

        var runner = new JavaCompilerService(Settings);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await runner.RunFileAsync(
                _currentFile,
                onOutputChanged: output => Application.Current.Dispatcher.Invoke(() => UpdateOutputs(output)),
                onInputWriterChanged: writer => Application.Current.Dispatcher.Invoke(() =>
                {
                    _consoleInputWriter = writer;
                    SendConsoleInputCommand.RaiseCanExecuteChanged();
                }),
                cancellationToken: cancellationToken);
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
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            StatusText = "Execution stopped.";
            StatusColor = Brushes.Orange;
            LastRunStatus = "Run stopped";
            LastRunDurationText = FormatDuration(stopwatch.Elapsed);
            LastRunStatusBrush = Brushes.Orange;
        }
        finally
        {
            _consoleInputWriter = null;
            IsConsoleAcceptingInput = false;
            IsExecutionRunning = false;
            SyncProjectFilesAfterRun();
            _executionCancellationSource?.Dispose();
            _executionCancellationSource = null;
        }
    }

    private void SyncProjectFilesAfterRun()
    {
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot) || !Directory.Exists(Settings.ProjectRoot))
            return;

        if (_selectedEditorTab is not null)
        {
            _selectedEditorTab.Content = _editor.Text;
            _selectedEditorTab.IsDirty = _hasUnsavedChanges;
        }

        var selectedPath = SelectedProjectNode?.FullPath;
        var parentPath = !string.IsNullOrWhiteSpace(_currentFile)
            ? Path.GetDirectoryName(_currentFile)
            : null;

        var vanishedCleanTabs = OpenEditorTabs
            .Where(tab => !tab.IsDirty && !File.Exists(tab.FilePath))
            .ToList();

        foreach (var tab in vanishedCleanTabs)
            RemoveEditorTabWithoutPrompt(tab);

        if (!string.IsNullOrWhiteSpace(selectedPath)
            && !File.Exists(selectedPath)
            && !Directory.Exists(selectedPath))
        {
            selectedPath = null;
        }

        RefreshProjectTree(selectedPath, parentPath);
    }

    private async void SendConsoleInput()
    {
        var writer = _consoleInputWriter;
        if (writer == null)
            return;

        var line = ConsoleInput;
        ConsoleInput = "";
        await writer(line);
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

    private void UpdateOutputs(string fullOutput, bool syncVisualizer = true)
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

        UpdateConsoleInputState(fullOutput);

        if (syncVisualizer && VisualizerAutoSync)
            LoadVisualizerFromSections(ConstantPoolOutput, InstructionsOutput);

        CopyOutputCommand.RaiseCanExecuteChanged();
        ClearOutputCommand.RaiseCanExecuteChanged();
    }

    private void UpdateConsoleInputState(string fullOutput)
    {
        if (!IsExecutionRunning || _consoleInputWriter == null)
        {
            IsConsoleAcceptingInput = false;
            return;
        }

        var vmMarker = "*** VM output ***";
        var vmIndex = fullOutput.LastIndexOf(vmMarker, StringComparison.OrdinalIgnoreCase);
        if (vmIndex < 0)
        {
            IsConsoleAcceptingInput = false;
            return;
        }

        var vmOutput = fullOutput[(vmIndex + vmMarker.Length)..];
        var traceMarker = "*** VM trace ***";
        var traceIndex = vmOutput.IndexOf(traceMarker, StringComparison.OrdinalIgnoreCase);
        if (traceIndex >= 0)
            vmOutput = vmOutput[..traceIndex];

        if (vmOutput.StartsWith("\r\n", StringComparison.Ordinal))
            vmOutput = vmOutput[2..];
        else if (vmOutput.StartsWith("\n", StringComparison.Ordinal))
            vmOutput = vmOutput[1..];

        IsConsoleAcceptingInput = vmOutput.Length > 0
            && !vmOutput.EndsWith("\n", StringComparison.Ordinal)
            && !vmOutput.EndsWith("\r", StringComparison.Ordinal);
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
        IsConsoleAcceptingInput = false;
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
        if (!IsCurrentFavaFile)
        {
            StatusText = "Only .fava files can be debugged.";
            StatusColor = Brushes.Orange;
            return;
        }
        if (!CanRunCompiler(showStatus: true)) return;

        SaveFile();
        _executionCancellationSource?.Dispose();
        _executionCancellationSource = new CancellationTokenSource();
        var cancellationToken = _executionCancellationSource.Token;
        IsExecutionRunning = true;
        StatusText = "Starting debug session…";
        StatusColor = Brushes.LightGray;

        var runner = new JavaCompilerService(Settings);
        try
        {
        var result = await runner.RunFileAsync(_currentFile, includeTrace: true, cancellationToken: cancellationToken);
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
        catch (OperationCanceledException)
        {
            StatusText = "Debug execution stopped.";
            StatusColor = Brushes.Orange;
        }
        finally
        {
            IsExecutionRunning = false;
            _executionCancellationSource?.Dispose();
            _executionCancellationSource = null;
        }
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

    private void StopExecution()
    {
        if (IsExecutionRunning)
        {
            StatusText = "Stopping execution...";
            StatusColor = Brushes.Orange;
            _executionCancellationSource?.Cancel();
            return;
        }

        if (CanDebugStop)
            DebugStop();
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
        OnPropertyChanged(nameof(CanStopExecution));
        StartDebugCommand.RaiseCanExecuteChanged();
        StopExecutionCommand.RaiseCanExecuteChanged();
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

        ConfigureProjectTestFolders(Settings.ProjectRoot, createIfMissing);
    }

    private void ConfigureProjectTestFolders(string projectRoot, bool createIfMissing)
    {
        var inputsDir = Path.Combine(projectRoot, "tests", "inputs");
        var outputsDir = Path.Combine(projectRoot, "tests", "outputs");
        var changed = !string.Equals(Settings.InputsDir, inputsDir, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Settings.OutputsDir, outputsDir, StringComparison.OrdinalIgnoreCase);

        Settings.InputsDir = inputsDir;
        Settings.OutputsDir = outputsDir;

        if (createIfMissing)
        {
            Directory.CreateDirectory(Settings.InputsDir);
            Directory.CreateDirectory(Settings.OutputsDir);
        }

        if (changed)
            Settings.Save();

        RaiseSettingsValidationChanged();
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
        StyleSettingsChanged?.Invoke();
        StatusText = "Settings saved.";
        StatusColor = Brushes.LightGreen;
        RaiseSettingsValidationChanged();
    }

    private void ResetIdeData()
    {
        var answer = MessageBox.Show(
            "Reset Fava Studio IDE data?\n\nThis clears settings, project metadata, recent files, recent projects, style settings, and Discord presence preferences. Project folders on disk are not deleted.",
            "Reset IDE Data",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        ProjectTree.Clear();
        TestResults.Clear();
        ToolTestPairs.Clear();
        OpenEditorTabs.Clear();
        QuickOpenResults.Clear();
        SelectedProjectNode = null;
        SelectedTestResult = null;
        SelectedToolTestPair = null;
        _currentTestTabPaths.Clear();
        _selectedEditorTab = null;
        _currentFile = null;
        _hasUnsavedChanges = false;
        _suppressDirtyTracking = true;
        _editor.Text = "";
        _suppressDirtyTracking = false;

        try
        {
            if (Directory.Exists(SettingsService.DataDirectory))
                Directory.Delete(SettingsService.DataDirectory, true);
        }
        catch
        {
            // If a file is briefly locked, saving below still rewrites a clean settings file.
        }

        Settings.ResetToDefaults();
        Settings.Save();
        RefreshRecentCollections();
        RefreshTestSuiteCases();
        IsWelcomeViewVisible = true;
        IsSettingsViewVisible = false;
        IsToolsViewVisible = false;
        IsVisualizerViewVisible = false;
        TestSummary = "No tests run yet.";
        StatusText = "Fava Studio data reset.";
        StatusColor = Brushes.LightGreen;
        StyleSettingsChanged?.Invoke();
        OnPropertyChanged(nameof(SelectedEditorTab));
        OnPropertyChanged(nameof(CurrentFileName));
        OnPropertyChanged(nameof(CurrentProjectDirectory));
        RaiseSettingsValidationChanged();
        RunCurrentCommand.RaiseCanExecuteChanged();
        StartDebugCommand.RaiseCanExecuteChanged();
        CloseProjectCommand.RaiseCanExecuteChanged();
        DeleteProjectCommand.RaiseCanExecuteChanged();
        CreateTestPairCommand.RaiseCanExecuteChanged();
    }

    private void SetPlainSetting(string currentValue, string newValue, Action<string> assign, [CallerMemberName] string? propertyName = null)
    {
        if (currentValue == newValue) return;
        assign(newValue);
        OnPropertyChanged(propertyName);
    }

    private void SetStyleSetting(string currentValue, string newValue, Action<string> assign, [CallerMemberName] string? propertyName = null)
    {
        if (currentValue == newValue) return;
        assign(newValue);
        OnPropertyChanged(propertyName);
        StyleSettingsChanged?.Invoke();
    }

    private void SetStyleNumber(double currentValue, double newValue, Action<double> assign, [CallerMemberName] string? propertyName = null)
    {
        var clamped = Math.Clamp(newValue, 10, 24);
        if (Math.Abs(currentValue - clamped) < 0.01) return;
        assign(clamped);
        OnPropertyChanged(propertyName);
        StyleSettingsChanged?.Invoke();
    }

    private void ApplyStylePreset(string? preset)
    {
        switch ((preset ?? "default").ToLowerInvariant())
        {
            case "professional":
                SetStyleValues("#1E1F22", "#2B2D30", "#25262A", "#E6EAF0", "#9AA4B2", "#4D8DFF", "#3C3F41", "#1E1F22", "#181A1F", "Consolas", 15, 15);
                break;
            case "graphite":
                SetStyleValues("#17191D", "#24272D", "#1E2127", "#ECEFF4", "#A8B0BE", "#8AB4FF", "#3A404A", "#16181C", "#14161A", "Cascadia Mono", 15, 15);
                break;
            case "ember":
                SetStyleValues("#201B19", "#2D2622", "#261F1C", "#F2ECE6", "#B8AAA0", "#FF9A3D", "#4B3B34", "#1B1715", "#181412", "Consolas", 15, 15);
                break;
            case "crimson":
                SetStyleValues("#100B0D", "#1D1215", "#271619", "#F7ECEE", "#B9959B", "#FF4655", "#56303A", "#0D090B", "#090608", "Cascadia Mono", 15, 15);
                break;
            case "cherryblossom":
                SetStyleValues("#FFF3F7", "#FFFFFF", "#FFE2EC", "#3A2029", "#8D6472", "#FF6FAE", "#F2B7CB", "#FFF9FB", "#FFF1F6", "Cascadia Mono", 15, 15);
                break;
            case "cyberpunk":
                SetStyleValues("#101014", "#1A1722", "#211C2C", "#F7F7FF", "#AAA3C2", "#00E5FF", "#4A3B68", "#0D0D12", "#09090D", "Cascadia Mono", 15, 15);
                break;
            case "matrix":
                SetStyleValues("#07110A", "#0D1B10", "#102515", "#D8FFE0", "#82B98F", "#36FF6A", "#24512D", "#050D07", "#040A05", "Cascadia Mono", 15, 15);
                break;
            case "ocean":
                SetStyleValues("#0A1720", "#132837", "#10222F", "#E3F7FF", "#93B7C8", "#3CC7D9", "#285164", "#07131B", "#061018", "Cascadia Mono", 15, 15);
                break;
            case "royal":
                SetStyleValues("#171225", "#251D38", "#20182F", "#F1ECFF", "#AEA2C9", "#B997FF", "#4A3A66", "#120E1D", "#0F0B18", "Cascadia Mono", 15, 15);
                break;
            case "solar":
                SetStyleValues("#241B0D", "#332512", "#2A1F10", "#FFF4D8", "#C8AA72", "#FFC857", "#5A421F", "#1C150A", "#161007", "Consolas", 15, 15);
                break;
            case "midnight":
                SetStyleValues("#101820", "#1D2833", "#17232D", "#EAF2FA", "#9CB0C3", "#7CC7FF", "#314457", "#0E141B", "#0C1218", "Cascadia Mono", 15, 15);
                break;
            case "orchid":
                SetStyleValues("#1D1824", "#2A2233", "#231C2C", "#F2EAF8", "#B6A8C8", "#D18BFF", "#45344F", "#19131F", "#15101A", "Cascadia Mono", 15, 15);
                break;
            case "daylight":
                SetStyleValues("#F4F6F8", "#FFFFFF", "#E9EEF3", "#18212B", "#5D6B78", "#1E7BD8", "#CBD5DF", "#FFFFFF", "#F8FAFC", "Consolas", 15, 15);
                break;
            case "mint":
            case "default":
            default:
                SetStyleValues("#141C1B", "#20302D", "#1A2927", "#E7F5F1", "#9DB8B0", "#56D6A3", "#314B46", "#111817", "#101615", "Cascadia Mono", 15, 15);
                break;
        }
    }

    private void SetStyleValues(
        string background,
        string panel,
        string panelAlt,
        string text,
        string muted,
        string accent,
        string border,
        string editorBackground,
        string consoleBackground,
        string editorFont,
        double editorSize,
        double consoleSize)
    {
        Settings.UiBackgroundColor = background;
        Settings.UiPanelColor = panel;
        Settings.UiPanelAltColor = panelAlt;
        Settings.UiTextColor = text;
        Settings.UiMutedTextColor = muted;
        Settings.UiAccentColor = accent;
        Settings.UiBorderColor = border;
        Settings.EditorBackgroundColor = editorBackground;
        Settings.ConsoleBackgroundColor = consoleBackground;
        Settings.EditorFontFamily = editorFont;
        Settings.EditorFontSize = editorSize;
        Settings.ConsoleFontSize = consoleSize;

        OnPropertyChanged(nameof(UiBackgroundColor));
        OnPropertyChanged(nameof(UiPanelColor));
        OnPropertyChanged(nameof(UiPanelAltColor));
        OnPropertyChanged(nameof(UiTextColor));
        OnPropertyChanged(nameof(UiMutedTextColor));
        OnPropertyChanged(nameof(UiAccentColor));
        OnPropertyChanged(nameof(UiBorderColor));
        OnPropertyChanged(nameof(EditorBackgroundColor));
        OnPropertyChanged(nameof(ConsoleBackgroundColor));
        OnPropertyChanged(nameof(EditorFontFamily));
        OnPropertyChanged(nameof(EditorFontSize));
        OnPropertyChanged(nameof(ConsoleFontSize));
        OnPropertyChanged(nameof(SyntaxColorMode));
        StyleSettingsChanged?.Invoke();
    }

    private void ChooseStyleColor(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return;

        var current = GetStyleColorValue(propertyName);
        var initial = ParseMediaColor(current);
        var selected = ShowColorPicker(initial, propertyName);
        if (selected is not Color color)
            return;

        SetStyleColorValue(propertyName, $"#{color.R:X2}{color.G:X2}{color.B:X2}");
    }

    private string GetStyleColorValue(string propertyName) => propertyName switch
    {
        nameof(UiBackgroundColor) => UiBackgroundColor,
        nameof(UiPanelColor) => UiPanelColor,
        nameof(UiPanelAltColor) => UiPanelAltColor,
        nameof(UiTextColor) => UiTextColor,
        nameof(UiMutedTextColor) => UiMutedTextColor,
        nameof(UiAccentColor) => UiAccentColor,
        nameof(UiBorderColor) => UiBorderColor,
        nameof(EditorBackgroundColor) => EditorBackgroundColor,
        nameof(ConsoleBackgroundColor) => ConsoleBackgroundColor,
        _ => "#1E1F22"
    };

    private void SetStyleColorValue(string propertyName, string value)
    {
        switch (propertyName)
        {
            case nameof(UiBackgroundColor): UiBackgroundColor = value; break;
            case nameof(UiPanelColor): UiPanelColor = value; break;
            case nameof(UiPanelAltColor): UiPanelAltColor = value; break;
            case nameof(UiTextColor): UiTextColor = value; break;
            case nameof(UiMutedTextColor): UiMutedTextColor = value; break;
            case nameof(UiAccentColor): UiAccentColor = value; break;
            case nameof(UiBorderColor): UiBorderColor = value; break;
            case nameof(EditorBackgroundColor): EditorBackgroundColor = value; break;
            case nameof(ConsoleBackgroundColor): ConsoleBackgroundColor = value; break;
        }
    }

    private static Color ParseMediaColor(string value)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(value.Trim());
        }
        catch
        {
            return Color.FromRgb(30, 31, 34);
        }
    }

    private static Color? ShowColorPicker(Color initial, string title)
    {
        var color = initial;
        var swatch = new Rectangle
        {
            Height = 58,
            RadiusX = 5,
            RadiusY = 5,
            Stroke = new SolidColorBrush(Color.FromRgb(60, 63, 65)),
            StrokeThickness = 1,
            Fill = new SolidColorBrush(color)
        };
        var hexText = new TextBlock
        {
            Text = ToHex(color),
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Consolas"),
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 12)
        };

        Slider CreateSlider(byte value)
        {
            return new Slider
            {
                Minimum = 0,
                Maximum = 255,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Value = value,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        var red = CreateSlider(color.R);
        var green = CreateSlider(color.G);
        var blue = CreateSlider(color.B);

        void Refresh()
        {
            color = Color.FromRgb((byte)red.Value, (byte)green.Value, (byte)blue.Value);
            swatch.Fill = new SolidColorBrush(color);
            hexText.Text = ToHex(color);
        }

        red.ValueChanged += (_, _) => Refresh();
        green.ValueChanged += (_, _) => Refresh();
        blue.ValueChanged += (_, _) => Refresh();

        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(swatch);
        panel.Children.Add(hexText);
        panel.Children.Add(BuildColorSliderRow("R", red));
        panel.Children.Add(BuildColorSliderRow("G", green));
        panel.Children.Add(BuildColorSliderRow("B", blue));

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 14, 0, 0)
        };
        var ok = new Button { Content = "Apply", Padding = new Thickness(14, 6, 14, 6), MinWidth = 78, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Cancel", Padding = new Thickness(14, 6, 14, 6), MinWidth = 78 };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);

        var dialog = new Window
        {
            Title = $"Choose {title}",
            Content = panel,
            Width = 360,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = new SolidColorBrush(Color.FromRgb(30, 31, 34)),
            Foreground = Brushes.White,
            Owner = Application.Current.MainWindow
        };

        ok.Click += (_, _) => dialog.DialogResult = true;
        cancel.Click += (_, _) => dialog.DialogResult = false;

        return dialog.ShowDialog() == true ? color : null;
    }

    private static Grid BuildColorSliderRow(string label, Slider slider)
    {
        var grid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });

        var name = new TextBlock { Text = label, Foreground = Brushes.LightGray, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold };
        var value = new TextBlock { Foreground = Brushes.LightGray, FontFamily = new FontFamily("Consolas"), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
        value.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Value") { Source = slider, StringFormat = "{0:0}" });

        Grid.SetColumn(name, 0);
        Grid.SetColumn(slider, 1);
        Grid.SetColumn(value, 2);
        grid.Children.Add(name);
        grid.Children.Add(slider);
        grid.Children.Add(value);
        return grid;
    }

    private static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

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

    private bool HasProjectNameConflict(string projectPath, out string conflictingProject)
    {
        conflictingProject = "";
        var projectName = GetProjectName(projectPath);
        if (string.IsNullOrWhiteSpace(projectName))
            return false;

        var fullPath = Path.GetFullPath(projectPath);
        foreach (var recentProject in Settings.RecentProjects)
        {
            if (string.IsNullOrWhiteSpace(recentProject) || !Directory.Exists(recentProject))
                continue;

            var recentFullPath = Path.GetFullPath(recentProject);
            if (string.Equals(recentFullPath, fullPath, StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(GetProjectName(recentProject), projectName, StringComparison.OrdinalIgnoreCase))
            {
                conflictingProject = recentProject;
                return true;
            }
        }

        return false;
    }

    private string GetProjectName(string projectPath) => GetProjectDisplayName(projectPath);

    private string GetProjectDisplayName(string projectPath)
    {
        var metadata = GetProjectMetadata(projectPath);
        if (!string.IsNullOrWhiteSpace(metadata?.Title))
            return metadata.Title.Trim();

        return GetFolderProjectName(projectPath);
    }

    private ProjectMetadata? GetProjectMetadata(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            return null;

        if (Settings.ProjectMetadata.TryGetValue(projectPath, out var metadata))
            return metadata;

        return Settings.ProjectMetadata.FirstOrDefault(entry =>
            string.Equals(Path.GetFullPath(entry.Key), Path.GetFullPath(projectPath), StringComparison.OrdinalIgnoreCase)).Value;
    }

    private void SetCurrentProjectMetadata(string? title, string? description)
    {
        if (string.IsNullOrWhiteSpace(Settings.ProjectRoot))
            return;

        if (!Settings.ProjectMetadata.TryGetValue(Settings.ProjectRoot, out var metadata))
        {
            metadata = new ProjectMetadata { Title = GetFolderProjectName(Settings.ProjectRoot) };
            Settings.ProjectMetadata[Settings.ProjectRoot] = metadata;
        }

        if (title is not null)
            metadata.Title = string.IsNullOrWhiteSpace(title) ? GetFolderProjectName(Settings.ProjectRoot) : title.Trim();
        if (description is not null)
            metadata.Description = description.Trim();
        metadata.UpdatedAt = DateTime.UtcNow;

        Settings.Save();
        RefreshRecentCollections();
        RefreshProjectTree(SelectedProjectNode?.FullPath);
        RaiseSettingsValidationChanged();
    }

    private static string GetFolderProjectName(string projectPath)
    {
        var trimmed = projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? projectPath : name;
    }

    private void RefreshRecentCollections()
    {
        RecentProjects.Clear();
        foreach (var project in Settings.RecentProjects.Where(Directory.Exists))
            RecentProjects.Add(project);

        WelcomeRecentProjects.Clear();
        foreach (var project in RecentProjects)
            WelcomeRecentProjects.Add(new RecentProjectItem(project, GetProjectMetadata(project)));

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
                if (!TryWriteTextToExistingPath(tab.FilePath, tab.Content, showDialog: true))
                    return false;
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
        var hasAntlr = File.Exists(Settings.ResolveAntlrJar());
        if (hasJava && hasCompilerRoot && hasAntlr)
            return true;

        if (showStatus)
        {
            StatusText = "Configure Java path and compiler root in Settings before running.";
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

    private static void RestoreExpandedPaths(ProjectNode? root, HashSet<string> expandedPaths, bool expandRoot = false)
    {
        if (root is null) return;
        root.IsExpanded = expandRoot || expandedPaths.Contains(root.FullPath);
        foreach (var child in root.Children)
            RestoreExpandedPaths(child, expandedPaths);
    }

    private static void AddAncestorPaths(HashSet<string> expandedPaths, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var current = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        while (!string.IsNullOrWhiteSpace(current))
        {
            expandedPaths.Add(current);
            current = Path.GetDirectoryName(current);
        }
    }

    private static bool IsFavaFile(string? path) =>
        !string.IsNullOrWhiteSpace(path) && path.EndsWith(".fava", StringComparison.OrdinalIgnoreCase);

    private static bool IsSameOrInsidePath(string path, string rootPath) =>
        string.Equals(Path.GetFullPath(path), Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase)
        || IsPathInside(path, rootPath);

    private static bool IsSamePath(string? path, string? otherPath)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(otherPath))
            return false;

        try
        {
            return string.Equals(Path.GetFullPath(path), Path.GetFullPath(otherPath), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(path, otherPath, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool IsSameOrInsidePathSafe(string? path, string? rootPath)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(rootPath))
            return false;

        try
        {
            return IsSameOrInsidePath(path, rootPath);
        }
        catch
        {
            return path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase);
        }
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
        CloseProjectCommand.RaiseCanExecuteChanged();
        DeleteProjectCommand.RaiseCanExecuteChanged();
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
        OnPropertyChanged(nameof(SettingsProjectDescription));
        OnPropertyChanged(nameof(SettingsProjectPath));
        OnPropertyChanged(nameof(ProjectTitle));
        OnPropertyChanged(nameof(ProjectDescription));
        OnPropertyChanged(nameof(IdeDataLocation));
        OnPropertyChanged(nameof(CompilerStatusText));
        OnPropertyChanged(nameof(CompilerStatusBrush));
        OnPropertyChanged(nameof(JavaStatusText));
        OnPropertyChanged(nameof(JavaStatusBrush));
        OnPropertyChanged(nameof(CompilerRootStatusText));
        OnPropertyChanged(nameof(CompilerRootStatusBrush));
        OnPropertyChanged(nameof(ToolInputsFolder));
        OnPropertyChanged(nameof(ToolOutputsFolder));
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

using System.Windows;
using System.Linq;
using FavaStudio.Editor;
using FavaStudio.Models;
using FavaStudio.ViewModels;

namespace FavaStudio;

public partial class MainWindow : Window
{
    private readonly DiagnosticUnderlineRenderer _diagnosticUnderlineRenderer;
    private readonly BreakpointMargin _breakpointMargin;
    private readonly BreakpointLineHighlighter _breakpointHighlighter;
    private readonly DebugCurrentLineHighlighter _debugCurrentLineHighlighter;

    public MainWindow()
    {
        InitializeComponent();

        _diagnosticUnderlineRenderer = new DiagnosticUnderlineRenderer(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_diagnosticUnderlineRenderer);
        Editor.TextArea.TextView.LineTransformers.Add(new FavaSyntaxColorizer());

        _breakpointHighlighter = new BreakpointLineHighlighter(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Insert(0, _breakpointHighlighter);

        _debugCurrentLineHighlighter = new DebugCurrentLineHighlighter(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Insert(1, _debugCurrentLineHighlighter);

        _breakpointMargin = new BreakpointMargin();
        Editor.TextArea.LeftMargins.Insert(0, _breakpointMargin);

        var vm = new MainViewModel(Editor);
        DataContext = vm;

        // Auto-scroll VM output when it updates
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.VmOutput))
                VmOutputBox.ScrollToEnd();
            if (e.PropertyName == nameof(vm.DebugCurrentSourceLine))
            {
                _debugCurrentLineHighlighter.SetLine(vm.DebugCurrentSourceLine);
                if (vm.DebugCurrentSourceLine is int line)
                    Editor.ScrollToLine(line);
            }
        };

        vm.Diagnostics.CollectionChanged += (_, _) => _diagnosticUnderlineRenderer.SetDiagnostics(vm.Diagnostics.ToList());
        _diagnosticUnderlineRenderer.SetDiagnostics(vm.Diagnostics.ToList());

        // Breakpoint wiring
        _breakpointMargin.BreakpointToggled += line =>
        {
            vm.ToggleBreakpoint(line);
            RefreshBreakpointRenderers(vm);
        };
        vm.BreakpointsChanged += () => RefreshBreakpointRenderers(vm);
    }

    private void RefreshBreakpointRenderers(MainViewModel vm)
    {
        var lines = vm.BreakpointLines;
        _breakpointMargin.SetBreakpoints(lines);
        _breakpointHighlighter.SetBreakpoints(lines);
    }

    private void ProjectTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm && e.NewValue is ProjectNode node)
            vm.SetSelectedProjectNode(node);
    }
}

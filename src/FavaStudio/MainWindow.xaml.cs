using System.Windows;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
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
    private readonly InlineValueHintRenderer _inlineValueHintRenderer;
    private readonly CurrentLineHighlighter _currentLineHighlighter;
    private readonly BracketHighlightRenderer _bracketHighlightRenderer;
    private readonly SearchResultRenderer _searchResultRenderer;
    private readonly List<TextSegment> _searchMatches = [];
    private int _activeSearchIndex = -1;
    private Point? _tabDragStartPoint;
    private EditorTab? _draggedTab;
    private readonly ToolTip _diagnosticToolTip = new();
    private string _activeTooltipText = "";
    private int _terminalInputStart;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += MainWindow_SourceInitialized;
        _diagnosticToolTip.Background = new SolidColorBrush(Color.FromRgb(0x2B, 0x2D, 0x30));
        _diagnosticToolTip.BorderBrush = new SolidColorBrush(Color.FromRgb(0x6A, 0x2A, 0x2A));
        _diagnosticToolTip.Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xEA, 0xF0));

        _diagnosticUnderlineRenderer = new DiagnosticUnderlineRenderer(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_diagnosticUnderlineRenderer);
        Editor.TextArea.TextView.LineTransformers.Add(new FavaSyntaxColorizer());
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.IndentationSize = 4;
        Editor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(105, 77, 141, 255));

        _currentLineHighlighter = new CurrentLineHighlighter(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Insert(0, _currentLineHighlighter);
        _breakpointHighlighter = new BreakpointLineHighlighter(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Insert(1, _breakpointHighlighter);

        _debugCurrentLineHighlighter = new DebugCurrentLineHighlighter(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Insert(2, _debugCurrentLineHighlighter);
        _inlineValueHintRenderer = new InlineValueHintRenderer(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_inlineValueHintRenderer);

        _bracketHighlightRenderer = new BracketHighlightRenderer(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_bracketHighlightRenderer);

        _searchResultRenderer = new SearchResultRenderer(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_searchResultRenderer);

        _breakpointMargin = new BreakpointMargin();
        Editor.TextArea.LeftMargins.Insert(0, _breakpointMargin);
        Editor.TextArea.Caret.PositionChanged += (_, _) =>
        {
            _currentLineHighlighter.Refresh();
            _bracketHighlightRenderer.Refresh();
        };
        Editor.TextArea.TextEntered += Editor_OnTextEntered;
        _diagnosticToolTip.PlacementTarget = Editor;
        _diagnosticToolTip.StaysOpen = true;
        Editor.MouseMove += Editor_OnMouseMove;
        Editor.MouseLeave += (_, _) => HideDiagnosticToolTip();
        Editor.PreviewMouseLeftButtonDown += Editor_OnPreviewMouseLeftButtonDown;
        Editor.PreviewMouseRightButtonDown += Editor_OnPreviewMouseRightButtonDown;
        Editor.PreviewKeyDown += Editor_OnPreviewKeyDown;
        PreviewKeyDown += MainWindow_OnPreviewKeyDown;

        var vm = new MainViewModel(Editor);
        DataContext = vm;

        // Auto-scroll VM output when it updates
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.VmOutput))
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    VmOutputBox.ScrollToEnd();
                    if (vm.IsConsoleAcceptingInput)
                        StartTerminalInputAtEnd();
                }));
            if (e.PropertyName == nameof(vm.IsConsoleAcceptingInput))
                Dispatcher.BeginInvoke(new Action(() => UpdateTerminalInputMode(vm.IsConsoleAcceptingInput)));
            if (e.PropertyName == nameof(vm.DebugCurrentSourceLine))
            {
                _debugCurrentLineHighlighter.SetLine(vm.DebugCurrentSourceLine);
                if (vm.DebugCurrentSourceLine is int line)
                    Editor.ScrollToLine(line);
            }
        };

        vm.Diagnostics.CollectionChanged += (_, _) => _diagnosticUnderlineRenderer.SetDiagnostics(vm.Diagnostics.ToList());
        _diagnosticUnderlineRenderer.SetDiagnostics(vm.Diagnostics.ToList());
        vm.DebugInlineValueHints.CollectionChanged += (_, _) => _inlineValueHintRenderer.SetHints(vm.DebugInlineValueHints.ToList());
        _inlineValueHintRenderer.SetHints(vm.DebugInlineValueHints.ToList());
        UpdateSearchMatches(resetActive: true);

        // Breakpoint wiring
        _breakpointMargin.BreakpointToggled += line =>
        {
            vm.ToggleBreakpoint(line);
            RefreshBreakpointRenderers(vm);
        };
        vm.BreakpointsChanged += () => RefreshBreakpointRenderers(vm);
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource source)
            source.AddHook(WindowProc);
    }

    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmGetMinMaxInfo = 0x0024;
        if (msg == wmGetMinMaxInfo)
        {
            ApplyMaximizedWorkArea(hwnd, lParam);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static void ApplyMaximizedWorkArea(IntPtr hwnd, IntPtr lParam)
    {
        var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero)
            return;

        var monitorInfo = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref monitorInfo))
            return;

        var minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(lParam);
        var workArea = monitorInfo.Work;
        var monitorArea = monitorInfo.Monitor;

        minMaxInfo.MaxPosition.X = Math.Abs(workArea.Left - monitorArea.Left);
        minMaxInfo.MaxPosition.Y = Math.Abs(workArea.Top - monitorArea.Top);
        minMaxInfo.MaxSize.X = Math.Abs(workArea.Right - workArea.Left);
        minMaxInfo.MaxSize.Y = Math.Abs(workArea.Bottom - workArea.Top);

        Marshal.StructureToPtr(minMaxInfo, lParam, true);
    }

    private void Editor_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (Editor.Document is null)
        {
            HideDiagnosticToolTip();
            return;
        }

        var position = Editor.GetPositionFromPoint(e.GetPosition(Editor));
        if (!position.HasValue)
        {
            HideDiagnosticToolTip();
            return;
        }

        var line = Editor.Document.GetLineByNumber(position.Value.Line);
        var column = Math.Clamp(position.Value.Column, 1, line.Length + 1);
        var offset = Editor.Document.GetOffset(position.Value.Line, column);
        var diagnostic = _diagnosticUnderlineRenderer.GetDiagnosticAtOffset(offset);
        var tooltipText = diagnostic?.Tooltip;
        if (tooltipText is null && DataContext is MainViewModel vm)
            tooltipText = FindHoverInfo(vm.HoverInfos, position.Value.Line, column)?.Tooltip;

        if (string.IsNullOrWhiteSpace(tooltipText))
        {
            HideDiagnosticToolTip();
            return;
        }

        if (_activeTooltipText == tooltipText && _diagnosticToolTip.IsOpen)
            return;

        _activeTooltipText = tooltipText;
        _diagnosticToolTip.Content = diagnostic is not null
            ? BuildHoverCard("Diagnostic", diagnostic.Severity, diagnostic.Message, diagnostic.Explanation, diagnostic.SourceLine)
            : BuildTypeCard(tooltipText);
        _diagnosticToolTip.IsOpen = true;
    }

    private void MinimizeWindow_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseWindow_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void HideDiagnosticToolTip()
    {
        _activeTooltipText = "";
        _diagnosticToolTip.IsOpen = false;
    }

    private static FavaHoverInfo? FindHoverInfo(IReadOnlyList<FavaHoverInfo> hoverInfos, int line, int column) =>
        hoverInfos
            .Where(info => info.Line == line && column >= info.StartColumn && column <= info.EndColumn)
            .OrderBy(info => info.EndColumn - info.StartColumn)
            .FirstOrDefault();

    private static Border BuildTypeCard(string tooltipText)
    {
        var lines = tooltipText.Split('\n');
        var title = lines.FirstOrDefault() ?? "Type";
        var detail = lines.Skip(1).FirstOrDefault() ?? "";
        return BuildHoverCard("Type Info", title, detail.Replace("Type: ", "", StringComparison.OrdinalIgnoreCase), "", "");
    }

    private static Border BuildHoverCard(string eyebrow, string title, string message, string detail, string sourceLine)
    {
        var panel = new StackPanel { MaxWidth = 540 };
        panel.Children.Add(new TextBlock
        {
            Text = eyebrow.ToUpperInvariant(),
            Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0xA4, 0xB2)),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 4)
        });
        panel.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x9A, 0x3D)),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xEA, 0xF0)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 5, 0, 0)
        });
        if (!string.IsNullOrWhiteSpace(detail))
        {
            panel.Children.Add(new TextBlock
            {
                Text = detail,
                Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xD0, 0xDC)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 7, 0, 0)
            });
        }
        if (!string.IsNullOrWhiteSpace(sourceLine))
        {
            panel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x22, 0x26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3F, 0x41)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(0, 8, 0, 0),
                Child = new TextBlock
                {
                    Text = sourceLine.TrimEnd(),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xEA, 0xF0)),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 12,
                    TextWrapping = TextWrapping.NoWrap
                }
            });
        }
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x2B, 0x2D, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x4F, 0x58)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10, 12, 10),
            Child = panel
        };
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

    private void OutlineList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (OutlineList.SelectedItem is FavaOutlineItem item)
            JumpToLineColumn(item.Line, item.Column);
    }

    private void ReferencesList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ReferencesList.SelectedItem is FavaReferenceItem item)
            JumpToLineColumn(item.Line, item.Column);
    }

    private void Editor_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            return;

        if (TryGoToDefinitionAtPoint(e.GetPosition(Editor)))
            e.Handled = true;
    }

    private void Editor_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Editor.Document is null)
            return;

        var position = Editor.GetPositionFromPoint(e.GetPosition(Editor));
        if (!position.HasValue)
            return;

        var line = Editor.Document.GetLineByNumber(position.Value.Line);
        var column = Math.Clamp(position.Value.Column, 1, line.Length + 1);
        var offset = Editor.Document.GetOffset(position.Value.Line, column);
        var insideSelection = Editor.SelectionLength > 0 &&
                              offset >= Editor.SelectionStart &&
                              offset <= Editor.SelectionStart + Editor.SelectionLength;
        if (!insideSelection)
            Editor.CaretOffset = offset;
    }

    private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
        {
            ShowFindBar(focusFind: true);
            e.Handled = true;
            return;
        }

        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.H)
        {
            ShowFindBar(focusFind: false);
            e.Handled = true;
            return;
        }

        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.G)
        {
            ShowFindBar(focusGoTo: true);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F3)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                MoveSearch(previous: true);
            else
                MoveSearch(previous: false);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && EditorFindBar.Visibility == Visibility.Visible)
        {
            HideFindBar();
            e.Handled = true;
        }
    }

    private void Editor_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Editor.Document is null)
            return;

        var caretOffset = Math.Clamp(Editor.CaretOffset, 0, Editor.Document.TextLength);
        var caretLine = Editor.Document.GetLineByOffset(caretOffset);
        var lineEndOffset = caretLine.Offset + caretLine.Length;
        var beforeLength = Math.Clamp(caretOffset - caretLine.Offset, 0, caretLine.Length);
        var afterLength = Math.Clamp(lineEndOffset - caretOffset, 0, caretLine.Length - beforeLength);
        var beforeCaret = Editor.Document.GetText(caretLine.Offset, beforeLength);
        var afterCaret = Editor.Document.GetText(caretOffset, afterLength);
        var indent = GetIndentation(beforeCaret);
        var innerIndent = indent;
        var shouldExpandBlock = beforeCaret.TrimEnd().EndsWith("{", StringComparison.Ordinal) &&
                                (string.IsNullOrWhiteSpace(afterCaret) || afterCaret.TrimStart().StartsWith("}", StringComparison.Ordinal));
        if (beforeCaret.TrimEnd().EndsWith("{", StringComparison.Ordinal))
            innerIndent += new string(' ', Editor.Options.IndentationSize);

        e.Handled = true;
        if (shouldExpandBlock)
        {
            var hasExistingCloseBrace = afterCaret.TrimStart().StartsWith("}", StringComparison.Ordinal);
            var text = Environment.NewLine + innerIndent + Environment.NewLine + indent + (hasExistingCloseBrace ? "" : "}");
            Editor.Document.Insert(caretOffset, text);
            Editor.CaretOffset = caretOffset + Environment.NewLine.Length + innerIndent.Length;
        }
        else
        {
            var text = Environment.NewLine + innerIndent;
            Editor.Document.Insert(caretOffset, text);
            Editor.CaretOffset = caretOffset + text.Length;
        }

        _currentLineHighlighter.Refresh();
        _bracketHighlightRenderer.Refresh();
    }

    private void Editor_OnTextEntered(object? sender, TextCompositionEventArgs e)
    {
        if (e.Text == "}")
            DedentClosingBrace();

        _bracketHighlightRenderer.Refresh();
        UpdateSearchMatches(resetActive: false);
    }

    private void DedentClosingBrace()
    {
        if (Editor.Document is null)
            return;

        var line = Editor.Document.GetLineByOffset(Editor.CaretOffset);
        var textBeforeCaret = Editor.Document.GetText(line.Offset, Math.Max(0, Editor.CaretOffset - line.Offset));
        if (!textBeforeCaret.All(ch => char.IsWhiteSpace(ch) || ch == '}'))
            return;

        var remove = Math.Min(Editor.Options.IndentationSize, textBeforeCaret.TakeWhile(ch => ch == ' ').Count());
        if (remove <= 0)
            return;

        Editor.Document.Remove(line.Offset, remove);
        Editor.CaretOffset = Math.Max(line.Offset, Editor.CaretOffset - remove);
    }

    private static string GetIndentation(string text)
    {
        var count = 0;
        while (count < text.Length && char.IsWhiteSpace(text[count]) && text[count] != '\r' && text[count] != '\n')
            count++;
        return text[..count];
    }

    private void ShowFindBar(bool focusFind = false, bool focusGoTo = false)
    {
        EditorFindBar.Visibility = Visibility.Visible;
        if (!string.IsNullOrEmpty(Editor.SelectedText) && !Editor.SelectedText.Contains('\n') && !Editor.SelectedText.Contains('\r'))
            FindBox.Text = Editor.SelectedText;

        UpdateSearchMatches(resetActive: true);

        if (focusGoTo)
        {
            GoToLineBox.Text = Editor.TextArea.Caret.Line.ToString();
            GoToLineBox.Focus();
            GoToLineBox.SelectAll();
        }
        else
        {
            if (!focusFind)
                ReplaceBox.Focus();
            else
                FindBox.Focus();
            if (focusFind)
                FindBox.SelectAll();
        }
    }

    private void HideFindBar()
    {
        EditorFindBar.Visibility = Visibility.Collapsed;
        _searchMatches.Clear();
        _activeSearchIndex = -1;
        _searchResultRenderer.Clear();
        Editor.Focus();
    }

    private void FindBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        UpdateSearchMatches(resetActive: true);

    private void SearchOption_OnChanged(object sender, RoutedEventArgs e) =>
        UpdateSearchMatches(resetActive: true);

    private void FindBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            MoveSearch(previous: (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
            e.Handled = true;
        }
    }

    private void ReplaceBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ReplaceOne();
            e.Handled = true;
        }
    }

    private void FindNext_OnClick(object sender, RoutedEventArgs e) => MoveSearch(previous: false);

    private void FindPrevious_OnClick(object sender, RoutedEventArgs e) => MoveSearch(previous: true);

    private void ReplaceOne_OnClick(object sender, RoutedEventArgs e) => ReplaceOne();

    private void ReplaceAll_OnClick(object sender, RoutedEventArgs e) => ReplaceAll();

    private void CloseEditorFindBar_OnClick(object sender, RoutedEventArgs e) => HideFindBar();

    private void GoToLineBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            GoToLine();
            e.Handled = true;
        }
    }

    private void GoToLine_OnClick(object sender, RoutedEventArgs e) => GoToLine();

    private void UpdateSearchMatches(bool resetActive)
    {
        if (_searchResultRenderer is null || FindBox is null || Editor.Document is null)
            return;

        _searchMatches.Clear();
        var pattern = FindBox.Text;
        if (string.IsNullOrEmpty(pattern))
        {
            _activeSearchIndex = -1;
            SearchStatus.Text = "";
            _searchResultRenderer.Clear();
            return;
        }

        var comparison = MatchCaseCheck.IsChecked == true
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var text = Editor.Document.Text;
        var offset = 0;
        while (offset <= text.Length - pattern.Length)
        {
            var matchOffset = text.IndexOf(pattern, offset, comparison);
            if (matchOffset < 0)
                break;

            if (WholeWordCheck.IsChecked != true || IsWholeWordMatch(text, matchOffset, pattern.Length))
                _searchMatches.Add(new TextSegment { StartOffset = matchOffset, Length = pattern.Length });

            offset = matchOffset + Math.Max(1, pattern.Length);
        }

        if (_searchMatches.Count == 0)
            _activeSearchIndex = -1;
        else if (resetActive || _activeSearchIndex < 0 || _activeSearchIndex >= _searchMatches.Count)
            _activeSearchIndex = FindMatchAtOrAfter(Editor.CaretOffset);

        ApplyActiveSearchSelection();
        UpdateSearchStatus();
    }

    private static bool IsWholeWordMatch(string text, int offset, int length)
    {
        var beforeOk = offset == 0 || !IsWordChar(text[offset - 1]);
        var afterOffset = offset + length;
        var afterOk = afterOffset >= text.Length || !IsWordChar(text[afterOffset]);
        return beforeOk && afterOk;
    }

    private static bool IsWordChar(char ch) => char.IsLetterOrDigit(ch) || ch == '_';

    private int FindMatchAtOrAfter(int offset)
    {
        for (var i = 0; i < _searchMatches.Count; i++)
        {
            if (_searchMatches[i].StartOffset >= offset)
                return i;
        }
        return _searchMatches.Count == 0 ? -1 : 0;
    }

    private void MoveSearch(bool previous)
    {
        if (EditorFindBar.Visibility != Visibility.Visible)
            ShowFindBar(focusFind: true);

        UpdateSearchMatches(resetActive: _searchMatches.Count == 0);
        if (_searchMatches.Count == 0)
            return;

        _activeSearchIndex = previous
            ? (_activeSearchIndex <= 0 ? _searchMatches.Count - 1 : _activeSearchIndex - 1)
            : (_activeSearchIndex + 1) % _searchMatches.Count;
        ApplyActiveSearchSelection();
        UpdateSearchStatus();
    }

    private void ApplyActiveSearchSelection()
    {
        _searchResultRenderer.SetMatches(_searchMatches, _activeSearchIndex);
        if (_activeSearchIndex < 0 || _activeSearchIndex >= _searchMatches.Count)
            return;

        var match = _searchMatches[_activeSearchIndex];
        Editor.Select(match.StartOffset, match.Length);
        var location = Editor.Document.GetLocation(match.StartOffset);
        Editor.ScrollToLine(location.Line);
    }

    private void UpdateSearchStatus()
    {
        SearchStatus.Text = _searchMatches.Count == 0
            ? "No results"
            : $"{_activeSearchIndex + 1}/{_searchMatches.Count}";
    }

    private void ReplaceOne()
    {
        if (Editor.Document is null)
            return;

        UpdateSearchMatches(resetActive: false);
        if (_activeSearchIndex < 0 || _activeSearchIndex >= _searchMatches.Count)
            return;

        var match = _searchMatches[_activeSearchIndex];
        Editor.Document.Replace(match.StartOffset, match.Length, ReplaceBox.Text);
        Editor.CaretOffset = match.StartOffset + ReplaceBox.Text.Length;
        UpdateSearchMatches(resetActive: true);
    }

    private void ReplaceAll()
    {
        if (Editor.Document is null || string.IsNullOrEmpty(FindBox.Text))
            return;

        UpdateSearchMatches(resetActive: true);
        if (_searchMatches.Count == 0)
            return;

        var replacement = ReplaceBox.Text;
        foreach (var match in _searchMatches.OrderByDescending(match => match.StartOffset).ToList())
            Editor.Document.Replace(match.StartOffset, match.Length, replacement);

        Editor.CaretOffset = 0;
        UpdateSearchMatches(resetActive: true);
    }

    private void GoToLine()
    {
        if (Editor.Document is null || !int.TryParse(GoToLineBox.Text, out var line))
            return;

        line = Math.Clamp(line, 1, Editor.Document.LineCount);
        var documentLine = Editor.Document.GetLineByNumber(line);
        Editor.CaretOffset = documentLine.Offset;
        Editor.ScrollToLine(line);
        Editor.Focus();
        _currentLineHighlighter.Refresh();
        _bracketHighlightRenderer.Refresh();
    }

    private void EditorContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var hasSelection = !string.IsNullOrEmpty(Editor.SelectedText);
        var symbol = FindSymbolAtCaret();
        var quickFix = FindQuickFixAtCaret();
        EditorContextCut.IsEnabled = hasSelection;
        EditorContextCopy.IsEnabled = hasSelection;
        EditorContextDelete.IsEnabled = hasSelection;
        EditorContextFormatSelection.IsEnabled = hasSelection;
        EditorContextToggleComment.IsEnabled = Editor.Document is not null && Editor.Document.LineCount > 0;
        EditorContextFindDeclaration.IsEnabled = symbol is not null && FindDefinitionAtCaret() is not null;
        EditorContextFindReferences.IsEnabled = symbol is not null;
        EditorContextQuickFix.IsEnabled = quickFix is not null;
        EditorContextQuickFix.Header = quickFix is null ? "Quick Fix" : $"Quick Fix: {quickFix.Title}";
    }

    private void EditorContextCut_OnClick(object sender, RoutedEventArgs e)
    {
        Editor.Cut();
        Editor.Focus();
    }

    private void EditorContextCopy_OnClick(object sender, RoutedEventArgs e)
    {
        Editor.Copy();
        Editor.Focus();
    }

    private void EditorContextPaste_OnClick(object sender, RoutedEventArgs e)
    {
        Editor.Paste();
        Editor.Focus();
    }

    private void EditorContextDelete_OnClick(object sender, RoutedEventArgs e)
    {
        if (Editor.Document is null || Editor.SelectionLength <= 0)
            return;

        Editor.Document.Remove(Editor.SelectionStart, Editor.SelectionLength);
        Editor.Focus();
    }

    private void EditorContextSelectAll_OnClick(object sender, RoutedEventArgs e)
    {
        Editor.SelectAll();
        Editor.Focus();
    }

    private void EditorContextFind_OnClick(object sender, RoutedEventArgs e) => ShowFindBar(focusFind: true);

    private void EditorContextReplace_OnClick(object sender, RoutedEventArgs e) => ShowFindBar(focusFind: false);

    private void EditorContextGoToLine_OnClick(object sender, RoutedEventArgs e) => ShowFindBar(focusGoTo: true);

    private void EditorContextFormatSelection_OnClick(object sender, RoutedEventArgs e)
    {
        if (Editor.Document is null || Editor.SelectionLength <= 0)
            return;

        var startLine = Editor.Document.GetLineByOffset(Editor.SelectionStart).LineNumber;
        var endLine = Editor.Document.GetLineByOffset(Editor.SelectionStart + Editor.SelectionLength).LineNumber;
        FormatLines(startLine, endLine);
        Editor.Focus();
    }

    private void EditorContextFormatDocument_OnClick(object sender, RoutedEventArgs e)
    {
        if (Editor.Document is null)
            return;

        FormatLines(1, Editor.Document.LineCount);
        Editor.Focus();
    }

    private void EditorContextToggleComment_OnClick(object sender, RoutedEventArgs e)
    {
        ToggleLineComment();
        Editor.Focus();
    }

    private void EditorContextDuplicateLine_OnClick(object sender, RoutedEventArgs e)
    {
        if (Editor.Document is null)
            return;

        if (Editor.SelectionLength > 0)
        {
            var selected = Editor.SelectedText;
            Editor.Document.Insert(Editor.SelectionStart + Editor.SelectionLength, selected);
            Editor.Select(Editor.SelectionStart + Editor.SelectionLength, selected.Length);
            return;
        }

        var line = Editor.Document.GetLineByOffset(Editor.CaretOffset);
        var lineText = Editor.Document.GetText(line.Offset, line.TotalLength);
        if (!lineText.EndsWith("\n", StringComparison.Ordinal))
            lineText += Environment.NewLine;
        Editor.Document.Insert(line.Offset + line.TotalLength, lineText);
        Editor.CaretOffset = line.Offset + line.TotalLength;
        Editor.Focus();
    }

    private void EditorContextMoveLineUp_OnClick(object sender, RoutedEventArgs e)
    {
        MoveCurrentLine(up: true);
        Editor.Focus();
    }

    private void EditorContextMoveLineDown_OnClick(object sender, RoutedEventArgs e)
    {
        MoveCurrentLine(up: false);
        Editor.Focus();
    }

    private void EditorContextFindDeclaration_OnClick(object sender, RoutedEventArgs e)
    {
        var definition = FindDefinitionAtCaret();
        if (definition is not null)
            JumpToLineColumn(definition.Line, definition.StartColumn);
        Editor.Focus();
    }

    private void EditorContextFindReferences_OnClick(object sender, RoutedEventArgs e)
    {
        ShowReferencesAtCaret();
        Editor.Focus();
    }

    private void EditorContextQuickFix_OnClick(object sender, RoutedEventArgs e)
    {
        ApplyQuickFixAtCaret();
        Editor.Focus();
    }

    private void EditorContextToggleBreakpoint_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        vm.ToggleBreakpoint(Editor.TextArea.Caret.Line);
        RefreshBreakpointRenderers(vm);
        Editor.Focus();
    }

    private void EditorContextSaveFile_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SaveFileCommand.CanExecute(null))
            vm.SaveFileCommand.Execute(null);
        Editor.Focus();
    }

    private void EditorContextRunCurrentFile_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.RunCurrentCommand.CanExecute(null))
            vm.RunCurrentCommand.Execute(null);
        Editor.Focus();
    }

    private bool TryGoToDefinitionAtPoint(Point point)
    {
        if (Editor.Document is null)
            return false;

        var position = Editor.GetPositionFromPoint(point);
        if (!position.HasValue)
            return false;

        var definition = FindDefinition(position.Value.Line, position.Value.Column);
        if (definition is null)
            return false;

        JumpToLineColumn(definition.Line, definition.StartColumn);
        return true;
    }

    private FavaHoverInfo? FindDefinitionAtCaret()
    {
        if (Editor.Document is null)
            return null;

        var location = Editor.Document.GetLocation(Editor.CaretOffset);
        return FindDefinition(location.Line, location.Column);
    }

    private FavaHoverInfo? FindSymbolAtCaret()
    {
        if (Editor.Document is null || DataContext is not MainViewModel vm)
            return null;

        var selectedSymbol = GetSelectedSymbolName();
        if (!string.IsNullOrWhiteSpace(selectedSymbol))
        {
            var selectionLocation = Editor.Document.GetLocation(Editor.SelectionStart);
            return vm.HoverInfos
                .Where(info => string.Equals(info.Name, selectedSymbol, StringComparison.Ordinal))
                .OrderBy(info => Math.Abs(info.Line - selectionLocation.Line))
                .ThenBy(info => Math.Abs(info.StartColumn - selectionLocation.Column))
                .FirstOrDefault();
        }

        var location = Editor.Document.GetLocation(Editor.CaretOffset);
        return FindHoverInfo(vm.HoverInfos, location.Line, location.Column);
    }

    private string? GetSelectedSymbolName()
    {
        var selected = Editor.SelectedText?.Trim();
        if (string.IsNullOrWhiteSpace(selected) || selected.Contains('\n') || selected.Contains('\r'))
            return null;

        return Regex.IsMatch(selected, @"^[A-Za-z_]\w*$") ? selected : null;
    }

    private FavaHoverInfo? FindDefinition(int line, int column)
    {
        if (DataContext is not MainViewModel vm)
            return null;

        var symbol = FindHoverInfo(vm.HoverInfos, line, column);
        if (symbol is null || string.IsNullOrWhiteSpace(symbol.Name))
            return null;

        return vm.HoverInfos
            .Where(info => info.IsDefinition && string.Equals(info.Name, symbol.Name, StringComparison.Ordinal))
            .OrderBy(info => Math.Abs(info.Line - line))
            .ThenBy(info => info.StartColumn)
            .FirstOrDefault();
    }

    private void ShowReferencesAtCaret()
    {
        if (Editor.Document is null || DataContext is not MainViewModel vm)
            return;

        var symbol = FindSymbolAtCaret();
        if (symbol is null || string.IsNullOrWhiteSpace(symbol.Name))
            return;

        var references = vm.HoverInfos
            .Where(info => string.Equals(info.Name, symbol.Name, StringComparison.Ordinal))
            .GroupBy(info => new { info.Line, info.StartColumn, info.EndColumn, info.Kind, info.Type })
            .Select(group => group.First())
            .OrderBy(info => info.Line)
            .ThenBy(info => info.StartColumn)
            .Select(info => new FavaReferenceItem
            {
                Name = info.Name,
                Kind = info.Kind,
                Type = info.Type,
                Line = info.Line,
                Column = info.StartColumn,
                IsDefinition = info.IsDefinition,
                Preview = GetLinePreview(info.Line)
            })
            .ToList();

        vm.SetReferenceResults(references);
        if (references.Count > 0)
            vm.ShowDiagnostics = true;
    }

    private string GetLinePreview(int line)
    {
        if (Editor.Document is null || line <= 0 || line > Editor.Document.LineCount)
            return "";

        return Editor.Document.GetText(Editor.Document.GetLineByNumber(line)).Trim();
    }

    private QuickFix? FindQuickFixAtCaret()
    {
        var diagnostic = GetDiagnosticAtCaret();
        if (diagnostic is null || Editor.Document is null)
            return null;

        var message = diagnostic.Message;
        var undeclared = Regex.Match(message, @"(?:variable\s+)?'?(?<name>[A-Za-z_]\w*)'?\s+(?:is\s+)?not declared", RegexOptions.IgnoreCase);
        if (undeclared.Success)
        {
            var name = undeclared.Groups["name"].Value;
            return new QuickFix($"declare integer {name}", () => InsertVariableDeclaration(name, "integer"));
        }

        if (message.Contains("cannot print", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("[]", StringComparison.OrdinalIgnoreCase))
        {
            var lineText = GetLinePreview(diagnostic.Line);
            if (Regex.IsMatch(lineText, @"^\s*print\s+.+;\s*$", RegexOptions.IgnoreCase))
                return new QuickFix("print array length", () => WrapPrintedExpressionWithLength(diagnostic.Line));
        }

        if (message.Contains("return", StringComparison.OrdinalIgnoreCase))
        {
            var returnType = InferNearestFunctionReturnType(diagnostic.Line);
            if (!string.Equals(returnType, "void", StringComparison.OrdinalIgnoreCase))
                return new QuickFix($"add {returnType} return stub", () => InsertReturnStub(diagnostic.Line, returnType));
        }

        return null;
    }

    private void ApplyQuickFixAtCaret()
    {
        var quickFix = FindQuickFixAtCaret();
        quickFix?.Apply();
    }

    private FavaDiagnostic? GetDiagnosticAtCaret()
    {
        if (Editor.Document is null)
            return null;

        var offset = Math.Clamp(Editor.CaretOffset, 0, Editor.Document.TextLength);
        return _diagnosticUnderlineRenderer.GetDiagnosticAtOffset(offset);
    }

    private void InsertVariableDeclaration(string name, string type)
    {
        if (Editor.Document is null)
            return;

        var caretLine = Editor.Document.GetLineByOffset(Editor.CaretOffset).LineNumber;
        for (var lineNumber = caretLine; lineNumber >= 1; lineNumber--)
        {
            var line = Editor.Document.GetLineByNumber(lineNumber);
            var text = Editor.Document.GetText(line);
            if (!text.Contains('{'))
                continue;

            var indent = GetIndentation(text) + new string(' ', Editor.Options.IndentationSize);
            var insertOffset = line.Offset + line.TotalLength;
            Editor.Document.Insert(insertOffset, $"{indent}{type} {name};{Environment.NewLine}");
            return;
        }

        Editor.Document.Insert(0, $"{type} {name};{Environment.NewLine}");
    }

    private void WrapPrintedExpressionWithLength(int lineNumber)
    {
        if (Editor.Document is null || lineNumber <= 0 || lineNumber > Editor.Document.LineCount)
            return;

        var line = Editor.Document.GetLineByNumber(lineNumber);
        var text = Editor.Document.GetText(line);
        var match = Regex.Match(text, @"^(?<prefix>\s*print\s+)(?<expr>.*?)(?<suffix>\s*;\s*)$", RegexOptions.IgnoreCase);
        if (!match.Success)
            return;

        var prefix = match.Groups["prefix"].Value;
        var expression = match.Groups["expr"].Value.Trim();
        var suffix = match.Groups["suffix"].Value;
        var replacement = $"{prefix}length({expression}){suffix}";
        Editor.Document.Replace(line.Offset, line.Length, replacement.TrimEnd('\r', '\n'));
    }

    private void InsertReturnStub(int lineNumber, string returnType)
    {
        if (Editor.Document is null)
            return;

        var line = Math.Clamp(lineNumber, 1, Editor.Document.LineCount);
        var documentLine = Editor.Document.GetLineByNumber(line);
        var indent = GetIndentation(Editor.Document.GetText(documentLine));
        var returnValue = returnType.ToLowerInvariant() switch
        {
            "real" => "0.0",
            "string" => "\"\"",
            "bool" => "false",
            _ => "0"
        };

        Editor.Document.Insert(documentLine.Offset, $"{indent}return {returnValue};{Environment.NewLine}");
    }

    private string InferNearestFunctionReturnType(int lineNumber)
    {
        if (Editor.Document is null || DataContext is not MainViewModel vm)
            return "integer";

        var function = vm.HoverInfos
            .Where(info => info.IsDefinition &&
                           info.Kind.Contains("function", StringComparison.OrdinalIgnoreCase) &&
                           info.Line <= lineNumber)
            .OrderByDescending(info => info.Line)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(function?.Type) ? "integer" : function.Type;
    }

    private sealed record QuickFix(string Title, Action Apply);

    private void JumpToLineColumn(int line, int column)
    {
        if (Editor.Document is null || line <= 0 || line > Editor.Document.LineCount)
            return;

        var documentLine = Editor.Document.GetLineByNumber(line);
        var offset = documentLine.Offset + Math.Clamp(column - 1, 0, documentLine.Length);
        Editor.CaretOffset = offset;
        Editor.Select(offset, 0);
        Editor.ScrollToLine(line);
        Editor.Focus();
        _currentLineHighlighter.Refresh();
    }

    private void FormatLines(int startLine, int endLine)
    {
        if (Editor.Document is null)
            return;

        startLine = Math.Clamp(startLine, 1, Editor.Document.LineCount);
        endLine = Math.Clamp(endLine, startLine, Editor.Document.LineCount);
        var indent = 0;
        for (var lineNumber = 1; lineNumber <= endLine; lineNumber++)
        {
            var line = Editor.Document.GetLineByNumber(lineNumber);
            var text = Editor.Document.GetText(line);
            var trimmed = text.TrimStart();
            if (trimmed.StartsWith("}", StringComparison.Ordinal))
                indent = Math.Max(0, indent - 1);

            if (lineNumber >= startLine && !string.IsNullOrWhiteSpace(text))
            {
                var newText = new string(' ', indent * Editor.Options.IndentationSize) + trimmed;
                Editor.Document.Replace(line.Offset, line.Length, newText.TrimEnd('\r', '\n'));
            }

            if (trimmed.EndsWith("{", StringComparison.Ordinal))
                indent++;
        }
    }

    private void ToggleLineComment()
    {
        if (Editor.Document is null)
            return;

        var startLine = Editor.Document.GetLineByOffset(Editor.SelectionStart).LineNumber;
        var endOffset = Editor.SelectionLength > 0 ? Editor.SelectionStart + Editor.SelectionLength : Editor.CaretOffset;
        var endLine = Editor.Document.GetLineByOffset(Math.Clamp(endOffset, 0, Editor.Document.TextLength)).LineNumber;
        var lines = Enumerable.Range(startLine, endLine - startLine + 1)
            .Select(lineNumber => Editor.Document.GetLineByNumber(lineNumber))
            .ToList();
        var shouldUncomment = lines
            .Where(line => !string.IsNullOrWhiteSpace(Editor.Document.GetText(line)))
            .All(line => Editor.Document.GetText(line).TrimStart().StartsWith("//", StringComparison.Ordinal));

        foreach (var line in lines.OrderByDescending(line => line.LineNumber))
        {
            var text = Editor.Document.GetText(line);
            var leading = text.Length - text.TrimStart().Length;
            if (shouldUncomment)
            {
                var commentOffset = line.Offset + leading;
                if (commentOffset + 2 <= Editor.Document.TextLength &&
                    Editor.Document.GetText(commentOffset, 2) == "//")
                    Editor.Document.Remove(commentOffset, 2);
            }
            else
            {
                Editor.Document.Insert(line.Offset + leading, "//");
            }
        }
    }

    private void MoveCurrentLine(bool up)
    {
        if (Editor.Document is null)
            return;

        var line = Editor.Document.GetLineByOffset(Editor.CaretOffset);
        if (up && line.LineNumber == 1)
            return;
        if (!up && line.LineNumber == Editor.Document.LineCount)
            return;

        var other = Editor.Document.GetLineByNumber(up ? line.LineNumber - 1 : line.LineNumber + 1);
        var lineText = Editor.Document.GetText(line.Offset, line.TotalLength);
        var otherText = Editor.Document.GetText(other.Offset, other.TotalLength);
        var caretColumn = Editor.TextArea.Caret.Column;

        if (up)
        {
            Editor.Document.Replace(other.Offset, other.TotalLength + line.TotalLength, lineText + otherText);
            Editor.CaretOffset = Editor.Document.GetLineByNumber(line.LineNumber - 1).Offset + Math.Max(0, caretColumn - 1);
        }
        else
        {
            Editor.Document.Replace(line.Offset, line.TotalLength + other.TotalLength, otherText + lineText);
            Editor.CaretOffset = Editor.Document.GetLineByNumber(line.LineNumber + 1).Offset + Math.Max(0, caretColumn - 1);
        }
    }

    private void EditorTabs_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _tabDragStartPoint = e.GetPosition(EditorTabs);
        _draggedTab = FindAncestor<TabItem>(e.OriginalSource as DependencyObject)?.DataContext as EditorTab;
    }

    private void EditorTabs_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _tabDragStartPoint is null || _draggedTab is null)
            return;

        var position = e.GetPosition(EditorTabs);
        if (Math.Abs(position.X - _tabDragStartPoint.Value.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _tabDragStartPoint.Value.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        DragDrop.DoDragDrop(EditorTabs, _draggedTab, DragDropEffects.Move);
        _tabDragStartPoint = null;
        _draggedTab = null;
    }

    private void EditorTabs_OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(EditorTab)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void EditorTabs_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(EditorTab)) || DataContext is not MainViewModel vm)
            return;

        var sourceTab = e.Data.GetData(typeof(EditorTab)) as EditorTab;
        if (sourceTab is null)
            return;

        var targetTab = FindAncestor<TabItem>(e.OriginalSource as DependencyObject)?.DataContext as EditorTab;
        var sourceIndex = vm.OpenEditorTabs.IndexOf(sourceTab);
        if (sourceIndex < 0)
            return;

        var targetIndex = targetTab is null
            ? vm.OpenEditorTabs.Count - 1
            : vm.OpenEditorTabs.IndexOf(targetTab);
        if (targetIndex < 0 || sourceIndex == targetIndex)
            return;

        vm.OpenEditorTabs.Move(sourceIndex, targetIndex);
        vm.SelectedEditorTab = sourceTab;
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void TerminalSurface_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel { IsConsoleAcceptingInput: true })
            return;

        VmOutputBox.Focus();
        MoveTerminalCaretToInputEnd();
    }

    private void TerminalSurface_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (DataContext is not MainViewModel { IsConsoleAcceptingInput: true })
            return;

        if (Keyboard.FocusedElement == VmOutputBox)
            return;

        VmOutputBox.Focus();
        MoveTerminalCaretToInputEnd();
        VmOutputBox.SelectedText = e.Text;
        e.Handled = true;
    }

    private void TerminalSurface_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel { IsConsoleAcceptingInput: true })
            return;

        if (Keyboard.FocusedElement == VmOutputBox)
            return;

        if (e.Key is Key.Back or Key.Delete or Key.Left or Key.Right or Key.Home or Key.End)
        {
            VmOutputBox.Focus();
            MoveTerminalCaretToInputEnd();
        }
        else if (e.Key == Key.Enter)
        {
            VmOutputBox.Focus();
            if (DataContext is MainViewModel vm && vm.SendConsoleInputCommand.CanExecute(null))
            {
                SendInlineTerminalInput(vm);
                e.Handled = true;
            }
        }
    }

    private void VmOutputBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (DataContext is not MainViewModel { IsConsoleAcceptingInput: true })
        {
            e.Handled = true;
            return;
        }

        EnsureTerminalCaretInInput();
    }

    private void VmOutputBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel { IsConsoleAcceptingInput: true })
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                e.Handled = e.Key is not Key.Tab;
            return;
        }

        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            return;

        if (e.Key == Key.Enter)
        {
            if (DataContext is MainViewModel vm && vm.SendConsoleInputCommand.CanExecute(null))
                SendInlineTerminalInput(vm);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Home)
        {
            VmOutputBox.CaretIndex = _terminalInputStart;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Back && VmOutputBox.SelectionLength == 0 && VmOutputBox.CaretIndex <= _terminalInputStart)
        {
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete && VmOutputBox.SelectionLength == 0 && VmOutputBox.CaretIndex < _terminalInputStart)
        {
            e.Handled = true;
            return;
        }

        if ((e.Key is Key.Left or Key.Up or Key.PageUp) && VmOutputBox.CaretIndex <= _terminalInputStart)
        {
            VmOutputBox.CaretIndex = _terminalInputStart;
            e.Handled = true;
            return;
        }

        EnsureTerminalSelectionDoesNotTouchHistory(e);
    }

    private void StartTerminalInputAtEnd()
    {
        _terminalInputStart = VmOutputBox.Text.Length;
        MoveTerminalCaretToInputEnd();
        VmOutputBox.ScrollToEnd();
    }

    private void UpdateTerminalInputMode(bool isAcceptingInput)
    {
        VmOutputBox.IsReadOnly = !isAcceptingInput;
        VmOutputBox.CaretBrush = isAcceptingInput
            ? (Brush)FindResource("TextMainBrush")
            : Brushes.Transparent;

        if (isAcceptingInput)
        {
            StartTerminalInputAtEnd();
            VmOutputBox.Focus();
        }
        else if (Keyboard.FocusedElement == VmOutputBox)
        {
            Keyboard.ClearFocus();
            VmOutputBox.ScrollToEnd();
        }
    }

    private void MoveTerminalCaretToInputEnd()
    {
        VmOutputBox.CaretIndex = VmOutputBox.Text.Length;
    }

    private void EnsureTerminalCaretInInput()
    {
        if (VmOutputBox.CaretIndex < _terminalInputStart)
            MoveTerminalCaretToInputEnd();
    }

    private void EnsureTerminalSelectionDoesNotTouchHistory(KeyEventArgs e)
    {
        if (VmOutputBox.SelectionLength == 0)
        {
            EnsureTerminalCaretInInput();
            return;
        }

        var selectionStart = VmOutputBox.SelectionStart;
        if (selectionStart < _terminalInputStart)
        {
            VmOutputBox.Select(_terminalInputStart, Math.Max(0, VmOutputBox.Text.Length - _terminalInputStart));
            e.Handled = true;
        }
    }

    private void SendInlineTerminalInput(MainViewModel vm)
    {
        var text = VmOutputBox.Text;
        var input = _terminalInputStart <= text.Length ? text[_terminalInputStart..] : "";
        vm.ConsoleInput = input.Replace("\r", "").Replace("\n", "");
        vm.SendConsoleInputCommand.Execute(null);
    }

    private const int MonitorDefaultToNearest = 0x00000002;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public NativePoint Reserved;
        public NativePoint MaxSize;
        public NativePoint MaxPosition;
        public NativePoint MinTrackSize;
        public NativePoint MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect Work;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

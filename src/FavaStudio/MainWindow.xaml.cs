using System.Windows;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
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
    private readonly CurrentLineHighlighter _currentLineHighlighter;
    private readonly BracketHighlightRenderer _bracketHighlightRenderer;
    private readonly SearchResultRenderer _searchResultRenderer;
    private readonly List<TextSegment> _searchMatches = [];
    private int _activeSearchIndex = -1;
    private Point? _tabDragStartPoint;
    private EditorTab? _draggedTab;

    public MainWindow()
    {
        InitializeComponent();

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
        Editor.PreviewKeyDown += Editor_OnPreviewKeyDown;
        PreviewKeyDown += MainWindow_OnPreviewKeyDown;

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
        UpdateSearchMatches(resetActive: true);

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
        EditorContextCut.IsEnabled = hasSelection;
        EditorContextCopy.IsEnabled = hasSelection;
        EditorContextDelete.IsEnabled = hasSelection;
        EditorContextFormatSelection.IsEnabled = hasSelection;
        EditorContextToggleComment.IsEnabled = Editor.Document is not null && Editor.Document.LineCount > 0;
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
}

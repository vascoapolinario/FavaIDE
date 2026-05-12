using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class BreakpointLineHighlighter(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;
    private HashSet<int> _breakpoints = [];

    private static readonly SolidColorBrush HighlightBrush;

    static BreakpointLineHighlighter()
    {
        HighlightBrush = new SolidColorBrush(Color.FromArgb(38, 220, 50, 50));
        HighlightBrush.Freeze();
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void SetBreakpoints(IEnumerable<int> lines)
    {
        _breakpoints = new HashSet<int>(lines);
        _editor.TextArea.TextView.InvalidateLayer(Layer);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null || _breakpoints.Count == 0) return;

        foreach (var vl in textView.VisualLines)
        {
            var lineNum = vl.FirstDocumentLine.LineNumber;
            if (!_breakpoints.Contains(lineNum)) continue;

            var y = vl.VisualTop - textView.ScrollOffset.Y;
            drawingContext.DrawRectangle(HighlightBrush, null,
                new Rect(0, y, textView.ActualWidth, vl.Height));
        }
    }
}

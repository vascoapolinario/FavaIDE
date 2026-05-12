using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class DebugCurrentLineHighlighter(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;
    private int? _line;

    private static readonly SolidColorBrush HighlightBrush;
    private static readonly Pen BorderPen;

    static DebugCurrentLineHighlighter()
    {
        HighlightBrush = new SolidColorBrush(Color.FromArgb(55, 255, 196, 87));
        HighlightBrush.Freeze();
        BorderPen = new Pen(new SolidColorBrush(Color.FromArgb(170, 255, 196, 87)), 1);
        BorderPen.Freeze();
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void SetLine(int? line)
    {
        _line = line > 0 ? line : null;
        _editor.TextArea.TextView.InvalidateLayer(Layer);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null || !_line.HasValue)
            return;

        foreach (var visualLine in textView.VisualLines)
        {
            if (visualLine.FirstDocumentLine.LineNumber != _line.Value)
                continue;

            var y = visualLine.VisualTop - textView.ScrollOffset.Y;
            drawingContext.DrawRectangle(
                HighlightBrush,
                BorderPen,
                new Rect(0, y, textView.ActualWidth, visualLine.Height));
            break;
        }
    }
}

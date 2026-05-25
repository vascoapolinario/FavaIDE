using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class CurrentLineHighlighter(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;

    private static readonly SolidColorBrush HighlightBrush;
    private static readonly Pen BorderPen;

    static CurrentLineHighlighter()
    {
        HighlightBrush = new SolidColorBrush(Color.FromArgb(26, 108, 166, 255));
        HighlightBrush.Freeze();
        BorderPen = new Pen(new SolidColorBrush(Color.FromArgb(70, 108, 166, 255)), 1);
        BorderPen.Freeze();
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Refresh() => _editor.TextArea.TextView.InvalidateLayer(Layer);

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null)
            return;

        var caretLine = _editor.TextArea.Caret.Line;
        foreach (var visualLine in textView.VisualLines)
        {
            if (visualLine.FirstDocumentLine.LineNumber != caretLine)
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

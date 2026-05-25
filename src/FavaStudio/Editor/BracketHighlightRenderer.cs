using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class BracketHighlightRenderer(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;
    private int? _firstOffset;
    private int? _secondOffset;

    private static readonly SolidColorBrush FillBrush;
    private static readonly Pen BorderPen;

    static BracketHighlightRenderer()
    {
        FillBrush = new SolidColorBrush(Color.FromArgb(54, 255, 154, 61));
        FillBrush.Freeze();
        BorderPen = new Pen(new SolidColorBrush(Color.FromArgb(190, 255, 154, 61)), 1);
        BorderPen.Freeze();
    }

    public KnownLayer Layer => KnownLayer.Selection;

    public void Refresh()
    {
        _firstOffset = null;
        _secondOffset = null;

        var document = _editor.Document;
        if (document is null || document.TextLength == 0)
        {
            _editor.TextArea.TextView.InvalidateLayer(Layer);
            return;
        }

        var caretOffset = _editor.CaretOffset;
        if (TryFindPair(document, caretOffset - 1, out var first, out var second) ||
            TryFindPair(document, caretOffset, out first, out second))
        {
            _firstOffset = first;
            _secondOffset = second;
        }

        _editor.TextArea.TextView.InvalidateLayer(Layer);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null)
            return;

        DrawBracket(textView, drawingContext, _firstOffset);
        DrawBracket(textView, drawingContext, _secondOffset);
    }

    private void DrawBracket(TextView textView, DrawingContext drawingContext, int? offset)
    {
        if (!offset.HasValue || _editor.Document is null || offset.Value < 0 || offset.Value >= _editor.Document.TextLength)
            return;

        var location = _editor.Document.GetLocation(offset.Value);
        var position = new TextViewPosition(location.Line, location.Column);
        var point = textView.GetVisualPosition(position, VisualYPosition.TextTop);
        var visualLine = textView.VisualLines.FirstOrDefault(line =>
            line.FirstDocumentLine.LineNumber <= location.Line &&
            line.LastDocumentLine.LineNumber >= location.Line);
        if (visualLine is null)
            return;

        var y = visualLine.VisualTop - textView.ScrollOffset.Y;
        var height = visualLine.Height;
        drawingContext.DrawRoundedRectangle(
            FillBrush,
            BorderPen,
            new Rect(point.X, y + 1, 9, height - 2),
            2,
            2);
    }

    private static bool TryFindPair(TextDocument document, int offset, out int first, out int second)
    {
        first = -1;
        second = -1;
        if (offset < 0 || offset >= document.TextLength)
            return false;

        var ch = document.GetCharAt(offset);
        return ch switch
        {
            '(' => FindForward(document, offset, '(', ')', out first, out second),
            '[' => FindForward(document, offset, '[', ']', out first, out second),
            '{' => FindForward(document, offset, '{', '}', out first, out second),
            ')' => FindBackward(document, offset, '(', ')', out first, out second),
            ']' => FindBackward(document, offset, '[', ']', out first, out second),
            '}' => FindBackward(document, offset, '{', '}', out first, out second),
            _ => false
        };
    }

    private static bool FindForward(TextDocument document, int startOffset, char open, char close, out int first, out int second)
    {
        first = startOffset;
        second = -1;
        var depth = 0;
        for (var i = startOffset; i < document.TextLength; i++)
        {
            var ch = document.GetCharAt(i);
            if (ch == open)
                depth++;
            else if (ch == close)
            {
                depth--;
                if (depth == 0)
                {
                    second = i;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool FindBackward(TextDocument document, int startOffset, char open, char close, out int first, out int second)
    {
        first = -1;
        second = startOffset;
        var depth = 0;
        for (var i = startOffset; i >= 0; i--)
        {
            var ch = document.GetCharAt(i);
            if (ch == close)
                depth++;
            else if (ch == open)
            {
                depth--;
                if (depth == 0)
                {
                    first = i;
                    return true;
                }
            }
        }
        return false;
    }
}

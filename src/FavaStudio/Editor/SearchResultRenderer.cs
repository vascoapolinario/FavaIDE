using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class SearchResultRenderer(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;
    private IReadOnlyList<TextSegment> _matches = [];
    private int _activeIndex = -1;

    private static readonly SolidColorBrush MatchBrush;
    private static readonly SolidColorBrush ActiveBrush;
    private static readonly Pen ActivePen;

    static SearchResultRenderer()
    {
        MatchBrush = new SolidColorBrush(Color.FromArgb(58, 255, 196, 87));
        MatchBrush.Freeze();
        ActiveBrush = new SolidColorBrush(Color.FromArgb(88, 255, 154, 61));
        ActiveBrush.Freeze();
        ActivePen = new Pen(new SolidColorBrush(Color.FromArgb(220, 255, 154, 61)), 1);
        ActivePen.Freeze();
    }

    public KnownLayer Layer => KnownLayer.Selection;

    public void SetMatches(IReadOnlyList<TextSegment> matches, int activeIndex)
    {
        _matches = matches;
        _activeIndex = activeIndex;
        _editor.TextArea.TextView.InvalidateLayer(Layer);
    }

    public void Clear() => SetMatches([], -1);

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null || _matches.Count == 0)
            return;

        for (var i = 0; i < _matches.Count; i++)
        {
            var segment = _matches[i];
            var isActive = i == _activeIndex;
            DrawSegment(textView, drawingContext, segment, isActive);
        }
    }

    private void DrawSegment(TextView textView, DrawingContext drawingContext, TextSegment segment, bool isActive)
    {
        if (_editor.Document is null || segment.Length <= 0)
            return;

        var start = _editor.Document.GetLocation(segment.StartOffset);
        var end = _editor.Document.GetLocation(segment.EndOffset);
        if (start.Line != end.Line)
            return;

        var visualLine = textView.VisualLines.FirstOrDefault(line =>
            line.FirstDocumentLine.LineNumber <= start.Line &&
            line.LastDocumentLine.LineNumber >= start.Line);
        if (visualLine is null)
            return;

        var startPoint = textView.GetVisualPosition(new TextViewPosition(start.Line, start.Column), VisualYPosition.TextTop);
        var endPoint = textView.GetVisualPosition(new TextViewPosition(end.Line, end.Column), VisualYPosition.TextTop);
        var y = visualLine.VisualTop - textView.ScrollOffset.Y + 2;
        var width = Math.Max(7, endPoint.X - startPoint.X);
        drawingContext.DrawRoundedRectangle(
            isActive ? ActiveBrush : MatchBrush,
            isActive ? ActivePen : null,
            new Rect(startPoint.X, y, width, Math.Max(4, visualLine.Height - 4)),
            2,
            2);
    }
}

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using FavaStudio.Models;

namespace FavaStudio.Editor;

public sealed class DiagnosticUnderlineRenderer(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;
    private IReadOnlyList<FavaDiagnostic> _diagnostics = [];

    public KnownLayer Layer => KnownLayer.Selection;

    public void SetDiagnostics(IReadOnlyList<FavaDiagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
        _editor.TextArea.TextView.InvalidateLayer(Layer);
    }

    public FavaDiagnostic? GetDiagnosticAtOffset(int offset)
    {
        if (_editor.Document is null || _diagnostics.Count == 0)
            return null;

        var exact = _diagnostics.FirstOrDefault(diagnostic =>
        {
            if (diagnostic.Line <= 0 || diagnostic.Line > _editor.Document.LineCount)
                return false;

            var line = _editor.Document.GetLineByNumber(diagnostic.Line);
            var startColumn = Math.Max(1, diagnostic.Column);
            var startOffset = line.Offset + Math.Min(startColumn - 1, line.Length);
            var length = Math.Max(1, diagnostic.UnderlineLength);
            var endOffset = Math.Min(line.EndOffset, startOffset + length);
            return offset >= startOffset && offset <= endOffset;
        });
        if (exact is not null)
            return exact;

        var hoverLine = _editor.Document.GetLineByOffset(Math.Clamp(offset, 0, _editor.Document.TextLength)).LineNumber;
        return _diagnostics.FirstOrDefault(diagnostic => diagnostic.Line == hoverLine);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null || _diagnostics.Count == 0)
            return;

        foreach (var diagnostic in _diagnostics)
        {
            if (diagnostic.Line <= 0 || diagnostic.Line > _editor.Document.LineCount)
                continue;

            var startColumn = diagnostic.Column <= 0 ? 1 : diagnostic.Column;
            var endColumn = startColumn + (diagnostic.UnderlineLength <= 0 ? 1 : diagnostic.UnderlineLength);
            var visualLine = textView.VisualLines.FirstOrDefault(line =>
                line.FirstDocumentLine.LineNumber <= diagnostic.Line &&
                line.LastDocumentLine.LineNumber >= diagnostic.Line);
            if (visualLine is null)
                continue;

            var start = textView.GetVisualPosition(new TextViewPosition(diagnostic.Line, startColumn), VisualYPosition.TextBottom);
            var end = textView.GetVisualPosition(new TextViewPosition(diagnostic.Line, endColumn), VisualYPosition.TextBottom);

            var x1 = start.X - textView.ScrollOffset.X;
            var x2 = end.X - textView.ScrollOffset.X;
            if (x2 <= x1)
                x2 = x1 + 8;
            var y = start.Y - textView.ScrollOffset.Y + 1;
            var textTop = textView.GetVisualPosition(new TextViewPosition(diagnostic.Line, startColumn), VisualYPosition.TextTop);
            var isWarning = string.Equals(diagnostic.Severity, "Warning", StringComparison.OrdinalIgnoreCase);
            var penBrush = isWarning
                ? new SolidColorBrush(Color.FromRgb(0xD7, 0xAA, 0x45))
                : new SolidColorBrush(Color.FromRgb(0xFF, 0x6D, 0x6D));
            var fill = isWarning
                ? new SolidColorBrush(Color.FromArgb(34, 0xD7, 0xAA, 0x45))
                : new SolidColorBrush(Color.FromArgb(38, 0xFF, 0x5D, 0x5D));
            var pen = new Pen(penBrush, 2.0);
            pen.Freeze();
            fill.Freeze();

            drawingContext.DrawRoundedRectangle(
                fill,
                null,
                new Rect(x1, textTop.Y - textView.ScrollOffset.Y, Math.Max(8, x2 - x1), Math.Max(8, y - (textTop.Y - textView.ScrollOffset.Y) + 3)),
                2,
                2);

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(x1, y), false, false);
                var up = true;
                for (var x = x1; x < x2; x += 4)
                {
                    var nextX = x + 2;
                    context.LineTo(new Point(nextX, up ? y - 2 : y), true, false);
                    up = !up;
                }
            }
            geometry.Freeze();
            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }
}

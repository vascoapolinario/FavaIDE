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

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null || _diagnostics.Count == 0)
            return;

        var pen = new Pen(Brushes.IndianRed, 1.6);
        pen.Freeze();

        foreach (var diagnostic in _diagnostics)
        {
            if (diagnostic.Line <= 0 || diagnostic.Line > _editor.Document.LineCount)
                continue;

            var startColumn = diagnostic.Column <= 0 ? 1 : diagnostic.Column;
            var endColumn = startColumn + (diagnostic.UnderlineLength <= 0 ? 1 : diagnostic.UnderlineLength);

            var start = textView.GetVisualPosition(new TextViewPosition(diagnostic.Line, startColumn), VisualYPosition.TextBottom);
            var end = textView.GetVisualPosition(new TextViewPosition(diagnostic.Line, endColumn), VisualYPosition.TextBottom);

            var x1 = start.X;
            var x2 = end.X <= x1 ? x1 + 8 : end.X;
            var y = start.Y + 1;

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

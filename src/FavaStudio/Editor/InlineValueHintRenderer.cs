using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using FavaStudio.Models;

namespace FavaStudio.Editor;

public sealed class InlineValueHintRenderer(TextEditor editor) : IBackgroundRenderer
{
    private readonly TextEditor _editor = editor;
    private IReadOnlyList<InlineValueHint> _hints = [];

    public KnownLayer Layer => KnownLayer.Text;

    public void SetHints(IReadOnlyList<InlineValueHint> hints)
    {
        _hints = hints;
        _editor.TextArea.TextView.InvalidateLayer(Layer);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || _editor.Document is null || _hints.Count == 0)
            return;

        var typeface = new Typeface(_editor.FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
        var pixelsPerDip = VisualTreeHelper.GetDpi(textView).PixelsPerDip;
        var foreground = new SolidColorBrush(Color.FromRgb(0x93, 0xD5, 0x8A));
        var background = new SolidColorBrush(Color.FromArgb(48, 0x3A, 0x66, 0x35));
        foreground.Freeze();
        background.Freeze();

        foreach (var hint in _hints)
        {
            if (hint.Line <= 0 || hint.Line > _editor.Document.LineCount || string.IsNullOrWhiteSpace(hint.Text))
                continue;

            var visualLine = textView.GetVisualLine(hint.Line);
            if (visualLine is null)
                continue;

            var documentLine = _editor.Document.GetLineByNumber(hint.Line);
            var endPoint = textView.GetVisualPosition(
                new TextViewPosition(hint.Line, documentLine.Length + 1),
                VisualYPosition.TextMiddle);
            var text = new FormattedText(
                hint.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                Math.Max(11, _editor.FontSize - 2),
                foreground,
                pixelsPerDip);

            var x = endPoint.X + 22;
            var y = endPoint.Y - text.Height / 2;
            var rect = new Rect(x - 6, y - 2, text.Width + 12, text.Height + 4);
            drawingContext.DrawRoundedRectangle(background, null, rect, 4, 4);
            drawingContext.DrawText(text, new Point(x, y));
        }
    }
}

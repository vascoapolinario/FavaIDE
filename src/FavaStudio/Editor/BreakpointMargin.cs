using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class BreakpointMargin : AbstractMargin
{
    private HashSet<int> _breakpoints = [];

    private static readonly SolidColorBrush BreakpointFill;
    private static readonly Pen BreakpointBorder;

    static BreakpointMargin()
    {
        BreakpointFill = new SolidColorBrush(Color.FromRgb(0xCC, 0x22, 0x22));
        BreakpointFill.Freeze();
        var borderBrush = new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x00));
        borderBrush.Freeze();
        BreakpointBorder = new Pen(borderBrush, 0.8);
        BreakpointBorder.Freeze();
    }

    public event Action<int>? BreakpointToggled;

    public void SetBreakpoints(IEnumerable<int> lines)
    {
        _breakpoints = new HashSet<int>(lines);
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize) => new(18, 0);

    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView is not null)
            oldTextView.VisualLinesChanged -= OnVisualLinesChanged;
        if (newTextView is not null)
            newTextView.VisualLinesChanged += OnVisualLinesChanged;
        base.OnTextViewChanged(oldTextView, newTextView);
        InvalidateVisual();
    }

    private void OnVisualLinesChanged(object? sender, EventArgs e) => InvalidateVisual();

    protected override void OnRender(DrawingContext drawingContext)
    {
        var tv = TextView;
        if (tv is null || !tv.VisualLinesValid) return;

        foreach (var vl in tv.VisualLines)
        {
            var lineNum = vl.FirstDocumentLine.LineNumber;
            if (!_breakpoints.Contains(lineNum)) continue;

            var y = vl.VisualTop - tv.ScrollOffset.Y;
            var cy = y + vl.Height / 2;
            drawingContext.DrawEllipse(BreakpointFill, BreakpointBorder, new Point(9, cy), 6, 6);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        e.Handled = true;

        var tv = TextView;
        if (tv is null) return;

        var mouseY = e.GetPosition(this).Y + tv.ScrollOffset.Y;
        foreach (var vl in tv.VisualLines)
        {
            if (mouseY >= vl.VisualTop && mouseY < vl.VisualTop + vl.Height)
            {
                BreakpointToggled?.Invoke(vl.FirstDocumentLine.LineNumber);
                break;
            }
        }
    }
}

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
    private bool _isHovering;

    private static readonly SolidColorBrush BreakpointFill;
    private static readonly Pen BreakpointBorder;
    private static readonly SolidColorBrush LaneBrush;
    private static readonly SolidColorBrush LaneHoverBrush;
    private static readonly Pen LaneDividerPen;

    static BreakpointMargin()
    {
        BreakpointFill = new SolidColorBrush(Color.FromRgb(0xCC, 0x22, 0x22));
        BreakpointFill.Freeze();
        var borderBrush = new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x00));
        borderBrush.Freeze();
        BreakpointBorder = new Pen(borderBrush, 0.8);
        BreakpointBorder.Freeze();

        LaneBrush = new SolidColorBrush(Color.FromArgb(32, 170, 40, 40));
        LaneBrush.Freeze();
        LaneHoverBrush = new SolidColorBrush(Color.FromArgb(56, 180, 50, 50));
        LaneHoverBrush.Freeze();
        var laneDividerBrush = new SolidColorBrush(Color.FromArgb(120, 120, 70, 70));
        laneDividerBrush.Freeze();
        LaneDividerPen = new Pen(laneDividerBrush, 1);
        LaneDividerPen.Freeze();
    }

    public BreakpointMargin()
    {
        Cursor = Cursors.Hand;
        ToolTip = "Click to toggle breakpoint";
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

        drawingContext.DrawRectangle(_isHovering ? LaneHoverBrush : LaneBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));
        drawingContext.DrawLine(LaneDividerPen, new Point(ActualWidth - 0.5, 0), new Point(ActualWidth - 0.5, ActualHeight));

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

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovering = true;
        InvalidateVisual();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovering = false;
        InvalidateVisual();
    }
}

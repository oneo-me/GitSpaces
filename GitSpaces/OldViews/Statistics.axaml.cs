using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.OldViews;

public class Chart : Control
{
    public static readonly StyledProperty<IBrush> LineBrushProperty =
        AvaloniaProperty.Register<Chart, IBrush>(nameof(LineBrush), Brushes.Gray);

    public IBrush LineBrush
    {
        get => GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush> ShapeBrushProperty =
        AvaloniaProperty.Register<Chart, IBrush>(nameof(ShapeBrush), Brushes.Gray);

    public IBrush ShapeBrush
    {
        get => GetValue(ShapeBrushProperty);
        set => SetValue(ShapeBrushProperty, value);
    }

    public static readonly StyledProperty<List<StatisticsSample>> SamplesProperty =
        AvaloniaProperty.Register<Chart, List<StatisticsSample>>(nameof(Samples));

    public List<StatisticsSample> Samples
    {
        get => GetValue(SamplesProperty);
        set => SetValue(SamplesProperty, value);
    }

    static Chart()
    {
        SamplesProperty.Changed.AddClassHandler<Chart>((c, e) =>
        {
            c._hitBoxes.Clear();
            c._lastHitIdx = -1;
            c.InvalidateVisual();
        });
    }

    public override void Render(DrawingContext context)
    {
        if (Samples == null) return;

        var samples = Samples;
        var maxV = 0;
        foreach (var s in samples)
        {
            if (maxV < s.Count) maxV = s.Count;
        }

        if (maxV < 5)
        {
            maxV = 5;
        }
        else if (maxV < 10)
        {
            maxV = 10;
        }
        else if (maxV < 50)
        {
            maxV = 50;
        }
        else if (maxV < 100)
        {
            maxV = 100;
        }
        else if (maxV < 200)
        {
            maxV = 200;
        }
        else if (maxV < 500)
        {
            maxV = 500;
        }
        else
        {
            maxV = (int)Math.Ceiling(maxV / 500.0) * 500;
        }

        var typeface = new Typeface("fonts:GitSpaces#JetBrains Mono");
        var pen = new Pen(LineBrush);
        var width = Bounds.Width;
        var height = Bounds.Height;

        // Transparent background to block mouse move events.
        context.DrawRectangle(Brushes.Transparent, null, new(0, 0, Bounds.Width, Bounds.Height));

        // Draw coordinate
        var maxLabel = new FormattedText($"{maxV}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12.0, LineBrush);
        var horizonStart = maxLabel.Width + 8;
        var labelHeight = maxLabel.Height;
        context.DrawText(maxLabel, new(0, -maxLabel.Height * 0.5));
        context.DrawLine(pen, new(horizonStart, 0), new(horizonStart, height - labelHeight));
        context.DrawLine(pen, new(horizonStart, height - labelHeight), new(width, height - labelHeight));

        if (samples.Count == 0) return;

        // Draw horizontal lines
        var stepX = (width - horizonStart) / samples.Count;
        var stepV = (height - labelHeight) / 5;
        var labelStepV = maxV / 5;
        var gridPen = new Pen(LineBrush, 1, new DashStyle());
        for (var i = 1; i < 5; i++)
        {
            var vLabel = new FormattedText(
                $"{maxV - i * labelStepV}",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12.0,
                LineBrush);

            var dashHeight = i * stepV;
            var vy = Math.Max(0, dashHeight - vLabel.Height * 0.5);
            using (context.PushOpacity(.1))
            {
                context.DrawLine(gridPen, new(horizonStart + 1, dashHeight), new(width, dashHeight));
            }

            context.DrawText(vLabel, new(horizonStart - vLabel.Width - 8, vy));
        }

        // Calculate hit boxes
        if (_hitBoxes.Count == 0)
        {
            var shapeWidth = Math.Min(32, stepX - 4);
            for (var i = 0; i < samples.Count; i++)
            {
                var h = samples[i].Count * (height - labelHeight) / maxV;
                var x = horizonStart + 1 + stepX * i + (stepX - shapeWidth) * 0.5;
                var y = height - labelHeight - h;
                _hitBoxes.Add(new(x, y, shapeWidth, h - 1));
            }
        }

        // Draw shapes
        for (var i = 0; i < samples.Count; i++)
        {
            var hLabel = new FormattedText(
                samples[i].Name,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                10.0,
                LineBrush);
            var rect = _hitBoxes[i];
            var xLabel = rect.X - (hLabel.Width - rect.Width) * 0.5;
            var yLabel = height - labelHeight + 4;

            context.DrawRectangle(ShapeBrush, null, rect);

            if (stepX < 32)
            {
                var matrix = Matrix.CreateTranslation(hLabel.Width * 0.5, -hLabel.Height * 0.5) // Center of label
                             * Matrix.CreateRotation(Math.PI * 0.25) // Rotate
                             * Matrix.CreateTranslation(xLabel, yLabel); // Move
                using (context.PushTransform(matrix))
                {
                    context.DrawText(hLabel, new(0, 0));
                }
            }
            else
            {
                context.DrawText(hLabel, new(xLabel, yLabel));
            }
        }

        // Draw labels on hover
        if (_lastHitIdx >= 0 && _lastHitIdx < samples.Count)
        {
            var rect = _hitBoxes[_lastHitIdx];
            var tooltip = new FormattedText(
                $"{samples[_lastHitIdx].Count}",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12.0,
                LineBrush);

            var tx = rect.X - (tooltip.Width - rect.Width) * 0.5;
            var ty = rect.Y - tooltip.Height - 4;
            context.DrawText(tooltip, new(tx, ty));
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var p = e.GetPosition(this);
        for (var i = 0; i < _hitBoxes.Count; i++)
        {
            if (_hitBoxes[i].Contains(p))
            {
                if (_lastHitIdx != i)
                {
                    _lastHitIdx = i;
                    InvalidateVisual();
                }

                return;
            }
        }

        if (_lastHitIdx != -1)
        {
            _lastHitIdx = -1;
            InvalidateVisual();
        }
    }

    readonly List<Rect> _hitBoxes = new();
    int _lastHitIdx = -1;
}

public partial class Statistics : Window
{
    public Statistics()
    {
        InitializeComponent();
    }

    void BeginMoveWindow(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

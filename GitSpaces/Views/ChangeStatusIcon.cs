using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.Views;

public class ChangeStatusIcon : Control
{
    static readonly IBrush[] BACKGROUNDS =
    [
        Brushes.Transparent,
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Color.FromRgb(238, 160, 14), 0), new(Color.FromRgb(228, 172, 67), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        },
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Color.FromRgb(47, 185, 47), 0), new(Color.FromRgb(75, 189, 75), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        },
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Colors.Tomato, 0), new(Color.FromRgb(252, 165, 150), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        },
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Colors.Orchid, 0), new(Color.FromRgb(248, 161, 245), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        },
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Color.FromRgb(238, 160, 14), 0), new(Color.FromRgb(228, 172, 67), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        },
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Color.FromRgb(238, 160, 14), 0), new(Color.FromRgb(228, 172, 67), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        },
        new LinearGradientBrush
        {
            GradientStops = new()
            {
                new(Color.FromRgb(47, 185, 47), 0), new(Color.FromRgb(75, 189, 75), 1)
            },
            StartPoint = new(0, 0, RelativeUnit.Relative),
            EndPoint = new(0, 1, RelativeUnit.Relative)
        }
    ];

    static readonly string[] INDICATOR = ["?", "±", "+", "−", "➜", "❏", "U", "★"];

    public static readonly StyledProperty<bool> IsWorkingCopyChangeProperty =
        AvaloniaProperty.Register<Avatar, bool>(nameof(IsWorkingCopyChange));

    public bool IsWorkingCopyChange
    {
        get => GetValue(IsWorkingCopyChangeProperty);
        set => SetValue(IsWorkingCopyChangeProperty, value);
    }

    public static readonly StyledProperty<Change> ChangeProperty =
        AvaloniaProperty.Register<Avatar, Change>(nameof(Change));

    public Change Change
    {
        get => GetValue(ChangeProperty);
        set => SetValue(ChangeProperty, value);
    }

    static ChangeStatusIcon()
    {
        AffectsRender<ChangeStatusIcon>(IsWorkingCopyChangeProperty, ChangeProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (Change == null || Bounds.Width <= 0) return;

        var typeface = new Typeface("fonts:GitSpaces#JetBrains Mono");

        IBrush background = null;
        string indicator;
        if (IsWorkingCopyChange)
        {
            if (Change.IsConflit)
            {
                background = Brushes.OrangeRed;
                indicator = "!";
            }
            else
            {
                background = BACKGROUNDS[(int)Change.WorkTree];
                indicator = INDICATOR[(int)Change.WorkTree];
            }
        }
        else
        {
            background = BACKGROUNDS[(int)Change.Index];
            indicator = INDICATOR[(int)Change.Index];
        }

        var txt = new FormattedText(
            indicator,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            Bounds.Width * 0.8,
            Brushes.White);

        var corner = (float)Math.Max(2, Bounds.Width / 16);
        var textOrigin = new Point((Bounds.Width - txt.Width) * 0.5, (Bounds.Height - txt.Height) * 0.5);
        context.DrawRectangle(background, null, new(0, 0, Bounds.Width, Bounds.Height), corner, corner);
        context.DrawText(txt, textOrigin);
    }
}

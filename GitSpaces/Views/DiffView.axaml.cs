using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace GitSpaces.Views;

public class ImageDiffView : Control
{
    public static readonly StyledProperty<double> AlphaProperty =
        AvaloniaProperty.Register<ImageDiffView, double>(nameof(Alpha), 0.5);

    public double Alpha
    {
        get => GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, value);
    }

    public static readonly StyledProperty<Bitmap> OldImageProperty =
        AvaloniaProperty.Register<ImageDiffView, Bitmap>(nameof(OldImage));

    public Bitmap OldImage
    {
        get => GetValue(OldImageProperty);
        set => SetValue(OldImageProperty, value);
    }

    public static readonly StyledProperty<Bitmap> NewImageProperty =
        AvaloniaProperty.Register<ImageDiffView, Bitmap>(nameof(NewImage));

    public Bitmap NewImage
    {
        get => GetValue(NewImageProperty);
        set => SetValue(NewImageProperty, value);
    }

    static ImageDiffView()
    {
        AffectsMeasure<ImageDiffView>(OldImageProperty, NewImageProperty);
        AffectsRender<ImageDiffView>(AlphaProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (_bgBrush == null)
        {
            var maskBrush = new SolidColorBrush(ActualThemeVariant == ThemeVariant.Dark ? 0xFF404040 : 0xFFBBBBBB);
            var bg = new DrawingGroup
            {
                Children =
                {
                    new GeometryDrawing
                    {
                        Brush = maskBrush, Geometry = new RectangleGeometry(new(0, 0, 12, 12))
                    },
                    new GeometryDrawing
                    {
                        Brush = maskBrush, Geometry = new RectangleGeometry(new(12, 12, 12, 12))
                    }
                }
            };

            _bgBrush = new(bg)
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                DestinationRect = new(new Size(24, 24), RelativeUnit.Absolute),
                Stretch = Stretch.None,
                TileMode = TileMode.Tile
            };
        }

        context.FillRectangle(_bgBrush, new(Bounds.Size));

        var alpha = Alpha;
        var w = Bounds.Width - 16;
        var h = Bounds.Height - 16;
        var x = w * alpha;
        var left = OldImage;
        if (left != null && alpha > 0)
        {
            var src = new Rect(0, 0, left.Size.Width * alpha, left.Size.Height);
            var dst = new Rect(8, 8, x, h);
            context.DrawImage(left, src, dst);
        }

        var right = NewImage;
        if (right != null && alpha < 1)
        {
            var src = new Rect(right.Size.Width * alpha, 0, right.Size.Width * (1 - alpha), right.Size.Height);
            var dst = new Rect(x + 8, 8, w - x, h);
            context.DrawImage(right, src, dst);
        }

        context.DrawLine(new Pen(Brushes.DarkGreen, 2), new(x + 8, 0), new(x + 8, Bounds.Height));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == "ActualThemeVariant")
        {
            _bgBrush = null;
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var left = OldImage;
        var right = NewImage;

        if (left != null)
        {
            var lSize = GetDesiredSize(left.Size, availableSize);
            if (right != null)
            {
                var rSize = GetDesiredSize(right.Size, availableSize);
                if (rSize.Width > lSize.Width) return rSize;
                return lSize;
            }

            return lSize;
        }

        if (right != null)
        {
            return GetDesiredSize(right.Size, availableSize);
        }

        return availableSize;
    }

    Size GetDesiredSize(Size img, Size available)
    {
        var w = available.Width - 16;
        var h = available.Height - 16;

        if (img.Width <= w)
        {
            if (img.Height <= h)
            {
                return new(img.Width + 16, img.Height + 16);
            }

            return new(h * img.Width / img.Height + 16, available.Height);
        }

        var s = Math.Max(img.Width / w, img.Height / h);
        return new(img.Width / s + 16, img.Height / s + 16);
    }

    DrawingBrush _bgBrush;
}

public partial class DiffView : UserControl
{
    public DiffView()
    {
        InitializeComponent();
    }
}

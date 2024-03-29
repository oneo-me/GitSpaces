using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using GitSpaces.Models;
using Object = GitSpaces.Models.Object;
using Path = Avalonia.Controls.Shapes.Path;

namespace GitSpaces.Views;

public class RevisionImageFileView : Control
{
    public static readonly StyledProperty<Bitmap> SourceProperty =
        AvaloniaProperty.Register<ImageDiffView, Bitmap>(nameof(Source));

    public Bitmap Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    static RevisionImageFileView()
    {
        AffectsMeasure<RevisionImageFileView>(SourceProperty);
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

        var source = Source;
        if (source != null)
        {
            context.DrawImage(source, new(source.Size), new(8, 8, Bounds.Width - 16, Bounds.Height - 16));
        }
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
        var source = Source;
        if (source == null)
        {
            return availableSize;
        }

        var w = availableSize.Width - 16;
        var h = availableSize.Height - 16;
        var size = source.Size;
        if (size.Width <= w)
        {
            if (size.Height <= h)
            {
                return new(size.Width + 16, size.Height + 16);
            }

            return new(h * size.Width / size.Height + 16, availableSize.Height);
        }

        var scale = Math.Max(size.Width / w, size.Height / h);
        return new(size.Width / scale + 16, size.Height / scale + 16);
    }

    DrawingBrush _bgBrush;
}

public class RevisionTextFileView : TextEditor
{
    protected override Type StyleKeyOverride => typeof(TextEditor);

    public RevisionTextFileView() : base(new(), new())
    {
        IsReadOnly = true;
        ShowLineNumbers = true;
        WordWrap = false;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

        TextArea.LeftMargins[0].Margin = new(8, 0);
        TextArea.TextView.Margin = new(4, 0);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TextArea.TextView.ContextRequested += OnTextViewContextRequested;

        _textMate = TextMateHelper.CreateForEditor(this);
        if (DataContext is RevisionTextFile source)
        {
            TextMateHelper.SetGrammarByFileName(_textMate, source.FileName);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        TextArea.TextView.ContextRequested -= OnTextViewContextRequested;

        if (_textMate != null)
        {
            _textMate.Dispose();
            _textMate = null;
        }

        GC.Collect();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        var source = DataContext as RevisionTextFile;
        if (source != null)
        {
            Text = source.Content;
            TextMateHelper.SetGrammarByFileName(_textMate, source.FileName);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == "ActualThemeVariant" && change.NewValue != null)
        {
            TextMateHelper.SetThemeByApp(_textMate);
        }
    }

    void OnTextViewContextRequested(object sender, ContextRequestedEventArgs e)
    {
        var selected = SelectedText;
        if (string.IsNullOrEmpty(selected)) return;

        var icon = new Path();
        icon.Width = 10;
        icon.Height = 10;
        icon.Stretch = Stretch.Uniform;
        icon.Data = Application.Current?.FindResource("Icons.Copy") as StreamGeometry;

        var copy = new MenuItem();
        copy.Header = App.Text("Copy");
        copy.Icon = icon;
        copy.Click += (o, ev) =>
        {
            App.CopyText(selected);
            ev.Handled = true;
        };

        var menu = new ContextMenu();
        menu.Items.Add(copy);
        menu.Open(TextArea.TextView);
        e.Handled = true;
    }

    TextMate.Installation _textMate;
}

public partial class RevisionFiles : UserControl
{
    public RevisionFiles()
    {
        InitializeComponent();
    }

    void OnTreeViewContextRequested(object sender, ContextRequestedEventArgs e)
    {
        var detail = DataContext as ViewModels.CommitDetail;
        var node = detail.SelectedRevisionFileNode;
        if (!node.IsFolder)
        {
            var menu = detail.CreateRevisionFileContextMenu(node.Backend as Object);
            menu.Open(sender as Control);
        }

        e.Handled = true;
    }
}

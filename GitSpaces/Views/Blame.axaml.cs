using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Utils;
using GitSpaces.Models;
using Path = Avalonia.Controls.Shapes.Path;

namespace GitSpaces.Views;

public class BlameTextEditor : TextEditor
{
    public class CommitInfoMargin : AbstractMargin
    {
        public CommitInfoMargin(BlameTextEditor editor)
        {
            _editor = editor;
            ClipToBounds = true;
        }

        public override void Render(DrawingContext context)
        {
            if (_editor.BlameData == null) return;

            var view = TextView;
            if (view != null && view.VisualLinesValid)
            {
                var typeface = view.CreateTypeface();
                var underlinePen = new Pen(Brushes.DarkOrange);

                foreach (var line in view.VisualLines)
                {
                    var lineNumber = line.FirstDocumentLine.LineNumber;
                    if (lineNumber > _editor.BlameData.LineInfos.Count) break;

                    var info = _editor.BlameData.LineInfos[lineNumber - 1];
                    var x = 0.0;
                    var y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop) - view.VerticalOffset;
                    if (!info.IsFirstInGroup && y > view.DefaultLineHeight * 0.6) continue;

                    var shaLink = new FormattedText(
                        info.CommitSHA,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        Brushes.DarkOrange);
                    context.DrawText(shaLink, new(x, y));
                    context.DrawLine(underlinePen, new(x, y + shaLink.Baseline + 2), new(x + shaLink.Width, y + shaLink.Baseline + 2));
                    x += shaLink.Width + 8;

                    var time = new FormattedText(
                        info.Time,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        _editor.Foreground);
                    context.DrawText(time, new(x, y));
                    x += time.Width + 8;

                    var author = new FormattedText(
                        info.Author,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        _editor.Foreground);
                    context.DrawText(author, new(x, y));
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var view = TextView;
            var maxWidth = 0.0;
            if (view != null && view.VisualLinesValid && _editor.BlameData != null)
            {
                var typeface = view.CreateTypeface();
                var calculated = new HashSet<string>();
                foreach (var line in view.VisualLines)
                {
                    var lineNumber = line.FirstDocumentLine.LineNumber;
                    if (lineNumber > _editor.BlameData.LineInfos.Count) break;

                    var info = _editor.BlameData.LineInfos[lineNumber - 1];

                    if (calculated.Contains(info.CommitSHA)) continue;
                    calculated.Add(info.CommitSHA);

                    var x = 0.0;
                    var shaLink = new FormattedText(
                        info.CommitSHA,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        Brushes.DarkOrange);
                    x += shaLink.Width + 8;

                    var time = new FormattedText(
                        info.Time,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        _editor.Foreground);
                    x += time.Width + 8;

                    var author = new FormattedText(
                        info.Author,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        _editor.Foreground);
                    x += author.Width;

                    if (maxWidth < x) maxWidth = x;
                }
            }

            return new(maxWidth, 0);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            var view = TextView;
            if (!e.Handled && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && view != null && view.VisualLinesValid)
            {
                var pos = e.GetPosition(this);
                var typeface = view.CreateTypeface();

                foreach (var line in view.VisualLines)
                {
                    var lineNumber = line.FirstDocumentLine.LineNumber;
                    if (lineNumber >= _editor.BlameData.LineInfos.Count) break;

                    var info = _editor.BlameData.LineInfos[lineNumber - 1];
                    var y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop) - view.VerticalOffset;
                    var shaLink = new FormattedText(
                        info.CommitSHA,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        _editor.FontSize,
                        Brushes.DarkOrange);

                    var rect = new Rect(0, y, shaLink.Width, shaLink.Height);
                    if (rect.Contains(pos))
                    {
                        _editor.OnCommitSHAClicked(info.CommitSHA);
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        readonly BlameTextEditor _editor;
    }

    public class VerticalSeperatorMargin : AbstractMargin
    {
        public VerticalSeperatorMargin(BlameTextEditor editor)
        {
            _editor = editor;
        }

        public override void Render(DrawingContext context)
        {
            var pen = new Pen(_editor.BorderBrush);
            context.DrawLine(pen, new(0, 0), new(0, Bounds.Height));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new(1, 0);
        }

        readonly BlameTextEditor _editor;
    }

    public static readonly StyledProperty<BlameData> BlameDataProperty =
        AvaloniaProperty.Register<BlameTextEditor, BlameData>(nameof(BlameData));

    public BlameData BlameData
    {
        get => GetValue(BlameDataProperty);
        set => SetValue(BlameDataProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextEditor);

    public BlameTextEditor() : base(new(), new())
    {
        IsReadOnly = true;
        ShowLineNumbers = false;
        WordWrap = false;

        _textMate = TextMateHelper.CreateForEditor(this);

        TextArea.LeftMargins.Add(new LineNumberMargin
        {
            Margin = new(8, 0)
        });
        TextArea.LeftMargins.Add(new VerticalSeperatorMargin(this));
        TextArea.LeftMargins.Add(new CommitInfoMargin(this)
        {
            Margin = new(8, 0)
        });
        TextArea.LeftMargins.Add(new VerticalSeperatorMargin(this));
        TextArea.TextView.ContextRequested += OnTextViewContextRequested;
        TextArea.TextView.VisualLinesChanged += OnTextViewVisualLinesChanged;
        TextArea.TextView.Margin = new(4, 0);
    }

    public void OnCommitSHAClicked(string sha)
    {
        if (DataContext is ViewModels.Blame blame)
        {
            blame.NavigateToCommit(sha);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        TextArea.LeftMargins.Clear();
        TextArea.TextView.ContextRequested -= OnTextViewContextRequested;
        TextArea.TextView.VisualLinesChanged -= OnTextViewVisualLinesChanged;

        if (_textMate != null)
        {
            _textMate.Dispose();
            _textMate = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BlameDataProperty)
        {
            if (BlameData != null)
            {
                TextMateHelper.SetGrammarByFileName(_textMate, BlameData.File);
                Text = BlameData.Content;
            }
            else
            {
                Text = string.Empty;
            }
        }
        else if (change.Property.Name == "ActualThemeVariant" && change.NewValue != null)
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

    void OnTextViewVisualLinesChanged(object sender, EventArgs e)
    {
        foreach (var margin in TextArea.LeftMargins)
        {
            if (margin is CommitInfoMargin commitInfo)
            {
                commitInfo.InvalidateMeasure();
                break;
            }
        }
    }

    TextMate.Installation _textMate;
}

public partial class Blame : Window
{
    public Blame()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Owner = desktop.MainWindow;
        }

        InitializeComponent();
    }

    void MaximizeOrRestoreWindow(object sender, TappedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }

        e.Handled = true;
    }

    void CustomResizeWindow(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border border)
        {
            if (border.Tag is WindowEdge edge)
            {
                BeginResizeDrag(edge, e);
            }
        }
    }

    void BeginMoveWindow(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        GC.Collect();
    }

    void OnCommitSHAPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.Blame blame)
        {
            var txt = sender as TextBlock;
            blame.NavigateToCommit(txt.Text);
        }

        e.Handled = true;
    }
}

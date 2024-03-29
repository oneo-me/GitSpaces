using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GitSpaces.Views;

public partial class CaptionButtonsMacOS : UserControl
{
    public CaptionButtonsMacOS()
    {
        InitializeComponent();
    }

    void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        if (window != null)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    void MaximizeOrRestoreWindow(object sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        if (window != null)
        {
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }

    void CloseWindow(object sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        if (window != null)
        {
            window.Close();
        }
    }
}

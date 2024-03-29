using Avalonia.Controls;
using Avalonia.Input;

namespace GitSpaces.Views;

public partial class FileHistories : Window
{
    public FileHistories()
    {
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
}

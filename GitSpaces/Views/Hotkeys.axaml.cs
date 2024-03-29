using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace GitSpaces.Views;

public partial class Hotkeys : Window
{
    public Hotkeys()
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

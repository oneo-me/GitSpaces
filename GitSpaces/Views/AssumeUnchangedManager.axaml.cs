using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace GitSpaces.Views;

public partial class AssumeUnchangedManager : Window
{
    public AssumeUnchangedManager()
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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitSpaces.Models;
using GitSpaces.Native;

namespace GitSpaces.Views;

public partial class SelfUpdate : Window
{
    public SelfUpdate()
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

    void GotoDownload(object sender, RoutedEventArgs e)
    {
        OS.OpenBrowser("https://github.com/sourcegit-scm/sourcegit/releases/latest");
        e.Handled = true;
    }

    void IgnoreThisVersion(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var ver = button.DataContext as Version;
        ViewModels.Preference.Instance.IgnoreUpdateTag = ver.TagName;
        Close();
        e.Handled = true;
    }
}

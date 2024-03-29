using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitSpaces.Services;
using OpenUI.Services;

namespace GitSpaces.OldViews;

public partial class About : Window
{
    public string Version { get; private set; }

    public About()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        Version = $"{ver.Major}.{ver.Minor}";
        DataContext = this;
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

    void OnVisitAvaloniaUI(object sender, PointerPressedEventArgs e)
    {
        var OS = Service.Get<ISystemService>();
        OS.OpenBrowser("https://www.avaloniaui.net/");
        e.Handled = true;
    }

    void OnVisitAvaloniaEdit(object sender, PointerPressedEventArgs e)
    {
        var OS = Service.Get<ISystemService>();
        OS.OpenBrowser("https://github.com/AvaloniaUI/AvaloniaEdit");
        e.Handled = true;
    }

    void OnVisitJetBrainsMonoFont(object sender, PointerPressedEventArgs e)
    {
        var OS = Service.Get<ISystemService>();
        OS.OpenBrowser("https://www.jetbrains.com/lp/mono/");
        e.Handled = true;
    }
}

using Avalonia.Controls;
using Avalonia.Platform;
using GitSpaces.Configs;
using GitSpaces.Services;
using OpenUI.Reactive;
using OpenUI.Services;

namespace GitSpaces.Views;

[View<MainWindow>]
public class MainWindow_Model : WindowModel
{
    readonly ConfigService _configService = Service.Get<ConfigService>();
    readonly WindowConfig _windowConfig;

    public MainWindow_Model()
    {
        _windowConfig = _configService.Get<WindowConfig>();

        using var stream = AssetLoader.Open(new("avares://GitSpaces/Assets/Icon.ico"));
        Icon = new(stream);
        Title = nameof(GitSpaces);

        var defaultWindowConfig = new WindowConfig();

        MinWidth = defaultWindowConfig.Width;
        MinHeight = defaultWindowConfig.Height;

        Left = _windowConfig.Left;
        Top = _windowConfig.Top;
        Width = _windowConfig.Width;
        Height = _windowConfig.Height;
        WindowState = _windowConfig.WindowState;
    }

    protected override void OnClosed()
    {
        base.OnClosed();

        if (WindowState == WindowState.Normal)
        {
            _windowConfig.Left = Left;
            _windowConfig.Top = Top;
            _windowConfig.Width = Width;
            _windowConfig.Height = Height;
        }

        _windowConfig.WindowState = WindowState;

        _configService.Save();
    }
}

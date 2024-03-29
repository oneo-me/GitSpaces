using System.Collections;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using GitSpaces.Configs;
using GitSpaces.Models;
using GitSpaces.Resources;
using GitSpaces.Services;
using GitSpaces.Views;
using OpenUI;
using OpenUI.Reactive;
using OpenUI.Services;
using Path = Avalonia.Controls.Shapes.Path;

namespace GitSpaces;

public class App : AppBase
{
    public App()
    {
        Global.App = this;
        Name = nameof(GitSpaces);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override WindowModel CreateMain()
    {
        return new MainWindow_Model();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Service.Add(new ConfigService());
        Service.Add(new UpdateService());

        base.OnFrameworkInitializationCompleted();
    }
}

public class App123 : Application
{
    // static App123()
    // {
    //     OS.SetupApp(builder);
    // }

    public static void RaiseException(string context, string message)
    {
        if (Current is App123 app && app._notificationReceiver != null)
        {
            var notice = new Notification
            {
                IsError = true, Message = message
            };
            app._notificationReceiver.OnReceiveNotification(context, notice);
        }
    }

    public static void SendNotification(string context, string message)
    {
        if (Current is App123 app && app._notificationReceiver != null)
        {
            var notice = new Notification
            {
                IsError = false, Message = message
            };
            app._notificationReceiver.OnReceiveNotification(context, notice);
        }
    }

    public static void SetLocale(string localeKey)
    {
        var app = Current as App123;
        var rd = new ResourceDictionary();

        var culture = CultureInfo.GetCultureInfo(localeKey.Replace("_", "-"));
        Locales.Culture = culture;

        var sets = Locales.ResourceManager.GetResourceSet(culture, true, true);
        foreach (var obj in sets)
            if (obj is DictionaryEntry entry)
                rd.Add(entry.Key, entry.Value);

        if (app._activeLocale != null)
            app.Resources.MergedDictionaries.Remove(app._activeLocale);

        app.Resources.MergedDictionaries.Add(rd);
        app._activeLocale = rd;
    }

    public static void SetTheme(string theme)
    {
        if (theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
            Current.RequestedThemeVariant = ThemeVariant.Light;
        else if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
            Current.RequestedThemeVariant = ThemeVariant.Dark;
        else
            Current.RequestedThemeVariant = ThemeVariant.Default;
    }

    public static async void CopyText(string data)
    {
        if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            if (desktop.MainWindow.Clipboard is { } clipbord)
                await clipbord.SetTextAsync(data);
    }

    public static string Text(string key, params object[] args)
    {
        var fmt = Current.FindResource($"Text.{key}") as string;
        if (string.IsNullOrWhiteSpace(fmt)) return $"Text.{key}";
        return string.Format(fmt, args);
    }

    public static Path CreateMenuIcon(string key)
    {
        var icon = new Path();
        icon.Width = 12;
        icon.Height = 12;
        icon.Stretch = Stretch.Uniform;
        icon.Data = Current.FindResource(key) as StreamGeometry;
        return icon;
    }

    public static TopLevel GetTopLevel()
    {
        if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }

    public static void Quit()
    {
        if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Close();
            desktop.Shutdown();
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var pref = Preference.Instance;

        SetLocale(pref.Locale);
        SetTheme(pref.Theme);
    }

    ResourceDictionary _activeLocale;
    INotificationReceiver _notificationReceiver;
}

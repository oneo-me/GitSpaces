using System.Reflection;
using Avalonia;
using GitSpaces.Logging;
using GitSpaces.Native;
using OpenUI.Desktop;

namespace GitSpaces;

static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Debug("Crash: ");
            Log.Debug(e.Message);
            Log.Debug("----------------------------");
            Log.Debug($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
            Log.Debug($"OS: {Environment.OSVersion}");
            Log.Debug($"Framework: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}");
            Log.Debug($"Source: {e.Source}");
            Log.Debug("---------------------------");
            Log.Debug(e.StackTrace);
        }
    }

    static AppBuilder BuildAvaloniaApp()
    {
        var builder = DesktopApp.Build<App>();

        OS.SetupApp(builder);

        return builder;
    }
}

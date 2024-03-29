using System.Text.Json;
using GitSpaces.Configs;
using GitSpaces.Models;

namespace GitSpaces.Services;

public class UpdateService
{
    public async Task CheckUpdateAsync(bool manually = false)
    {
        try
        {
            // Fetch lastest release information.
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            var data = await client.GetStringAsync("https://api.github.com/repos/sourcegit-scm/sourcegit/releases/latest");

            // Parse json into Models.Version.
            var ver = JsonSerializer.Deserialize(data, JsonCodeGen.Default.Version);
            if (ver == null) return;

            // Check if already up-to-date.
            if (!ver.IsNewVersion)
            {
                if (manually)
                    ShowSelfUpdateResult(new AlreadyUpToDate());
                return;
            }

            // Should not check ignored tag if this is called manually.
            if (!manually)
            {
                var pref = Preference.Instance;
                if (ver.TagName == pref.IgnoreUpdateTag) return;
            }

            ShowSelfUpdateResult(ver);
        }
        catch (Exception e)
        {
            if (manually) ShowSelfUpdateResult(e);
        }
    }

    static void ShowSelfUpdateResult(object data)
    {
        // TODO 显示更新界面
        // Dispatcher.UIThread.Post(() =>
        // {
        //     if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //     {
        //         var dialog = new SelfUpdate
        //         {
        //             DataContext = new SelfUpdate
        //             {
        //                 Data = data
        //             }
        //         };
        //
        //         dialog.Show(desktop.MainWindow);
        //     }
        // });
    }
}

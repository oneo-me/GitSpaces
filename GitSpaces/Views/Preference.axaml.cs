using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitSpaces.Commands;
using GitSpaces.Models;
using Version = GitSpaces.Commands.Version;

namespace GitSpaces.Views;

public partial class Preference : Window
{
    public AvaloniaList<FontFamily> InstalledFonts { get; }

    public AvaloniaList<FontFamily> InstalledMonospaceFonts { get; }

    public string DefaultUser { get; set; }

    public string DefaultEmail { get; set; }

    public CRLFMode CRLFMode { get; set; }

    public bool EnableGPGSigning { get; set; }

    public string GPGExecutableFile { get; set; }

    public string GPGUserKey { get; set; }

    public Preference()
    {
        var pref = ViewModels.Preference.Instance;
        DataContext = pref;

        var builtInMono = new FontFamily("fonts:GitSpaces#JetBrains Mono");

        InstalledFonts = new();
        InstalledFonts.Add(builtInMono);
        InstalledFonts.AddRange(FontManager.Current.SystemFonts);

        InstalledMonospaceFonts = new();
        InstalledMonospaceFonts.Add(builtInMono);

        var curMonoFont = pref.MonospaceFont;
        if (curMonoFont != builtInMono)
        {
            InstalledMonospaceFonts.Add(curMonoFont);
        }

        Task.Run(() =>
        {
            var sysMonoFonts = new List<FontFamily>();
            foreach (var font in FontManager.Current.SystemFonts)
            {
                if (font == curMonoFont) continue;

                var typeface = new Typeface(font);
                var testI = new FormattedText(
                    "i",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    12,
                    Brushes.White);
                var testW = new FormattedText(
                    "W",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    12,
                    Brushes.White);
                if (testI.Width == testW.Width)
                {
                    sysMonoFonts.Add(font);
                }
            }

            Dispatcher.UIThread.Post(() => InstalledMonospaceFonts.AddRange(sysMonoFonts));
        });

        var ver = string.Empty;
        if (pref.IsGitConfigured)
        {
            var config = new Config(null).ListAll();

            if (config.ContainsKey("user.name")) DefaultUser = config["user.name"];
            if (config.ContainsKey("user.email")) DefaultEmail = config["user.email"];
            if (config.ContainsKey("user.signingkey")) GPGUserKey = config["user.signingkey"];
            if (config.ContainsKey("core.autocrlf")) CRLFMode = CRLFMode.Supported.Find(x => x.Value == config["core.autocrlf"]);
            if (config.ContainsKey("commit.gpgsign")) EnableGPGSigning = config["commit.gpgsign"] == "true";
            if (config.ContainsKey("gpg.program")) GPGExecutableFile = config["gpg.program"];

            ver = new Version().Query();
        }

        InitializeComponent();
        txtVersion.Text = ver;
    }

    void BeginMoveWindow(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    void CloseWindow(object sender, RoutedEventArgs e)
    {
        var cmd = new Config(null);

        var config = cmd.ListAll();
        var oldUser = config.ContainsKey("user.name") ? config["user.name"] : string.Empty;
        var oldEmail = config.ContainsKey("user.email") ? config["user.email"] : string.Empty;
        var oldGPGSignKey = config.ContainsKey("user.signingkey") ? config["user.signingkey"] : string.Empty;
        var oldCRLF = config.ContainsKey("core.autocrlf") ? config["core.autocrlf"] : string.Empty;
        var oldGPGSignEnable = config.ContainsKey("commit.gpgsign") ? config["commit.gpgsign"] : "false";
        var oldGPGExec = config.ContainsKey("gpg.program") ? config["gpg.program"] : string.Empty;

        if (DefaultUser != oldUser) cmd.Set("user.name", DefaultUser);
        if (DefaultEmail != oldEmail) cmd.Set("user.email", DefaultEmail);
        if (GPGUserKey != oldGPGSignKey) cmd.Set("user.signingkey", GPGUserKey);
        if (CRLFMode != null && CRLFMode.Value != oldCRLF) cmd.Set("core.autocrlf", CRLFMode.Value);
        if (EnableGPGSigning != (oldGPGSignEnable == "true")) cmd.Set("commit.gpgsign", EnableGPGSigning ? "true" : "false");
        if (GPGExecutableFile != oldGPGExec) cmd.Set("gpg.program", GPGExecutableFile);

        Close();
    }

    async void SelectGitExecutable(object sender, RoutedEventArgs e)
    {
        var pattern = OperatingSystem.IsWindows() ? "git.exe" : "git";
        var options = new FilePickerOpenOptions
        {
            FileTypeFilter =
            [
                new("Git Executable")
                {
                    Patterns = [pattern]
                }
            ],
            AllowMultiple = false
        };

        var selected = await StorageProvider.OpenFilePickerAsync(options);
        if (selected.Count == 1)
        {
            ViewModels.Preference.Instance.GitInstallPath = selected[0].Path.LocalPath;
            txtVersion.Text = new Version().Query();
        }

        e.Handled = true;
    }

    async void SelectDefaultCloneDir(object sender, RoutedEventArgs e)
    {
        var options = new FolderPickerOpenOptions
        {
            AllowMultiple = false
        };
        var selected = await StorageProvider.OpenFolderPickerAsync(options);
        if (selected.Count == 1)
        {
            ViewModels.Preference.Instance.GitDefaultCloneDir = selected[0].Path.LocalPath;
        }
    }

    async void SelectGPGExecutable(object sender, RoutedEventArgs e)
    {
        var pattern = OperatingSystem.IsWindows() ? "gpg.exe" : "gpg";
        var options = new FilePickerOpenOptions
        {
            FileTypeFilter =
            [
                new("GPG Executable")
                {
                    Patterns = [pattern]
                }
            ],
            AllowMultiple = false
        };

        var selected = await StorageProvider.OpenFilePickerAsync(options);
        if (selected.Count == 1)
        {
            GPGExecutableFile = selected[0].Path.LocalPath;
        }
    }

    async void SelectExternalMergeTool(object sender, RoutedEventArgs e)
    {
        var type = ViewModels.Preference.Instance.ExternalMergeToolType;
        if (type < 0 || type >= ExternalMergeTools.Supported.Count)
        {
            ViewModels.Preference.Instance.ExternalMergeToolType = 0;
            type = 0;
        }

        var tool = ExternalMergeTools.Supported[type];
        var pattern = Path.GetFileName(tool.Exec);
        var options = new FilePickerOpenOptions
        {
            FileTypeFilter =
            [
                new(tool.Name)
                {
                    Patterns = [pattern]
                }
            ],
            AllowMultiple = false
        };

        var selected = await StorageProvider.OpenFilePickerAsync(options);
        if (selected.Count == 1)
        {
            ViewModels.Preference.Instance.ExternalMergeToolPath = selected[0].Path.LocalPath;
        }
    }
}

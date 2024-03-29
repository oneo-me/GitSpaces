﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Dialogs;
using Avalonia.Media;

namespace GitSpaces.Native;

[SupportedOSPlatform("linux")]
class Linux : OS.IBackend
{
    public void SetupApp(AppBuilder builder)
    {
        builder.With(new FontManagerOptions
        {
            DefaultFamilyName = "fonts:GitSpaces#JetBrains Mono"
        });

        // Free-desktop file picker has an extra black background panel.
        builder.UseManagedSystemDialogs();
    }

    public string FindGitExecutable()
    {
        if (File.Exists("/usr/bin/git")) return "/usr/bin/git";
        return string.Empty;
    }

    public string FindVSCode()
    {
        var toolPath = "/usr/share/code/code";
        if (File.Exists(toolPath)) return toolPath;
        return string.Empty;
    }

    public string FindFleet()
    {
        var toolPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.local/share/JetBrains/Toolbox/apps/fleet/bin/Fleet";
        if (File.Exists(toolPath)) return toolPath;
        return string.Empty;
    }

    public void OpenBrowser(string url)
    {
        if (!File.Exists("/usr/bin/xdg-open"))
        {
            App123.RaiseException("", "You should install xdg-open first!");
            return;
        }

        Process.Start("xdg-open", $"\"{url}\"");
    }

    public void OpenInFileManager(string path, bool select)
    {
        if (!File.Exists("/usr/bin/xdg-open"))
        {
            App123.RaiseException("", "You should install xdg-open first!");
            return;
        }

        if (Directory.Exists(path))
        {
            Process.Start("xdg-open", $"\"{path}\"");
        }
        else
        {
            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir))
            {
                Process.Start("xdg-open", $"\"{dir}\"");
            }
        }
    }

    public void OpenTerminal(string workdir)
    {
        var dir = string.IsNullOrEmpty(workdir) ? "~" : workdir;
        if (File.Exists("/usr/bin/gnome-terminal"))
        {
            Process.Start("/usr/bin/gnome-terminal", $"--working-directory=\"{dir}\"");
        }
        else if (File.Exists("/usr/bin/konsole"))
        {
            Process.Start("/usr/bin/konsole", $"--workdir \"{dir}\"");
        }
        else if (File.Exists("/usr/bin/xfce4-terminal"))
        {
            Process.Start("/usr/bin/xfce4-terminal", $"--working-directory=\"{dir}\"");
        }
        else
        {
            App123.RaiseException("", "Only supports gnome-terminal/konsole/xfce4-terminal!");
        }
    }

    public void OpenWithDefaultEditor(string file)
    {
        if (!File.Exists("/usr/bin/xdg-open"))
        {
            App123.RaiseException("", "You should install xdg-open first!");
            return;
        }

        var proc = Process.Start("xdg-open", $"\"{file}\"");
        proc.WaitForExit();

        if (proc.ExitCode != 0)
        {
            App123.RaiseException("", $"Failed to open \"{file}\"");
        }

        proc.Close();
    }
}

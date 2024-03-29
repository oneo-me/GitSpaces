﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using Avalonia;
using Avalonia.Media;

namespace GitSpaces.Native;

[SupportedOSPlatform("macOS")]
class MacOS : OS.IBackend
{
    public void SetupApp(AppBuilder builder)
    {
        builder.With(new FontManagerOptions
        {
            DefaultFamilyName = "PingFang SC"
        });
    }

    public string FindGitExecutable()
    {
        if (File.Exists("/usr/bin/git")) return "/usr/bin/git";
        return string.Empty;
    }

    public string FindVSCode()
    {
        var toolPath = "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code";
        if (File.Exists(toolPath))
            return toolPath;
        return string.Empty;
    }

    public string FindFleet()
    {
        var toolPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Applications/Fleet.app/Contents/MacOS/Fleet";
        if (File.Exists(toolPath))
            return toolPath;
        return string.Empty;
    }

    public void OpenBrowser(string url)
    {
        Process.Start("open", url);
    }

    public void OpenInFileManager(string path, bool select)
    {
        if (Directory.Exists(path))
        {
            Process.Start("open", path);
        }
        else if (File.Exists(path))
        {
            Process.Start("open", $"\"{path}\" -R");
        }
    }

    public void OpenTerminal(string workdir)
    {
        var dir = string.IsNullOrEmpty(workdir) ? "~" : workdir;
        var builder = new StringBuilder();
        builder.AppendLine("on run argv");
        builder.AppendLine("    tell application \"Terminal\"");
        builder.AppendLine($"        do script \"cd '{dir}'\"");
        builder.AppendLine("        activate");
        builder.AppendLine("    end tell");
        builder.AppendLine("end run");

        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, builder.ToString());

        var proc = Process.Start("/usr/bin/osascript", $"\"{tmp}\"");
        proc.Exited += (o, e) => File.Delete(tmp);
    }

    public void OpenWithDefaultEditor(string file)
    {
        Process.Start("open", file);
    }
}

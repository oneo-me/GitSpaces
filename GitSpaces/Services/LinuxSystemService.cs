using System.Diagnostics;
using System.Runtime.Versioning;

namespace GitSpaces.Services;

[SupportedOSPlatform("linux")]
public class LinuxSystemService : ISystemService
{
    public string GitInstallPath { get; set; }
    public string VSCodeExecutableFile { get; }

    public LinuxSystemService()
    {
        GitInstallPath = FindGitExecutable();
        VSCodeExecutableFile = FindVSCode();
    }

    public string FindGitExecutable()
    {
        if (File.Exists("/usr/bin/git")) return "/usr/bin/git";
        return string.Empty;
    }

    public string FindVSCode()
    {
        var toolPath = "/usr/share/code/code";
        if (File.Exists(toolPath))
            return toolPath;
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

    public void OpenTerminal(string workdir)
    {
        var dir = string.IsNullOrEmpty(workdir) ? "~" : workdir;
        if (File.Exists("/usr/bin/gnome-terminal"))
            Process.Start("/usr/bin/gnome-terminal", $"--working-directory=\"{dir}\"");
        else if (File.Exists("/usr/bin/konsole"))
            Process.Start("/usr/bin/konsole", $"--workdir \"{dir}\"");
        else if (File.Exists("/usr/bin/xfce4-terminal"))
            Process.Start("/usr/bin/xfce4-terminal", $"--working-directory=\"{dir}\"");
        else
            App123.RaiseException("", "Only supports gnome-terminal/konsole/xfce4-terminal!");
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
                Process.Start("xdg-open", $"\"{dir}\"");
        }
    }

    public void OpenInVSCode(string repo)
    {
        if (string.IsNullOrEmpty(VSCodeExecutableFile))
        {
            App123.RaiseException(repo, "Visual Studio Code can NOT be found in your system!!!");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = repo, FileName = VSCodeExecutableFile, Arguments = $"\"{repo}\"", UseShellExecute = false
        });
    }
}

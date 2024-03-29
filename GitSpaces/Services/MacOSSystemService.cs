using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Text;

namespace GitSpaces.Services;

[SupportedOSPlatform("osx")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MacOSSystemService : ISystemService
{
    public string GitInstallPath { get; set; }

    public string VSCodeExecutableFile { get; }

    public MacOSSystemService()
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
        var toolPath = "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code";
        if (File.Exists(toolPath))
            return toolPath;
        return string.Empty;
    }

    public void OpenBrowser(string url)
    {
        Process.Start("open", url);
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

    public void OpenInFileManager(string path, bool select)
    {
        if (Directory.Exists(path))
            Process.Start("open", path);
        else if (File.Exists(path))
            Process.Start("open", $"\"{path}\" -R");
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

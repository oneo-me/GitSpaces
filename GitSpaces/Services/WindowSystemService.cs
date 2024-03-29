using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Win32;

namespace GitSpaces.Services;

[SupportedOSPlatform("windows")]
public class WindowSystemService : ISystemService
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    static extern bool PathFindOnPath([In] [Out] StringBuilder pszFile, [In] string[]? ppszOtherDirs);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    static extern IntPtr ILCreateFromPathW(string pszPath);

    [DllImport("shell32.dll", SetLastError = false)]
    static extern void ILFree(IntPtr pidl);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, IntPtr apidl, int dwFlags);

    void OpenFolderAndSelectFile(string folderPath)
    {
        var pidl = ILCreateFromPathW(folderPath);

        try
        {
            SHOpenFolderAndSelectItems(pidl, 0, 0, 0);
        }
        finally
        {
            ILFree(pidl);
        }
    }

    public string GitInstallPath { get; set; }
    public string VSCodeExecutableFile { get; }

    public WindowSystemService()
    {
        GitInstallPath = FindGitExecutable();
        VSCodeExecutableFile = FindVSCode();
    }

    public string FindGitExecutable()
    {
        var reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        var git = reg.OpenSubKey("SOFTWARE\\GitForWindows");
        if (git?.GetValue("InstallPath") is string installPath)
            return Path.Combine(installPath, "bin", "git.exe");

        var builder = new StringBuilder("git.exe", 259);
        if (!PathFindOnPath(builder, null))
            return string.Empty;

        var exePath = builder.ToString();
        if (string.IsNullOrEmpty(exePath)) return string.Empty;

        return exePath;
    }

    public string FindVSCode()
    {
        var root = RegistryKey.OpenBaseKey(
            RegistryHive.LocalMachine,
            Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);

        var vscode = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{EA457B21-F73E-494C-ACAB-524FDE069978}_is1");
        if (vscode != null && vscode.GetValue("DisplayIcon") is string displayIcon)
            return displayIcon;

        var toolPath = Environment.ExpandEnvironmentVariables($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe");
        if (File.Exists(toolPath)) return toolPath;
        return string.Empty;
    }

    public void OpenBrowser(string url)
    {
        var info = new ProcessStartInfo("cmd", $"/c start {url}");
        info.CreateNoWindow = true;
        Process.Start(info);
    }

    public void OpenTerminal(string workdir)
    {
        var bash = Path.Combine(Path.GetDirectoryName(GitInstallPath)!, "bash.exe");
        if (!File.Exists(bash))
        {
            App123.RaiseException(string.IsNullOrEmpty(workdir) ? "" : workdir, $"Can NOT found bash.exe under '{Path.GetDirectoryName(GitInstallPath)}'");
            return;
        }

        var startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = true;
        startInfo.FileName = bash;
        if (!string.IsNullOrEmpty(workdir) && Path.Exists(workdir)) startInfo.WorkingDirectory = workdir;
        Process.Start(startInfo);
    }

    public void OpenInFileManager(string path, bool select)
    {
        var fullpath = string.Empty;
        if (File.Exists(path))
        {
            fullpath = new FileInfo(path).FullName;

            // For security reason, we never execute a file.
            // Instead, we open the folder and select it.
            select = true;
        }
        else
        {
            fullpath = new DirectoryInfo(path).FullName;
        }

        if (select)
            // The fullpath here may be a file or a folder.
            OpenFolderAndSelectFile(fullpath);
        else
            // The fullpath here is always a folder.
            Process.Start(new ProcessStartInfo(fullpath)
            {
                UseShellExecute = true, CreateNoWindow = true
            });
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

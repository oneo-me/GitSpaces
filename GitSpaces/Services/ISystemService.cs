using System.Diagnostics.CodeAnalysis;

namespace GitSpaces.Services;

public interface ISystemService
{
    string GitInstallPath { get; set; }
    string FindGitExecutable();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    string FindVSCode();

    void OpenBrowser(string url);
    void OpenTerminal(string workdir);
    void OpenInFileManager(string path, bool select = false);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    void OpenInVSCode(string fullpath);
}

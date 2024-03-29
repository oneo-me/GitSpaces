using System.Diagnostics;
using Avalonia.Threading;
using GitSpaces.Services;
using OpenUI.Services;

namespace GitSpaces.Commands;

public static class SaveRevisionFile
{
    public static void Run(string repo, string revision, string file, string saveTo)
    {
        var isLFSFiltered = new IsLFSFiltered(repo, file).Result();
        if (isLFSFiltered)
        {
            var tmpFile = saveTo + ".tmp";
            if (ExecCmd(repo, $"show {revision}:\"{file}\"", tmpFile))
            {
                ExecCmd(repo, "lfs smudge", saveTo, tmpFile);
            }

            File.Delete(tmpFile);
        }
        else
        {
            ExecCmd(repo, $"show {revision}:\"{file}\"", saveTo);
        }
    }

    static bool ExecCmd(string repo, string args, string outputFile, string inputFile = null)
    {
        var OS = Service.Get<ISystemService>();
        var starter = new ProcessStartInfo();
        starter.WorkingDirectory = repo;
        starter.FileName = OS.GitInstallPath;
        starter.Arguments = args;
        starter.UseShellExecute = false;
        starter.CreateNoWindow = true;
        starter.WindowStyle = ProcessWindowStyle.Hidden;
        starter.RedirectStandardInput = true;
        starter.RedirectStandardOutput = true;
        starter.RedirectStandardError = true;

        using (var sw = File.OpenWrite(outputFile))
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = starter
                };
                proc.Start();

                if (inputFile != null)
                {
                    using (var sr = new StreamReader(inputFile))
                    {
                        while (true)
                        {
                            var line = sr.ReadLine();
                            if (line == null) break;
                            proc.StandardInput.WriteLine(line);
                        }
                    }
                }

                proc.StandardOutput.BaseStream.CopyTo(sw);
                proc.WaitForExit();
                var rs = proc.ExitCode == 0;
                proc.Close();

                return rs;
            }
            catch (Exception e)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    App123.RaiseException(repo, "Save file failed: " + e.Message);
                });
                return false;
            }
        }
    }
}

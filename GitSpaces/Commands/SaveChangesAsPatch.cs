using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Threading;
using GitSpaces.Models;
using GitSpaces.Native;

namespace GitSpaces.Commands;

public static class SaveChangesAsPatch
{
    public static bool Exec(string repo, List<Change> changes, bool isUnstaged, string saveTo)
    {
        using (var sw = File.Create(saveTo))
        {
            foreach (var change in changes)
            {
                if (!ProcessSingleChange(repo, new(change, isUnstaged), sw)) return false;
            }
        }

        return true;
    }

    static bool ProcessSingleChange(string repo, DiffOption opt, FileStream writer)
    {
        var starter = new ProcessStartInfo();
        starter.WorkingDirectory = repo;
        starter.FileName = OS.GitInstallPath;
        starter.Arguments = $"diff --ignore-cr-at-eol --unified=4 {opt}";
        starter.UseShellExecute = false;
        starter.CreateNoWindow = true;
        starter.WindowStyle = ProcessWindowStyle.Hidden;
        starter.RedirectStandardOutput = true;

        try
        {
            var proc = new Process
            {
                StartInfo = starter
            };
            proc.Start();
            proc.StandardOutput.BaseStream.CopyTo(writer);
            proc.WaitForExit();
            var rs = proc.ExitCode == 0;
            proc.Close();

            return rs;
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                App123.RaiseException(repo, "Save change to patch failed: " + e.Message);
            });
            return false;
        }
    }
}

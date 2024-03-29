using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;
using GitSpaces.Native;

namespace GitSpaces.Commands;

public static class GetImageFileAsBitmap
{
    public static Bitmap Run(string repo, string revision, string file)
    {
        var starter = new ProcessStartInfo();
        starter.WorkingDirectory = repo;
        starter.FileName = OS.GitInstallPath;
        starter.Arguments = $"show {revision}:\"{file}\"";
        starter.UseShellExecute = false;
        starter.CreateNoWindow = true;
        starter.WindowStyle = ProcessWindowStyle.Hidden;
        starter.RedirectStandardOutput = true;

        try
        {
            var stream = new MemoryStream();
            var proc = new Process
            {
                StartInfo = starter
            };
            proc.Start();
            proc.StandardOutput.BaseStream.CopyTo(stream);
            proc.WaitForExit();
            proc.Close();

            stream.Position = 0;
            return new(stream);
        }
        catch
        {
            return null;
        }
    }
}

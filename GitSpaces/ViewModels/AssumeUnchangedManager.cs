using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using GitSpaces.Commands;

namespace GitSpaces.ViewModels;

public class AssumeUnchangedManager
{
    public AvaloniaList<string> Files { get; }

    public AssumeUnchangedManager(string repo)
    {
        _repo = repo;
        Files = new();

        Task.Run(() =>
        {
            var collect = new AssumeUnchanged(_repo).View();
            Dispatcher.UIThread.Invoke(() =>
            {
                Files.AddRange(collect);
            });
        });
    }

    public void Remove(object param)
    {
        if (param is string file)
        {
            new AssumeUnchanged(_repo).Remove(file);
            Files.Remove(file);
        }
    }

    readonly string _repo;
}

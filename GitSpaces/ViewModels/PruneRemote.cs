using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class PruneRemote : Popup
{
    public Remote Remote { get; }

    public PruneRemote(Repository repo, Remote remote)
    {
        _repo = repo;
        Remote = remote;
        View = new OldViews.PruneRemote
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Run `prune` on remote ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Remote(_repo.FullPath).Prune(Remote.Name);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class DeleteRemote : Popup
{
    public Remote Remote { get; }

    public DeleteRemote(Repository repo, Remote remote)
    {
        _repo = repo;
        Remote = remote;
        View = new OldViews.DeleteRemote
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Deleting remote ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Remote(_repo.FullPath).Delete(Remote.Name);
            CallUIThread(() =>
            {
                _repo.MarkBranchesDirtyManually();
                _repo.SetWatcherEnabled(true);
            });
            return succ;
        });
    }

    readonly Repository _repo;
}

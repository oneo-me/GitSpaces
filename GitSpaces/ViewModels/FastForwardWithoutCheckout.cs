using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class FastForwardWithoutCheckout : Popup
{
    public Branch Local { get; }

    public Branch To { get; }

    public FastForwardWithoutCheckout(Repository repo, Branch local, Branch upstream)
    {
        _repo = repo;
        Local = local;
        To = upstream;
        View = new Views.FastForwardWithoutCheckout
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Fast-Forward ...";

        return Task.Run(() =>
        {
            new Commands.Fetch(_repo.FullPath, To.Remote, Local.Name, To.Name, SetProgressDescription).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
}

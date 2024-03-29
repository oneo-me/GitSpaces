using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Rebase : Popup
{
    public Branch Current { get; private set; }

    public object On { get; private set; }

    public bool AutoStash { get; set; }

    public Rebase(Repository repo, Branch current, Branch on)
    {
        _repo = repo;
        _revision = on.Head;
        Current = current;
        On = on;
        AutoStash = true;
        View = new Views.Rebase
        {
            DataContext = this
        };
    }

    public Rebase(Repository repo, Branch current, Commit on)
    {
        _repo = repo;
        _revision = on.SHA;
        Current = current;
        On = on;
        AutoStash = true;
        View = new Views.Rebase
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Rebasing ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Rebase(_repo.FullPath, _revision, AutoStash).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    readonly string _revision = string.Empty;
}

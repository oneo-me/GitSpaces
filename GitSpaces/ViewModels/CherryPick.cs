using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class CherryPick : Popup
{
    public Commit Target { get; }

    public bool AutoCommit { get; set; }

    public CherryPick(Repository repo, Commit target)
    {
        _repo = repo;
        Target = target;
        AutoCommit = true;
        View = new OldViews.CherryPick
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Cherry-Pick commit '{Target.SHA}' ...";

        return Task.Run(() =>
        {
            var succ = new Commands.CherryPick(_repo.FullPath, Target.SHA, !AutoCommit).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

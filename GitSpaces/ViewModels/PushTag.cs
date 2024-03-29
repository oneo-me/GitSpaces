using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class PushTag : Popup
{
    public Tag Target { get; }

    public List<Remote> Remotes => _repo.Remotes;

    public Remote SelectedRemote { get; set; }

    public PushTag(Repository repo, Tag target)
    {
        _repo = repo;
        Target = target;
        SelectedRemote = _repo.Remotes[0];
        View = new OldViews.PushTag
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Pushing tag '{Target.Name}' to remote '{SelectedRemote.Name}' ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Push(_repo.FullPath, SelectedRemote.Name, Target.Name, false).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

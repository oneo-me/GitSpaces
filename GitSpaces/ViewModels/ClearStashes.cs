using GitSpaces.Commands;

namespace GitSpaces.ViewModels;

public class ClearStashes : Popup
{
    public ClearStashes(Repository repo)
    {
        _repo = repo;
        View = new OldViews.ClearStashes
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Clear all stashes...";

        return Task.Run(() =>
        {
            new Stash(_repo.FullPath).Clear();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
}

using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class DeleteTag : Popup
{
    public Tag Target { get; }

    public bool ShouldPushToRemote { get; set; }

    public DeleteTag(Repository repo, Tag tag)
    {
        _repo = repo;
        Target = tag;
        ShouldPushToRemote = true;
        View = new OldViews.DeleteTag
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Deleting tag '{Target.Name}' ...";

        return Task.Run(() =>
        {
            var remotes = ShouldPushToRemote ? _repo.Remotes : null;
            var succ = Commands.Tag.Delete(_repo.FullPath, Target.Name, remotes);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

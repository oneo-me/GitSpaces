using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class DeleteBranch : Popup
{
    public Branch Target { get; }

    public DeleteBranch(Repository repo, Branch branch)
    {
        _repo = repo;
        Target = branch;
        View = new OldViews.DeleteBranch
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Deleting branch...";

        return Task.Run(() =>
        {
            if (Target.IsLocal)
            {
                Commands.Branch.Delete(_repo.FullPath, Target.Name);
            }
            else
            {
                new Commands.Push(_repo.FullPath, Target.Remote, Target.Name).Exec();
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
}

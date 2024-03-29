using System.Threading.Tasks;
using GitSpaces.Commands;

namespace GitSpaces.ViewModels;

public class DeleteSubmodule : Popup
{
    public string Submodule { get; }

    public DeleteSubmodule(Repository repo, string submodule)
    {
        _repo = repo;
        Submodule = submodule;
        View = new OldViews.DeleteSubmodule
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Deleting submodule ...";

        return Task.Run(() =>
        {
            var succ = new Submodule(_repo.FullPath).Delete(Submodule);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

using System.Threading.Tasks;

namespace GitSpaces.ViewModels;

public class Checkout : Popup
{
    public string Branch { get; }

    public Checkout(Repository repo, string branch)
    {
        _repo = repo;
        Branch = branch;
        View = new Views.Checkout
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Checkout '{Branch}' ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Checkout(_repo.FullPath).Branch(Branch, SetProgressDescription);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

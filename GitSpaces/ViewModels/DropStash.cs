using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class DropStash : Popup
{
    public Stash Stash { get; }

    public DropStash(string repo, Stash stash)
    {
        _repo = repo;
        Stash = stash;
        View = new Views.DropStash
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        ProgressDescription = $"Dropping stash: {Stash.Name}";

        return Task.Run(() =>
        {
            new Commands.Stash(_repo).Drop(Stash.Name);
            return true;
        });
    }

    readonly string _repo;
}

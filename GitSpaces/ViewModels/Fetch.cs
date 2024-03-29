using System.Collections.Generic;
using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Fetch : Popup
{
    public List<Remote> Remotes => _repo.Remotes;

    public bool FetchAllRemotes
    {
        get => _fetchAllRemotes;
        set => SetProperty(ref _fetchAllRemotes, value);
    }

    public Remote SelectedRemote { get; set; }

    public bool Prune { get; set; }

    public Fetch(Repository repo, Remote preferedRemote = null)
    {
        _repo = repo;
        _fetchAllRemotes = preferedRemote == null;
        SelectedRemote = preferedRemote != null ? preferedRemote : _repo.Remotes[0];
        Prune = true;
        View = new Views.Fetch
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        return Task.Run(() =>
        {
            if (FetchAllRemotes)
            {
                foreach (var remote in _repo.Remotes)
                {
                    SetProgressDescription($"Fetching remote: {remote.Name}");
                    new Commands.Fetch(_repo.FullPath, remote.Name, Prune, SetProgressDescription).Exec();
                }
            }
            else
            {
                SetProgressDescription($"Fetching remote: {SelectedRemote.Name}");
                new Commands.Fetch(_repo.FullPath, SelectedRemote.Name, Prune, SetProgressDescription).Exec();
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
    bool _fetchAllRemotes = true;
}

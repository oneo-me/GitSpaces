using System.Collections.Generic;
using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class DiscardModeAll
{
}

public class DiscardModeSingle
{
    public string File { get; set; }
}

public class DiscardModeMulti
{
    public int Count { get; set; }
}

public class Discard : Popup
{
    public object Mode { get; private set; }

    public Discard(Repository repo)
    {
        _repo = repo;

        Mode = new DiscardModeAll();
        View = new Views.Discard
        {
            DataContext = this
        };
    }

    public Discard(Repository repo, List<Change> changes, bool isUnstaged)
    {
        _repo = repo;
        _changes = changes;
        _isUnstaged = isUnstaged;

        if (_changes == null)
        {
            Mode = new DiscardModeAll();
        }
        else if (_changes.Count == 1)
        {
            Mode = new DiscardModeSingle
            {
                File = _changes[0].Path
            };
        }
        else
        {
            Mode = new DiscardModeMulti
            {
                Count = _changes.Count
            };
        }

        View = new Views.Discard
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = _changes == null ? "Discard all local changes ..." : $"Discard total {_changes.Count} changes ...";

        return Task.Run(() =>
        {
            if (_changes == null)
            {
                Commands.Discard.All(_repo.FullPath);
            }
            else if (_isUnstaged)
            {
                Commands.Discard.ChangesInWorkTree(_repo.FullPath, _changes);
            }
            else
            {
                Commands.Discard.ChangesInStaged(_repo.FullPath, _changes);
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
    readonly List<Change> _changes;
    readonly bool _isUnstaged = true;
}

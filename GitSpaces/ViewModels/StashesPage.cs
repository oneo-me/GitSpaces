using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using GitSpaces.Models;
using Stash = GitSpaces.Models.Stash;

namespace GitSpaces.ViewModels;

public class StashesPage : ObservableObject
{
    public int Count => _stashes == null ? 0 : _stashes.Count;

    public List<Stash> Stashes
    {
        get => _stashes;
        set
        {
            if (SetProperty(ref _stashes, value))
            {
                SelectedStash = null;
            }
        }
    }

    public Stash SelectedStash
    {
        get => _selectedStash;
        set
        {
            if (SetProperty(ref _selectedStash, value))
            {
                if (value == null)
                {
                    Changes = null;
                }
                else
                {
                    Task.Run(() =>
                    {
                        var changes = new QueryStashChanges(_repo.FullPath, value.SHA).Result();
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            Changes = changes;
                        });
                    });
                }
            }
        }
    }

    public List<Change> Changes
    {
        get => _changes;
        private set
        {
            if (SetProperty(ref _changes, value))
            {
                SelectedChange = null;
            }
        }
    }

    public Change SelectedChange
    {
        get => _selectedChange;
        set
        {
            if (SetProperty(ref _selectedChange, value))
            {
                if (value == null)
                {
                    DiffContext = null;
                }
                else
                {
                    DiffContext = new(_repo.FullPath, new($"{_selectedStash.SHA}^", _selectedStash.SHA, value), _diffContext);
                }
            }
        }
    }

    public DiffContext DiffContext
    {
        get => _diffContext;
        private set => SetProperty(ref _diffContext, value);
    }

    public StashesPage(Repository repo)
    {
        _repo = repo;
    }

    public void Cleanup()
    {
        _repo = null;
        if (_stashes != null) _stashes.Clear();
        _selectedStash = null;
        if (_changes != null) _changes.Clear();
        _selectedChange = null;
        _diffContext = null;
    }

    public void Apply(object param)
    {
        if (param is Stash stash)
        {
            Task.Run(() =>
            {
                new Commands.Stash(_repo.FullPath).Apply(stash.Name);
            });
        }
    }

    public void Pop(object param)
    {
        if (param is Stash stash)
        {
            Task.Run(() =>
            {
                new Commands.Stash(_repo.FullPath).Pop(stash.Name);
            });
        }
    }

    public void Drop(object param)
    {
        if (param is Stash stash && PopupHost.CanCreatePopup())
        {
            PopupHost.ShowPopup(new DropStash(_repo.FullPath, stash));
        }
    }

    public void Clear()
    {
        if (PopupHost.CanCreatePopup())
        {
            PopupHost.ShowPopup(new ClearStashes(_repo));
        }
    }

    Repository _repo;
    List<Stash> _stashes;
    Stash _selectedStash;
    List<Change> _changes;
    Change _selectedChange;
    DiffContext _diffContext;
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitSpaces.Commands;
using Branch = GitSpaces.Models.Branch;
using Remote = GitSpaces.Models.Remote;

namespace GitSpaces.ViewModels;

public class Pull : Popup
{
    public List<Remote> Remotes => _repo.Remotes;
    public Branch Current { get; }

    public bool HasSpecifiedRemoteBranch { get; private set; }

    public Remote SelectedRemote
    {
        get => _selectedRemote;
        set
        {
            if (SetProperty(ref _selectedRemote, value))
            {
                var branches = new List<Branch>();
                foreach (var branch in _repo.Branches)
                {
                    if (branch.Remote == value.Name) branches.Add(branch);
                }

                RemoteBranches = branches;
                SelectedBranch = branches.Count > 0 ? branches[0] : null;
            }
        }
    }

    public List<Branch> RemoteBranches
    {
        get => _remoteBranches;
        private set => SetProperty(ref _remoteBranches, value);
    }

    [Required(ErrorMessage = "Remote branch to pull is required!!!")]
    public Branch SelectedBranch
    {
        get => _selectedBranch;
        set => SetProperty(ref _selectedBranch, value);
    }

    public bool UseRebase { get; set; } = true;

    public bool AutoStash { get; set; } = true;

    public Pull(Repository repo, Branch specifiedRemoteBranch)
    {
        _repo = repo;
        Current = repo.Branches.Find(x => x.IsCurrent);

        if (specifiedRemoteBranch != null)
        {
            _selectedRemote = repo.Remotes.Find(x => x.Name == specifiedRemoteBranch.Remote);
            _selectedBranch = specifiedRemoteBranch;
            HasSpecifiedRemoteBranch = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(Current.Upstream))
            {
                foreach (var branch in repo.Branches)
                {
                    if (!branch.IsLocal && Current.Upstream == branch.FullName)
                    {
                        _selectedRemote = repo.Remotes.Find(x => x.Name == branch.Remote);
                        _selectedBranch = branch;
                        break;
                    }
                }
            }

            HasSpecifiedRemoteBranch = false;
        }

        // Make sure remote is exists.
        if (_selectedRemote == null)
        {
            _selectedRemote = repo.Remotes[0];
            _selectedBranch = null;
            HasSpecifiedRemoteBranch = false;
        }

        _remoteBranches = new();
        foreach (var branch in _repo.Branches)
        {
            if (branch.Remote == _selectedRemote.Name) _remoteBranches.Add(branch);
        }

        if (_selectedBranch == null && _remoteBranches.Count > 0)
        {
            _selectedBranch = _remoteBranches[0];
        }

        View = new OldViews.Pull
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        return Task.Run(() =>
        {
            var needPopStash = false;
            if (AutoStash && _repo.WorkingCopyChangesCount > 0)
            {
                SetProgressDescription("Adding untracked changes...");
                var succ = new Add(_repo.FullPath).Exec();
                if (succ)
                {
                    SetProgressDescription("Stash local changes...");
                    succ = new Stash(_repo.FullPath).Push("PULL_AUTO_STASH");
                }

                if (!succ)
                {
                    CallUIThread(() => _repo.SetWatcherEnabled(true));
                    return false;
                }

                needPopStash = true;
            }

            SetProgressDescription($"Pull {_selectedRemote.Name}/{_selectedBranch.Name}...");
            var rs = new Commands.Pull(_repo.FullPath, _selectedRemote.Name, _selectedBranch.Name, UseRebase, SetProgressDescription).Exec();
            if (rs && needPopStash)
            {
                SetProgressDescription("Re-apply local changes...");
                rs = new Stash(_repo.FullPath).Pop("stash@{0}");
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return rs;
        });
    }

    readonly Repository _repo;
    Remote _selectedRemote;
    List<Branch> _remoteBranches;
    Branch _selectedBranch;
}

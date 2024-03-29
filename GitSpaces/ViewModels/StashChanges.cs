using GitSpaces.Models;
using Stash = GitSpaces.Commands.Stash;

namespace GitSpaces.ViewModels;

public class StashChanges : Popup
{
    public string Message { get; set; }

    public bool CanIgnoreUntracked { get; }

    public bool IncludeUntracked { get; set; }

    public StashChanges(Repository repo, List<Change> changes, bool canIgnoreUntracked)
    {
        _repo = repo;
        _changes = changes;

        CanIgnoreUntracked = canIgnoreUntracked;
        IncludeUntracked = true;
        View = new OldViews.StashChanges
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        var jobs = _changes;
        if (CanIgnoreUntracked && !IncludeUntracked)
        {
            jobs = new();
            foreach (var job in _changes)
            {
                if (job.WorkTree != ChangeState.Untracked && job.WorkTree != ChangeState.Added)
                {
                    jobs.Add(job);
                }
            }
        }

        if (jobs.Count == 0) return null;

        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Stash changes ...";

        return Task.Run(() =>
        {
            new Stash(_repo.FullPath).Push(jobs, Message);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
    readonly List<Change> _changes;
}

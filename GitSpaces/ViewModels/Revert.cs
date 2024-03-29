﻿using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Revert : Popup
{
    public Commit Target { get; }

    public bool AutoCommit { get; set; }

    public Revert(Repository repo, Commit target)
    {
        _repo = repo;
        Target = target;
        AutoCommit = true;
        View = new OldViews.Revert
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Revert commit '{Target.SHA}' ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Revert(_repo.FullPath, Target.SHA, AutoCommit).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

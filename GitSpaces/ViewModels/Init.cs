﻿using GitSpaces.Configs;

namespace GitSpaces.ViewModels;

public class Init : Popup
{
    public string TargetPath
    {
        get => _targetPath;
        set => SetProperty(ref _targetPath, value);
    }

    public Init(string path)
    {
        TargetPath = path;
        View = new OldViews.Init
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        ProgressDescription = $"Initialize git repository at: '{_targetPath}'";

        return Task.Run(() =>
        {
            var succ = new Commands.Init(HostPageId, _targetPath).Exec();
            if (!succ) return false;

            var gitDir = Path.GetFullPath(Path.Combine(_targetPath, ".git"));

            CallUIThread(() =>
            {
                var repo = Preference.AddRepository(_targetPath, gitDir);
                var node = new RepositoryNode
                {
                    Id = repo.FullPath, Name = Path.GetFileName(repo.FullPath), Bookmark = 0, IsRepository = true
                };
                Preference.AddNode(node);
            });

            return true;
        });
    }

    string _targetPath;
}

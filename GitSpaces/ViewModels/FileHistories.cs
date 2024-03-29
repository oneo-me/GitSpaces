using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using Commit = GitSpaces.Models.Commit;

namespace GitSpaces.ViewModels;

public class FileHistories : ObservableObject
{
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public List<Commit> Commits
    {
        get => _commits;
        set => SetProperty(ref _commits, value);
    }

    public Commit SelectedCommit
    {
        get => _selectedCommit;
        set
        {
            if (SetProperty(ref _selectedCommit, value))
            {
                if (value == null)
                {
                    DiffContext = null;
                    DetailContext.Commit = null;
                }
                else
                {
                    DiffContext = new(_repo, new(value, _file), _diffContext);
                    DetailContext.Commit = value;
                }
            }
        }
    }

    public DiffContext DiffContext
    {
        get => _diffContext;
        set => SetProperty(ref _diffContext, value);
    }

    public CommitDetail DetailContext
    {
        get => _detailContext;
        set => SetProperty(ref _detailContext, value);
    }

    public FileHistories(string repo, string file)
    {
        _repo = repo;
        _file = file;
        _detailContext = new(repo);

        Task.Run(() =>
        {
            var commits = new QueryCommits(_repo, $"-n 10000 -- \"{file}\"").Result();
            Dispatcher.UIThread.Invoke(() =>
            {
                IsLoading = false;
                Commits = commits;
                if (commits.Count > 0) SelectedCommit = commits[0];
            });
        });
    }

    readonly string _repo = string.Empty;
    readonly string _file = string.Empty;
    bool _isLoading = true;
    List<Commit> _commits;
    Commit _selectedCommit;
    DiffContext _diffContext;
    CommitDetail _detailContext;
}

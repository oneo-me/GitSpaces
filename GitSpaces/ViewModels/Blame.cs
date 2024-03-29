using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Blame : ObservableObject
{
    public string Title { get; private set; }

    public string SelectedSHA
    {
        get => _selectedSHA;
        private set => SetProperty(ref _selectedSHA, value);
    }

    public bool IsBinary => _data != null && _data.IsBinary;

    public BlameData Data
    {
        get => _data;
        private set => SetProperty(ref _data, value);
    }

    public Blame(string repo, string file, string revision)
    {
        _repo = repo;

        Title = $"{file} @ {revision.Substring(0, 10)}";
        Task.Run(() =>
        {
            var result = new Commands.Blame(repo, file, revision).Result();
            Dispatcher.UIThread.Invoke(() =>
            {
                Data = result;
                OnPropertyChanged(nameof(IsBinary));
            });
        });
    }

    public void NavigateToCommit(string commitSHA)
    {
        var repo = Preference.FindRepository(_repo);
        if (repo != null) repo.NavigateToCommit(commitSHA);
    }

    readonly string _repo = string.Empty;
    string _selectedSHA = string.Empty;
    BlameData _data;
}

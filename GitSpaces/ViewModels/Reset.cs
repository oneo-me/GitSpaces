using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class ResetMode
{
    public string Name { get; set; }
    public string Desc { get; set; }
    public string Arg { get; set; }
    public IBrush Color { get; set; }

    public ResetMode(string n, string d, string a, IBrush b)
    {
        Name = n;
        Desc = d;
        Arg = a;
        Color = b;
    }
}

public class Reset : Popup
{
    public Branch Current { get; private set; }

    public Commit To { get; }

    public List<ResetMode> Modes { get; }

    public ResetMode SelectedMode { get; set; }

    public Reset(Repository repo, Branch current, Commit to)
    {
        _repo = repo;
        Current = current;
        To = to;
        Modes = new()
        {
            new("Soft", "Keep all changes. Stage differences", "--soft", Brushes.Green), new("Mixed", "Keep all changes. Unstage differences", "--mixed", Brushes.Orange), new("Hard", "Discard all changes", "--hard", Brushes.Red)
        };
        SelectedMode = Modes[0];
        View = new OldViews.Reset
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Reset current branch to {To.SHA} ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Reset(_repo.FullPath, To.SHA, SelectedMode.Arg).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
}

using System.ComponentModel.DataAnnotations;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Reword : Popup
{
    public Commit Head { get; }

    [Required(ErrorMessage = "Commit message is required!!!")]
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value, true);
    }

    public Reword(Repository repo, Commit head)
    {
        _repo = repo;
        Head = head;
        Message = head.FullMessage;
        View = new OldViews.Reword
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        if (_message == Head.FullMessage) return null;

        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Editing head commit message ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Commit(_repo.FullPath, _message, true, true).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    string _message = string.Empty;
}

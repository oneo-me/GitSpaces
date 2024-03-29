using Avalonia.Collections;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class LauncherPage : PopupHost
{
    public RepositoryNode Node
    {
        get => _node;
        set => SetProperty(ref _node, value);
    }

    public object Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    public AvaloniaList<Notification> Notifications { get; set; } = new();

    public LauncherPage()
    {
        _node = new()
        {
            Id = Guid.NewGuid().ToString(), Name = "WelcomePage", Bookmark = 0, IsRepository = false
        };
        _data = new Welcome();
    }

    public LauncherPage(RepositoryNode node, Repository repo)
    {
        _node = node;
        _data = repo;
    }

    public override string GetId()
    {
        return _node.Id;
    }

    public void CopyPath()
    {
        if (_node.IsRepository) App123.CopyText(_node.Id);
    }

    public void DismissNotification(object param)
    {
        if (param is Notification notice)
        {
            Notifications.Remove(notice);
        }
    }

    RepositoryNode _node;
    object _data;
}

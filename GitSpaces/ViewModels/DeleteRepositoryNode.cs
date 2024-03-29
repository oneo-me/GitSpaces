using System.Threading.Tasks;
using GitSpaces.Configs;

namespace GitSpaces.ViewModels;

public class DeleteRepositoryNode : Popup
{
    public RepositoryNode Node
    {
        get => _node;
        set => SetProperty(ref _node, value);
    }

    public DeleteRepositoryNode(RepositoryNode node)
    {
        _node = node;
        View = new OldViews.DeleteRepositoryNode
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        Preference.RemoveNode(_node);
        return null;
    }

    RepositoryNode _node;
}

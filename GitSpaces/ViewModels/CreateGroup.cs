using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GitSpaces.ViewModels;

public class CreateGroup : Popup
{
    [Required(ErrorMessage = "Group name is required!")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    public CreateGroup(RepositoryNode parent)
    {
        _parent = parent;
        View = new Views.CreateGroup
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        Preference.AddNode(new()
        {
            Id = Guid.NewGuid().ToString(), Name = _name, IsRepository = false, IsExpanded = false
        }, _parent);

        return null;
    }

    readonly RepositoryNode _parent;
    string _name = string.Empty;
}

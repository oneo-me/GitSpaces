﻿using System.ComponentModel.DataAnnotations;
using GitSpaces.Configs;

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
        View = new OldViews.CreateGroup
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

﻿using System.Text.Json.Serialization;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Services;
using OpenUI.Services;

namespace GitSpaces.ViewModels;

public class RepositoryNode : ObservableObject
{
    public string Id
    {
        get => _id;
        set
        {
            var normalized = value.Replace('\\', '/');
            SetProperty(ref _id, normalized);
        }
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Bookmark
    {
        get => _bookmark;
        set => SetProperty(ref _bookmark, value);
    }

    public bool IsRepository
    {
        get => _isRepository;
        set => SetProperty(ref _isRepository, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    [JsonIgnore]
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public AvaloniaList<RepositoryNode> SubNodes
    {
        get => _subNodes;
        set => SetProperty(ref _subNodes, value);
    }

    public void Edit()
    {
        if (PopupHost.CanCreatePopup()) PopupHost.ShowPopup(new EditRepositoryNode(this));
    }

    public void AddSubFolder()
    {
        if (PopupHost.CanCreatePopup()) PopupHost.ShowPopup(new CreateGroup(this));
    }

    public void OpenInFileManager()
    {
        if (!IsRepository) return;
        var OS = Service.Get<ISystemService>();
        OS.OpenInFileManager(_id);
    }

    public void OpenTerminal()
    {
        if (!IsRepository) return;
        var OS = Service.Get<ISystemService>();
        OS.OpenTerminal(_id);
    }

    public void Delete()
    {
        if (PopupHost.CanCreatePopup()) PopupHost.ShowPopup(new DeleteRepositoryNode(this));
    }

    string _id = string.Empty;
    string _name = string.Empty;
    bool _isRepository;
    int _bookmark;
    bool _isExpanded;
    bool _isVisible = true;
    AvaloniaList<RepositoryNode> _subNodes = new();
}

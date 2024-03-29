using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Configs;
using GitSpaces.Services;
using OpenUI.Services;

namespace GitSpaces.ViewModels;

public class Welcome : ObservableObject
{
    public bool IsClearSearchVisible => !string.IsNullOrEmpty(_searchFilter);

    public AvaloniaList<RepositoryNode> RepositoryNodes => Preference.Instance.RepositoryNodes;

    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            if (SetProperty(ref _searchFilter, value))
            {
                Referesh();
                OnPropertyChanged(nameof(IsClearSearchVisible));
            }
        }
    }

    public void InitRepository(string path)
    {
        if (!Preference.Instance.IsGitConfigured)
        {
            App123.RaiseException(PopupHost.Active.GetId(), App123.Text("NotConfigured"));
            return;
        }

        if (PopupHost.CanCreatePopup())
        {
            PopupHost.ShowPopup(new Init(path));
        }
    }

    public void Clone(object param)
    {
        var launcher = param as Launcher;
        var page = launcher.ActivePage;

        if (!Preference.Instance.IsGitConfigured)
        {
            App123.RaiseException(page.GetId(), App123.Text("NotConfigured"));
            return;
        }

        if (PopupHost.CanCreatePopup())
        {
            PopupHost.ShowPopup(new Clone(launcher, page));
        }
    }

    public void OpenTerminal()
    {
        if (!Preference.Instance.IsGitConfigured)
        {
            App123.RaiseException(PopupHost.Active.GetId(), App123.Text("NotConfigured"));
        }
        else
        {
            var OS = Service.Get<ISystemService>();
            OS.OpenTerminal(null);
        }
    }

    public void ClearSearchFilter()
    {
        SearchFilter = string.Empty;
    }

    public void AddFolder()
    {
        if (PopupHost.CanCreatePopup()) PopupHost.ShowPopup(new CreateGroup(null));
    }

    public void MoveNode(RepositoryNode from, RepositoryNode to)
    {
        Preference.MoveNode(from, to);
    }

    void Referesh()
    {
        if (string.IsNullOrWhiteSpace(_searchFilter))
        {
            foreach (var node in RepositoryNodes) ResetVisibility(node);
        }
        else
        {
            foreach (var node in RepositoryNodes) SetVisibilityBySearch(node);
        }
    }

    void ResetVisibility(RepositoryNode node)
    {
        node.IsVisible = true;
        foreach (var subNode in node.SubNodes) ResetVisibility(subNode);
    }

    void SetVisibilityBySearch(RepositoryNode node)
    {
        if (!node.IsRepository)
        {
            if (node.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
            {
                node.IsVisible = true;
                foreach (var subNode in node.SubNodes) ResetVisibility(subNode);
            }
            else
            {
                var hasVisibleSubNode = false;
                foreach (var subNode in node.SubNodes)
                {
                    SetVisibilityBySearch(subNode);
                    hasVisibleSubNode |= subNode.IsVisible;
                }

                node.IsVisible = hasVisibleSubNode;
            }
        }
        else
        {
            node.IsVisible = node.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase);
        }
    }

    string _searchFilter = string.Empty;
}

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitSpaces.Commands;
using GitSpaces.Models;
using GitSpaces.ViewModels;
using Branch = GitSpaces.Models.Branch;
using Commit = GitSpaces.Models.Commit;
using GC = System.GC;
using Remote = GitSpaces.Models.Remote;
using Tag = GitSpaces.Models.Tag;

namespace GitSpaces.Views;

public class RepositorySubView : Border
{
    public static readonly StyledProperty<object> DataProperty =
        AvaloniaProperty.Register<RepositorySubView, object>(nameof(Data), false);

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Border);

    static RepositorySubView()
    {
        DataProperty.Changed.AddClassHandler<RepositorySubView>((view, ev) =>
        {
            var data = view.Data;

            if (data == null)
            {
                view.Child = null;
            }
            else if (data is ViewModels.Histories)
            {
                view.Child = new Histories
                {
                    DataContext = data
                };
            }
            else if (data is ViewModels.WorkingCopy)
            {
                view.Child = new WorkingCopy
                {
                    DataContext = data
                };
            }
            else if (data is ViewModels.StashesPage)
            {
                view.Child = new StashesPage
                {
                    DataContext = data
                };
            }
            else
            {
                view.Child = null;
            }

            GC.Collect();
        });
    }
}

public partial class Repository : UserControl
{
    public Repository()
    {
        InitializeComponent();
    }

    void OnLocalBranchTreeLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TreeView tree) tree.UnselectAll();
    }

    void OnRemoteBranchTreeLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TreeView tree) tree.UnselectAll();
    }

    void OnTagDataGridLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is DataGrid datagrid) datagrid.SelectedItem = null;
    }

    void OnLocalBranchTreeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TreeView tree && tree.SelectedItem != null)
        {
            remoteBranchTree.UnselectAll();

            var node = tree.SelectedItem as BranchTreeNode;
            if (node.IsBranch && DataContext is ViewModels.Repository repo)
            {
                repo.NavigateToCommit((node.Backend as Branch).Head);
            }
        }
    }

    void OnRemoteBranchTreeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TreeView tree && tree.SelectedItem != null)
        {
            localBranchTree.UnselectAll();

            var node = tree.SelectedItem as BranchTreeNode;
            if (node.IsBranch && DataContext is ViewModels.Repository repo)
            {
                repo.NavigateToCommit((node.Backend as Branch).Head);
            }
        }
    }

    void OnTagDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.SelectedItem != null)
        {
            var tag = datagrid.SelectedItem as Tag;
            if (DataContext is ViewModels.Repository repo)
            {
                repo.NavigateToCommit(tag.SHA);
            }
        }
    }

    void OnSearchCommitPanelPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        var grid = sender as Grid;
        if (e.Property == IsVisibleProperty && grid.IsVisible)
        {
            txtSearchCommitsBox.Focus();
        }
    }

    void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ViewModels.Repository repo)
            {
                repo.StartSearchCommits();
            }

            e.Handled = true;
        }
    }

    void OnSearchResultDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.SelectedItem != null)
        {
            if (DataContext is ViewModels.Repository repo)
            {
                var commit = datagrid.SelectedItem as Commit;
                repo.NavigateToCommit(commit.SHA);
            }
        }

        e.Handled = true;
    }

    void OnToggleFilter(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            var filter = string.Empty;
            if (toggle.DataContext is BranchTreeNode node)
            {
                if (node.IsBranch)
                {
                    filter = (node.Backend as Branch).FullName;
                }
            }
            else if (toggle.DataContext is Tag tag)
            {
                filter = tag.Name;
            }

            if (!string.IsNullOrEmpty(filter) && DataContext is ViewModels.Repository repo)
            {
                repo.UpdateFilter(filter, toggle.IsChecked == true);
            }
        }

        e.Handled = true;
    }

    void OnLocalBranchContextMenuRequested(object sender, ContextRequestedEventArgs e)
    {
        remoteBranchTree.UnselectAll();

        if (sender is Grid grid && grid.DataContext is BranchTreeNode node)
        {
            if (node.IsBranch && DataContext is ViewModels.Repository repo)
            {
                var menu = repo.CreateContextMenuForLocalBranch(node.Backend as Branch);
                if (menu != null) menu.Open(grid);
            }
        }

        e.Handled = true;
    }

    void OnRemoteBranchContextMenuRequested(object sender, ContextRequestedEventArgs e)
    {
        localBranchTree.UnselectAll();

        if (sender is Grid grid && grid.DataContext is BranchTreeNode node && DataContext is ViewModels.Repository repo)
        {
            if (node.IsRemote)
            {
                var menu = repo.CreateContextMenuForRemote(node.Backend as Remote);
                if (menu != null) menu.Open(grid);
            }
            else if (node.IsBranch)
            {
                var menu = repo.CreateContextMenuForRemoteBranch(node.Backend as Branch);
                if (menu != null) menu.Open(grid);
            }
        }

        e.Handled = true;
    }

    void OnTagContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.SelectedItem != null && DataContext is ViewModels.Repository repo)
        {
            var tag = datagrid.SelectedItem as Tag;
            var menu = repo.CreateContextMenuForTag(tag);
            if (menu != null) menu.Open(datagrid);
        }

        e.Handled = true;
    }

    void OnSubmoduleContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.SelectedItem != null && DataContext is ViewModels.Repository repo)
        {
            var submodule = datagrid.SelectedItem as string;
            var menu = repo.CreateContextMenuForSubmodule(submodule);
            if (menu != null) menu.Open(datagrid);
        }

        e.Handled = true;
    }

    void OpenGitFlowMenu(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Repository repo)
        {
            var menu = repo.CreateContextMenuForGitFlow();
            if (menu != null) menu.Open(sender as Button);
        }

        e.Handled = true;
    }

    async void UpdateSubmodules(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Repository repo)
        {
            repo.SetWatcherEnabled(false);
            iconSubmoduleUpdate.Classes.Add("rotating");
            await Task.Run(() => new Submodule(repo.FullPath).Update());
            iconSubmoduleUpdate.Classes.Remove("rotating");
            repo.SetWatcherEnabled(true);
        }

        e.Handled = true;
    }

    void OnDoubleTappedLocalBranchNode(object sender, TappedEventArgs e)
    {
        if (!PopupHost.CanCreatePopup()) return;

        if (sender is Grid grid && DataContext is ViewModels.Repository repo)
        {
            var node = grid.DataContext as BranchTreeNode;
            if (node != null && node.IsBranch)
            {
                var branch = node.Backend as Branch;
                if (branch.IsCurrent) return;

                PopupHost.ShowAndStartPopup(new ViewModels.Checkout(repo, branch.Name));
                e.Handled = true;
            }
        }
    }

    async void OpenStatistics(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Repository repo)
        {
            var dialog = new Statistics
            {
                DataContext = new ViewModels.Statistics(repo.FullPath)
            };
            await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);
            e.Handled = true;
        }
    }
}

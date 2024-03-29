using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GitSpaces.Commands;
using GitSpaces.Models;
using GitSpaces.ViewModels;

namespace GitSpaces.OldViews;

public partial class WorkingCopy : UserControl
{
    public WorkingCopy()
    {
        InitializeComponent();
    }

    void ViewAssumeUnchanged(object sender, RoutedEventArgs e)
    {
        var repoPage = this.FindAncestorOfType<Repository>();
        if (repoPage != null)
        {
            var repo = (repoPage.DataContext as ViewModels.Repository).FullPath;
            var window = new AssumeUnchangedManager();
            window.DataContext = new ViewModels.AssumeUnchangedManager(repo);
            window.ShowDialog((Window)TopLevel.GetTopLevel(this));
        }

        e.Handled = true;
    }

    void StageSelected(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ViewModels.WorkingCopy;
        if (vm == null) return;

        var selected = new List<Change>();
        switch (Configs.Preference.Instance.UnstagedChangeViewMode)
        {
            case ChangeViewMode.List:
                foreach (var item in unstagedList.SelectedItems)
                {
                    if (item is Change change) selected.Add(change);
                }

                break;

            case ChangeViewMode.Grid:
                foreach (var item in unstagedGrid.SelectedItems)
                {
                    if (item is Change change) selected.Add(change);
                }

                break;

            default:
                foreach (var item in unstagedTree.SelectedItems)
                {
                    if (item is FileTreeNode node) CollectChangesFromNode(selected, node);
                }

                break;
        }

        vm.StageChanges(selected);
        e.Handled = true;
    }

    void StageAll(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ViewModels.WorkingCopy;
        if (vm == null) return;

        vm.StageChanges(vm.Unstaged);
        e.Handled = true;
    }

    void UnstageSelected(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ViewModels.WorkingCopy;
        if (vm == null) return;

        var selected = new List<Change>();
        switch (Configs.Preference.Instance.StagedChangeViewMode)
        {
            case ChangeViewMode.List:
                foreach (var item in stagedList.SelectedItems)
                {
                    if (item is Change change) selected.Add(change);
                }

                break;

            case ChangeViewMode.Grid:
                foreach (var item in stagedGrid.SelectedItems)
                {
                    if (item is Change change) selected.Add(change);
                }

                break;

            default:
                foreach (var item in stagedTree.SelectedItems)
                {
                    if (item is FileTreeNode node) CollectChangesFromNode(selected, node);
                }

                break;
        }

        vm.UnstageChanges(selected);
        e.Handled = true;
    }

    void UnstageAll(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ViewModels.WorkingCopy;
        if (vm == null) return;

        vm.UnstageChanges(vm.Staged);
        e.Handled = true;
    }

    void OnUnstagedListKeyDown(object sender, KeyEventArgs e)
    {
        var datagrid = sender as DataGrid;
        if (datagrid.SelectedItems.Count > 0 && e.Key == Key.Space && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in datagrid.SelectedItems)
            {
                if (item is Change change) selected.Add(change);
            }

            vm.StageChanges(selected);
        }

        e.Handled = true;
    }

    void OnUnstagedTreeViewKeyDown(object sender, KeyEventArgs e)
    {
        var tree = sender as TreeView;
        if (tree.SelectedItems.Count > 0 && e.Key == Key.Space && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in tree.SelectedItems)
            {
                if (item is FileTreeNode node) CollectChangesFromNode(selected, node);
            }

            vm.StageChanges(selected);
        }

        e.Handled = true;
    }

    void OnStagedListKeyDown(object sender, KeyEventArgs e)
    {
        var datagrid = sender as DataGrid;
        if (datagrid.SelectedItems.Count > 0 && e.Key == Key.Space && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in datagrid.SelectedItems)
            {
                if (item is Change change) selected.Add(change);
            }

            vm.UnstageChanges(selected);
        }

        e.Handled = true;
    }

    void OnStagedTreeViewKeyDown(object sender, KeyEventArgs e)
    {
        var tree = sender as TreeView;
        if (tree.SelectedItems.Count > 0 && e.Key == Key.Space && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in tree.SelectedItems)
            {
                if (item is FileTreeNode node) CollectChangesFromNode(selected, node);
            }

            vm.UnstageChanges(selected);
        }

        e.Handled = true;
    }

    void OnUnstagedListContextRequested(object sender, ContextRequestedEventArgs e)
    {
        var datagrid = sender as DataGrid;
        if (datagrid.SelectedItems.Count > 0 && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in datagrid.SelectedItems)
            {
                if (item is Change change) selected.Add(change);
            }

            var menu = vm.CreateContextMenuForUnstagedChanges(selected);
            if (menu != null) menu.Open(datagrid);
        }

        e.Handled = true;
    }

    void OnUnstagedTreeViewContextRequested(object sender, ContextRequestedEventArgs e)
    {
        var tree = sender as TreeView;
        if (tree.SelectedItems.Count > 0 && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in tree.SelectedItems)
            {
                if (item is FileTreeNode node) CollectChangesFromNode(selected, node);
            }

            var menu = vm.CreateContextMenuForUnstagedChanges(selected);
            if (menu != null) menu.Open(tree);
        }

        e.Handled = true;
    }

    void OnStagedListContextRequested(object sender, ContextRequestedEventArgs e)
    {
        var datagrid = sender as DataGrid;
        if (datagrid.SelectedItems.Count > 0 && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in datagrid.SelectedItems)
            {
                if (item is Change change) selected.Add(change);
            }

            var menu = vm.CreateContextMenuForStagedChanges(selected);
            if (menu != null) menu.Open(datagrid);
        }

        e.Handled = true;
    }

    void OnStagedTreeViewContextRequested(object sender, ContextRequestedEventArgs e)
    {
        var tree = sender as TreeView;
        if (tree.SelectedItems.Count > 0 && DataContext is ViewModels.WorkingCopy vm)
        {
            var selected = new List<Change>();
            foreach (var item in tree.SelectedItems)
            {
                if (item is FileTreeNode node) CollectChangesFromNode(selected, node);
            }

            var menu = vm.CreateContextMenuForStagedChanges(selected);
            if (menu != null) menu.Open(tree);
        }

        e.Handled = true;
    }

    void StartAmend(object sender, RoutedEventArgs e)
    {
        var repoPage = this.FindAncestorOfType<Repository>();
        if (repoPage != null)
        {
            var repo = (repoPage.DataContext as ViewModels.Repository).FullPath;
            var commits = new QueryCommits(repo, "-n 1", false).Result();
            if (commits.Count == 0)
            {
                App123.RaiseException(repo, "No commits to amend!!!");

                var chkBox = sender as CheckBox;
                chkBox.IsChecked = false;
                e.Handled = true;
                return;
            }

            var vm = DataContext as ViewModels.WorkingCopy;
            vm.CommitMessage = commits[0].FullMessage;
        }

        e.Handled = true;
    }

    void Commit(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ViewModels.WorkingCopy;
        vm.DoCommit(false);
        e.Handled = true;
    }

    void CommitWithPush(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ViewModels.WorkingCopy;
        vm.DoCommit(true);
        e.Handled = true;
    }

    void CollectChangesFromNode(List<Change> outs, FileTreeNode node)
    {
        if (node.IsFolder)
        {
            foreach (var child in node.Children) CollectChangesFromNode(outs, child);
        }
        else
        {
            var change = node.Backend as Change;
            if (change != null && !outs.Contains(change)) outs.Add(change);
        }
    }

    void OnOpenCommitMessagePicker(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && DataContext is ViewModels.WorkingCopy vm)
        {
            var menu = vm.CreateContextMenuForCommitMessages();
            menu.Placement = PlacementMode.TopEdgeAlignedLeft;
            menu.Open(button);
            e.Handled = true;
        }
    }
}

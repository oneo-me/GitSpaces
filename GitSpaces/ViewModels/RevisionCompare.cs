using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using GitSpaces.Models;
using GitSpaces.Native;
using Commit = GitSpaces.Models.Commit;

namespace GitSpaces.ViewModels;

public class RevisionCompare : ObservableObject
{
    public Commit StartPoint { get; }

    public Commit EndPoint { get; }

    public List<Change> VisibleChanges
    {
        get => _visibleChanges;
        private set => SetProperty(ref _visibleChanges, value);
    }

    public List<FileTreeNode> ChangeTree
    {
        get => _changeTree;
        private set => SetProperty(ref _changeTree, value);
    }

    public Change SelectedChange
    {
        get => _selectedChange;
        set
        {
            if (SetProperty(ref _selectedChange, value))
            {
                if (value == null)
                {
                    SelectedNode = null;
                    DiffContext = null;
                }
                else
                {
                    SelectedNode = FileTreeNode.SelectByPath(_changeTree, value.Path);
                    DiffContext = new(_repo, new(StartPoint.SHA, EndPoint.SHA, value), _diffContext);
                }
            }
        }
    }

    public FileTreeNode SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (SetProperty(ref _selectedNode, value))
            {
                if (value == null)
                {
                    SelectedChange = null;
                }
                else
                {
                    SelectedChange = value.Backend as Change;
                }
            }
        }
    }

    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            if (SetProperty(ref _searchFilter, value))
            {
                RefreshVisible();
            }
        }
    }

    public DiffContext DiffContext
    {
        get => _diffContext;
        private set => SetProperty(ref _diffContext, value);
    }

    public RevisionCompare(string repo, Commit startPoint, Commit endPoint)
    {
        _repo = repo;
        StartPoint = startPoint;
        EndPoint = endPoint;

        Task.Run(() =>
        {
            _changes = new CompareRevisions(_repo, startPoint.SHA, endPoint.SHA).Result();

            var visible = _changes;
            if (!string.IsNullOrWhiteSpace(_searchFilter))
            {
                visible = new();
                foreach (var c in _changes)
                {
                    if (c.Path.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        visible.Add(c);
                    }
                }
            }

            var tree = FileTreeNode.Build(visible);
            Dispatcher.UIThread.Invoke(() =>
            {
                VisibleChanges = visible;
                ChangeTree = tree;
            });
        });
    }

    public void Cleanup()
    {
        _repo = null;
        if (_changes != null) _changes.Clear();
        if (_visibleChanges != null) _visibleChanges.Clear();
        if (_changeTree != null) _changeTree.Clear();
        _selectedChange = null;
        _selectedNode = null;
        _searchFilter = null;
        _diffContext = null;
    }

    public void NavigateTo(string commitSHA)
    {
        var repo = Preference.FindRepository(_repo);
        if (repo != null) repo.NavigateToCommit(commitSHA);
    }

    public void ClearSearchFilter()
    {
        SearchFilter = string.Empty;
    }

    public ContextMenu CreateChangeContextMenu(Change change)
    {
        var menu = new ContextMenu();

        if (change.Index != ChangeState.Deleted)
        {
            var history = new MenuItem();
            history.Header = App.Text("FileHistory");
            history.Click += (_, ev) =>
            {
                var window = new Views.FileHistories
                {
                    DataContext = new FileHistories(_repo, change.Path)
                };
                window.Show();
                ev.Handled = true;
            };

            var full = Path.GetFullPath(Path.Combine(_repo, change.Path));
            var explore = new MenuItem();
            explore.Header = App.Text("RevealFile");
            explore.IsEnabled = File.Exists(full);
            explore.Click += (_, ev) =>
            {
                OS.OpenInFileManager(full, true);
                ev.Handled = true;
            };

            menu.Items.Add(history);
            menu.Items.Add(explore);
        }

        var copyPath = new MenuItem();
        copyPath.Header = App.Text("CopyPath");
        copyPath.Click += (_, ev) =>
        {
            App.CopyText(change.Path);
            ev.Handled = true;
        };

        menu.Items.Add(copyPath);
        return menu;
    }

    void RefreshVisible()
    {
        if (_changes == null) return;

        if (string.IsNullOrEmpty(_searchFilter))
        {
            VisibleChanges = _changes;
        }
        else
        {
            var visible = new List<Change>();
            foreach (var c in _changes)
            {
                if (c.Path.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                {
                    visible.Add(c);
                }
            }

            VisibleChanges = visible;
        }

        ChangeTree = FileTreeNode.Build(_visibleChanges);
    }

    string _repo = string.Empty;
    List<Change> _changes;
    List<Change> _visibleChanges;
    List<FileTreeNode> _changeTree;
    Change _selectedChange;
    FileTreeNode _selectedNode;
    string _searchFilter = string.Empty;
    DiffContext _diffContext;
}

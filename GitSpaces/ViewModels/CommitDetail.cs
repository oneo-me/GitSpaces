using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using GitSpaces.Models;
using GitSpaces.Native;
using Commit = GitSpaces.Models.Commit;
using Object = GitSpaces.Models.Object;

namespace GitSpaces.ViewModels;

public class CommitDetail : ObservableObject
{
    public DiffContext DiffContext
    {
        get => _diffContext;
        private set => SetProperty(ref _diffContext, value);
    }

    public int ActivePageIndex
    {
        get => _activePageIndex;
        set => SetProperty(ref _activePageIndex, value);
    }

    public Commit Commit
    {
        get => _commit;
        set
        {
            if (SetProperty(ref _commit, value)) Refresh();
        }
    }

    public List<Change> Changes
    {
        get => _changes;
        set => SetProperty(ref _changes, value);
    }

    public List<Change> VisibleChanges
    {
        get => _visibleChanges;
        set => SetProperty(ref _visibleChanges, value);
    }

    public List<FileTreeNode> ChangeTree
    {
        get => _changeTree;
        set => SetProperty(ref _changeTree, value);
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
                    SelectedChangeNode = null;
                    DiffContext = null;
                }
                else
                {
                    SelectedChangeNode = FileTreeNode.SelectByPath(_changeTree, value.Path);
                    DiffContext = new(_repo, new(_commit, value), _diffContext);
                }
            }
        }
    }

    public FileTreeNode SelectedChangeNode
    {
        get => _selectedChangeNode;
        set
        {
            if (SetProperty(ref _selectedChangeNode, value))
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

    public string SearchChangeFilter
    {
        get => _searchChangeFilter;
        set
        {
            if (SetProperty(ref _searchChangeFilter, value))
            {
                RefreshVisibleChanges();
            }
        }
    }

    public List<FileTreeNode> RevisionFilesTree
    {
        get => _revisionFilesTree;
        set => SetProperty(ref _revisionFilesTree, value);
    }

    public FileTreeNode SelectedRevisionFileNode
    {
        get => _selectedRevisionFileNode;
        set
        {
            if (SetProperty(ref _selectedRevisionFileNode, value) && value != null && !value.IsFolder)
            {
                RefreshViewRevisionFile(value.Backend as Object);
            }
            else
            {
                ViewRevisionFileContent = null;
            }
        }
    }

    public string SearchFileFilter
    {
        get => _searchFileFilter;
        set
        {
            if (SetProperty(ref _searchFileFilter, value))
            {
                RefreshVisibleFiles();
            }
        }
    }

    public object ViewRevisionFileContent
    {
        get => _viewRevisionFileContent;
        set => SetProperty(ref _viewRevisionFileContent, value);
    }

    public CommitDetail(string repo)
    {
        _repo = repo;
    }

    public void Cleanup()
    {
        _repo = null;
        _commit = null;
        if (_changes != null) _changes.Clear();
        if (_visibleChanges != null) _visibleChanges.Clear();
        if (_changeTree != null) _changeTree.Clear();
        _selectedChange = null;
        _selectedChangeNode = null;
        _searchChangeFilter = null;
        _diffContext = null;
        if (_revisionFiles != null) _revisionFiles.Clear();
        if (_revisionFilesTree != null) _revisionFilesTree.Clear();
        _selectedRevisionFileNode = null;
        _searchFileFilter = null;
        _viewRevisionFileContent = null;
        _cancelToken = null;
    }

    public void NavigateTo(string commitSHA)
    {
        var repo = Preference.FindRepository(_repo);
        if (repo != null) repo.NavigateToCommit(commitSHA);
    }

    public void ClearSearchChangeFilter()
    {
        SearchChangeFilter = string.Empty;
    }

    public void ClearSearchFileFilter()
    {
        SearchFileFilter = string.Empty;
    }

    public ContextMenu CreateChangeContextMenu(Change change)
    {
        var menu = new ContextMenu();

        if (change.Index != ChangeState.Deleted)
        {
            var history = new MenuItem();
            history.Header = App.Text("FileHistory");
            history.Icon = App.CreateMenuIcon("Icons.Histories");
            history.Click += (_, ev) =>
            {
                var window = new OldViews.FileHistories
                {
                    DataContext = new FileHistories(_repo, change.Path)
                };
                window.Show();
                ev.Handled = true;
            };

            var blame = new MenuItem();
            blame.Header = App.Text("Blame");
            blame.Icon = App.CreateMenuIcon("Icons.Blame");
            blame.Click += (o, ev) =>
            {
                var window = new OldViews.Blame
                {
                    DataContext = new Blame(_repo, change.Path, _commit.SHA)
                };
                window.Show();
                ev.Handled = true;
            };

            var full = Path.GetFullPath(Path.Combine(_repo, change.Path));
            var explore = new MenuItem();
            explore.Header = App.Text("RevealFile");
            explore.Icon = App.CreateMenuIcon("Icons.Folder.Open");
            explore.IsEnabled = File.Exists(full);
            explore.Click += (_, ev) =>
            {
                OS.OpenInFileManager(full, true);
                ev.Handled = true;
            };

            menu.Items.Add(history);
            menu.Items.Add(blame);
            menu.Items.Add(explore);
        }

        var copyPath = new MenuItem();
        copyPath.Header = App.Text("CopyPath");
        copyPath.Icon = App.CreateMenuIcon("Icons.Copy");
        copyPath.Click += (_, ev) =>
        {
            App.CopyText(change.Path);
            ev.Handled = true;
        };

        menu.Items.Add(copyPath);
        return menu;
    }

    public ContextMenu CreateRevisionFileContextMenu(Object file)
    {
        var history = new MenuItem();
        history.Header = App.Text("FileHistory");
        history.Icon = App.CreateMenuIcon("Icons.Histories");
        history.Click += (_, ev) =>
        {
            var window = new OldViews.FileHistories
            {
                DataContext = new FileHistories(_repo, file.Path)
            };
            window.Show();
            ev.Handled = true;
        };

        var blame = new MenuItem();
        blame.Header = App.Text("Blame");
        blame.Icon = App.CreateMenuIcon("Icons.Blame");
        blame.Click += (o, ev) =>
        {
            var window = new OldViews.Blame
            {
                DataContext = new Blame(_repo, file.Path, _commit.SHA)
            };
            window.Show();
            ev.Handled = true;
        };

        var full = Path.GetFullPath(Path.Combine(_repo, file.Path));
        var explore = new MenuItem();
        explore.Header = App.Text("RevealFile");
        explore.Icon = App.CreateMenuIcon("Icons.Folder.Open");
        explore.Click += (_, ev) =>
        {
            OS.OpenInFileManager(full, file.Type == ObjectType.Blob);
            ev.Handled = true;
        };

        var saveAs = new MenuItem();
        saveAs.Header = App.Text("SaveAs");
        saveAs.Icon = App.CreateMenuIcon("Icons.Save");
        saveAs.IsEnabled = file.Type == ObjectType.Blob;
        saveAs.Click += async (_, ev) =>
        {
            var topLevel = App.GetTopLevel();
            if (topLevel == null) return;

            var options = new FolderPickerOpenOptions
            {
                AllowMultiple = false
            };
            var selected = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
            if (selected.Count == 1)
            {
                var saveTo = Path.Combine(selected[0].Path.LocalPath, Path.GetFileName(file.Path));
                SaveRevisionFile.Run(_repo, _commit.SHA, file.Path, saveTo);
            }

            ev.Handled = true;
        };

        var copyPath = new MenuItem();
        copyPath.Header = App.Text("CopyPath");
        copyPath.Icon = App.CreateMenuIcon("Icons.Copy");
        copyPath.Click += (_, ev) =>
        {
            App.CopyText(file.Path);
            ev.Handled = true;
        };

        var menu = new ContextMenu();
        menu.Items.Add(history);
        menu.Items.Add(blame);
        menu.Items.Add(explore);
        menu.Items.Add(saveAs);
        menu.Items.Add(copyPath);
        return menu;
    }

    void Refresh()
    {
        _changes = null;
        VisibleChanges = null;
        SelectedChange = null;
        RevisionFilesTree = null;
        SelectedRevisionFileNode = null;
        if (_commit == null) return;
        if (_cancelToken != null) _cancelToken.Requested = true;

        _cancelToken = new();
        var cmdChanges = new QueryCommitChanges(_repo, _commit.SHA)
        {
            Cancel = _cancelToken
        };
        var cmdRevisionFiles = new QueryRevisionObjects(_repo, _commit.SHA)
        {
            Cancel = _cancelToken
        };

        Task.Run(() =>
        {
            var changes = cmdChanges.Result();
            if (cmdChanges.Cancel.Requested) return;

            var visible = changes;
            if (!string.IsNullOrWhiteSpace(_searchChangeFilter))
            {
                visible = new();
                foreach (var c in changes)
                {
                    if (c.Path.Contains(_searchChangeFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        visible.Add(c);
                    }
                }
            }

            var tree = FileTreeNode.Build(visible);
            Dispatcher.UIThread.Invoke(() =>
            {
                Changes = changes;
                VisibleChanges = visible;
                ChangeTree = tree;
            });
        });

        Task.Run(() =>
        {
            var files = cmdRevisionFiles.Result();
            if (cmdRevisionFiles.Cancel.Requested) return;

            var visible = files;
            if (!string.IsNullOrWhiteSpace(_searchFileFilter))
            {
                visible = new();
                foreach (var f in files)
                {
                    if (f.Path.Contains(_searchFileFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        visible.Add(f);
                    }
                }
            }

            var tree = FileTreeNode.Build(visible);
            Dispatcher.UIThread.Invoke(() =>
            {
                _revisionFiles = files;
                RevisionFilesTree = tree;
            });
        });
    }

    void RefreshVisibleChanges()
    {
        if (_changes == null) return;

        if (string.IsNullOrEmpty(_searchChangeFilter))
        {
            VisibleChanges = _changes;
        }
        else
        {
            var visible = new List<Change>();
            foreach (var c in _changes)
            {
                if (c.Path.Contains(_searchChangeFilter, StringComparison.OrdinalIgnoreCase))
                {
                    visible.Add(c);
                }
            }

            VisibleChanges = visible;
        }

        ChangeTree = FileTreeNode.Build(_visibleChanges);
    }

    void RefreshVisibleFiles()
    {
        if (_revisionFiles == null) return;

        var visible = _revisionFiles;
        if (!string.IsNullOrWhiteSpace(_searchFileFilter))
        {
            visible = new();
            foreach (var f in _revisionFiles)
            {
                if (f.Path.Contains(_searchFileFilter, StringComparison.OrdinalIgnoreCase))
                {
                    visible.Add(f);
                }
            }
        }

        RevisionFilesTree = FileTreeNode.Build(visible);
    }

    void RefreshViewRevisionFile(Object file)
    {
        switch (file.Type)
        {
            case ObjectType.Blob:
                Task.Run(() =>
                {
                    var isBinary = new IsBinary(_repo, _commit.SHA, file.Path).Result();
                    if (isBinary)
                    {
                        var ext = Path.GetExtension(file.Path);
                        if (IMG_EXTS.Contains(ext))
                        {
                            var bitmap = GetImageFileAsBitmap.Run(_repo, _commit.SHA, file.Path);
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ViewRevisionFileContent = new RevisionImageFile
                                {
                                    Image = bitmap
                                };
                            });
                        }
                        else
                        {
                            var size = new QueryFileSize(_repo, file.Path, _commit.SHA).Result();
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ViewRevisionFileContent = new RevisionBinaryFile
                                {
                                    Size = size
                                };
                            });
                        }

                        return;
                    }

                    var content = new QueryFileContent(_repo, _commit.SHA, file.Path).Result();
                    if (content.StartsWith("version https://git-lfs.github.com/spec/", StringComparison.Ordinal))
                    {
                        var obj = new RevisionLFSObject
                        {
                            Object = new()
                        };
                        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length == 3)
                        {
                            foreach (var line in lines)
                            {
                                if (line.StartsWith("oid sha256:", StringComparison.Ordinal))
                                {
                                    obj.Object.Oid = line.Substring(11);
                                }
                                else if (line.StartsWith("size ", StringComparison.Ordinal))
                                {
                                    obj.Object.Size = long.Parse(line.Substring(5));
                                }
                            }

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ViewRevisionFileContent = obj;
                            });
                            return;
                        }
                    }

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ViewRevisionFileContent = new RevisionTextFile
                        {
                            FileName = file.Path, Content = content
                        };
                    });
                });
                break;

            case ObjectType.Commit:
                ViewRevisionFileContent = new RevisionSubmodule
                {
                    SHA = file.SHA
                };
                break;

            default:
                ViewRevisionFileContent = null;
                break;
        }
    }

    static readonly HashSet<string> IMG_EXTS = new()
    {
        ".ico",
        ".bmp",
        ".jpg",
        ".png",
        ".jpeg"
    };

    string _repo = string.Empty;
    int _activePageIndex;
    Commit _commit;
    List<Change> _changes;
    List<Change> _visibleChanges;
    List<FileTreeNode> _changeTree;
    Change _selectedChange;
    FileTreeNode _selectedChangeNode;
    string _searchChangeFilter = string.Empty;
    DiffContext _diffContext;
    List<Object> _revisionFiles;
    List<FileTreeNode> _revisionFilesTree;
    FileTreeNode _selectedRevisionFileNode;
    string _searchFileFilter = string.Empty;
    object _viewRevisionFileContent;
    Command.CancelToken _cancelToken;
}

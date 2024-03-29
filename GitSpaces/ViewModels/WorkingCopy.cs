using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using GitSpaces.Configs;
using GitSpaces.Models;
using GitSpaces.Native;
using Commit = GitSpaces.Commands.Commit;

namespace GitSpaces.ViewModels;

public class ConflictContext
{
    public Change Change { get; set; }
}

public class WorkingCopy : ObservableObject
{
    public bool IsStaging
    {
        get => _isStaging;
        private set => SetProperty(ref _isStaging, value);
    }

    public bool IsUnstaging
    {
        get => _isUnstaging;
        private set => SetProperty(ref _isUnstaging, value);
    }

    public bool IsCommitting
    {
        get => _isCommitting;
        private set => SetProperty(ref _isCommitting, value);
    }

    public bool UseAmend
    {
        get => _useAmend;
        set => SetProperty(ref _useAmend, value);
    }

    public List<Change> Unstaged
    {
        get => _unstaged;
        private set => SetProperty(ref _unstaged, value);
    }

    public List<Change> Staged
    {
        get => _staged;
        private set => SetProperty(ref _staged, value);
    }

    public int Count { get; private set; }

    public Change SelectedUnstagedChange
    {
        get => _selectedUnstagedChange;
        set
        {
            if (SetProperty(ref _selectedUnstagedChange, value) && value != null)
            {
                SelectedStagedChange = null;
                SelectedStagedTreeNode = null;
                SetDetail(value, true);
            }
        }
    }

    public Change SelectedStagedChange
    {
        get => _selectedStagedChange;
        set
        {
            if (SetProperty(ref _selectedStagedChange, value) && value != null)
            {
                SelectedUnstagedChange = null;
                SelectedUnstagedTreeNode = null;
                SetDetail(value, false);
            }
        }
    }

    public List<FileTreeNode> UnstagedTree
    {
        get => _unstagedTree;
        private set => SetProperty(ref _unstagedTree, value);
    }

    public List<FileTreeNode> StagedTree
    {
        get => _stagedTree;
        private set => SetProperty(ref _stagedTree, value);
    }

    public FileTreeNode SelectedUnstagedTreeNode
    {
        get => _selectedUnstagedTreeNode;
        set
        {
            if (SetProperty(ref _selectedUnstagedTreeNode, value))
            {
                if (value == null)
                {
                    SelectedUnstagedChange = null;
                }
                else
                {
                    SelectedUnstagedChange = value.Backend as Change;
                    SelectedStagedTreeNode = null;
                    SelectedStagedChange = null;

                    if (value.IsFolder)
                    {
                        SetDetail(null, true);
                    }
                }
            }
        }
    }

    public FileTreeNode SelectedStagedTreeNode
    {
        get => _selectedStagedTreeNode;
        set
        {
            if (SetProperty(ref _selectedStagedTreeNode, value))
            {
                if (value == null)
                {
                    SelectedStagedChange = null;
                }
                else
                {
                    SelectedStagedChange = value.Backend as Change;
                    SelectedUnstagedTreeNode = null;
                    SelectedUnstagedChange = null;

                    if (value.IsFolder)
                    {
                        SetDetail(null, false);
                    }
                }
            }
        }
    }

    public object DetailContext
    {
        get => _detailContext;
        private set => SetProperty(ref _detailContext, value);
    }

    public string CommitMessage
    {
        get => _commitMessage;
        set => SetProperty(ref _commitMessage, value);
    }

    public WorkingCopy(Repository repo)
    {
        _repo = repo;
    }

    public void Cleanup()
    {
        _repo = null;
        if (_unstaged != null) _unstaged.Clear();
        if (_staged != null) _staged.Clear();
        if (_unstagedTree != null) _unstagedTree.Clear();
        if (_stagedTree != null) _stagedTree.Clear();
        _selectedUnstagedChange = null;
        _selectedStagedChange = null;
        _selectedUnstagedTreeNode = null;
        _selectedStagedTreeNode = null;
        _detailContext = null;
        _commitMessage = string.Empty;
    }

    public bool SetData(List<Change> changes)
    {
        var unstaged = new List<Change>();
        var staged = new List<Change>();

        var viewFile = string.Empty;
        var lastSelectedIsUnstaged = false;
        if (_selectedUnstagedChange != null)
        {
            viewFile = _selectedUnstagedChange.Path;
            lastSelectedIsUnstaged = true;
        }
        else if (_selectedStagedChange != null)
        {
            viewFile = _selectedStagedChange.Path;
        }

        var viewChange = null as Change;
        var hasConflict = false;
        foreach (var c in changes)
        {
            if (c.Index == ChangeState.Modified
                || c.Index == ChangeState.Added
                || c.Index == ChangeState.Deleted
                || c.Index == ChangeState.Renamed)
            {
                staged.Add(c);
                if (!lastSelectedIsUnstaged && c.Path == viewFile)
                {
                    viewChange = c;
                }
            }

            if (c.WorkTree != ChangeState.None)
            {
                unstaged.Add(c);
                hasConflict |= c.IsConflit;
                if (lastSelectedIsUnstaged && c.Path == viewFile)
                {
                    viewChange = c;
                }
            }
        }

        Count = changes.Count;

        var unstagedTree = FileTreeNode.Build(unstaged);
        var stagedTree = FileTreeNode.Build(staged);
        Dispatcher.UIThread.Invoke(() =>
        {
            _isLoadingData = true;
            Unstaged = unstaged;
            Staged = staged;
            UnstagedTree = unstagedTree;
            StagedTree = stagedTree;
            _isLoadingData = false;

            // Restore last selection states.
            if (viewChange != null)
            {
                var scrollOffset = Vector.Zero;
                if (_detailContext is DiffContext old) scrollOffset = old.SyncScrollOffset;

                if (lastSelectedIsUnstaged)
                {
                    SelectedUnstagedChange = viewChange;
                    SelectedUnstagedTreeNode = FileTreeNode.SelectByPath(_unstagedTree, viewFile);
                }
                else
                {
                    SelectedStagedChange = viewChange;
                    SelectedStagedTreeNode = FileTreeNode.SelectByPath(_stagedTree, viewFile);
                }

                if (_detailContext is DiffContext cur) cur.SyncScrollOffset = scrollOffset;
            }
            else
            {
                SelectedUnstagedChange = null;
                SelectedUnstagedTreeNode = null;
                SelectedStagedChange = null;
                SelectedStagedTreeNode = null;
                SetDetail(null, false);
            }
        });

        return hasConflict;
    }

    public void SetDetail(Change change, bool isUnstaged)
    {
        if (_isLoadingData) return;

        if (change == null)
        {
            DetailContext = null;
        }
        else if (change.IsConflit)
        {
            DetailContext = new ConflictContext
            {
                Change = change
            };
        }
        else
        {
            if (_detailContext is DiffContext previous)
            {
                DetailContext = new DiffContext(_repo.FullPath, new(change, isUnstaged), previous);
            }
            else
            {
                DetailContext = new DiffContext(_repo.FullPath, new(change, isUnstaged));
            }
        }
    }

    public async void StageChanges(List<Change> changes)
    {
        if (_unstaged.Count == 0 || changes.Count == 0) return;

        SetDetail(null, true);
        IsStaging = true;
        _repo.SetWatcherEnabled(false);
        if (changes.Count == _unstaged.Count)
        {
            await Task.Run(() => new Add(_repo.FullPath).Exec());
        }
        else
        {
            for (var i = 0; i < changes.Count; i += 10)
            {
                var count = Math.Min(10, changes.Count - i);
                var step = changes.GetRange(i, count);
                await Task.Run(() => new Add(_repo.FullPath, step).Exec());
            }
        }

        _repo.MarkWorkingCopyDirtyManually();
        _repo.SetWatcherEnabled(true);
        IsStaging = false;
    }

    public async void UnstageChanges(List<Change> changes)
    {
        if (_staged.Count == 0 || changes.Count == 0) return;

        SetDetail(null, false);
        IsUnstaging = true;
        _repo.SetWatcherEnabled(false);
        if (changes.Count == _staged.Count)
        {
            await Task.Run(() => new Commands.Reset(_repo.FullPath).Exec());
        }
        else
        {
            for (var i = 0; i < changes.Count; i += 10)
            {
                var count = Math.Min(10, changes.Count - i);
                var step = changes.GetRange(i, count);
                await Task.Run(() => new Commands.Reset(_repo.FullPath, step).Exec());
            }
        }

        _repo.MarkWorkingCopyDirtyManually();
        _repo.SetWatcherEnabled(true);
        IsUnstaging = false;
    }

    public void Discard(List<Change> changes, bool isUnstaged)
    {
        if (PopupHost.CanCreatePopup())
        {
            if (isUnstaged)
            {
                if (changes.Count == _unstaged.Count && _staged.Count == 0)
                {
                    PopupHost.ShowPopup(new Discard(_repo));
                }
                else
                {
                    PopupHost.ShowPopup(new Discard(_repo, changes, true));
                }
            }
            else
            {
                if (changes.Count == _staged.Count && _unstaged.Count == 0)
                {
                    PopupHost.ShowPopup(new Discard(_repo));
                }
                else
                {
                    PopupHost.ShowPopup(new Discard(_repo, changes, false));
                }
            }
        }
    }

    public async void UseTheirs()
    {
        if (_detailContext is ConflictContext ctx)
        {
            _repo.SetWatcherEnabled(false);
            var succ = await Task.Run(() => new Commands.Checkout(_repo.FullPath).File(ctx.Change.Path, true));
            if (succ)
            {
                await Task.Run(() => new Add(_repo.FullPath, [ctx.Change]).Exec());
            }

            _repo.MarkWorkingCopyDirtyManually();
            _repo.SetWatcherEnabled(true);
        }
    }

    public async void UseMine()
    {
        if (_detailContext is ConflictContext ctx)
        {
            _repo.SetWatcherEnabled(false);
            var succ = await Task.Run(() => new Commands.Checkout(_repo.FullPath).File(ctx.Change.Path, false));
            if (succ)
            {
                await Task.Run(() => new Add(_repo.FullPath, [ctx.Change]).Exec());
            }

            _repo.MarkWorkingCopyDirtyManually();
            _repo.SetWatcherEnabled(true);
        }
    }

    public async void UseExternalMergeTool()
    {
        if (_detailContext is ConflictContext ctx)
        {
            var type = Preference.Instance.ExternalMergeToolType;
            var exec = Preference.Instance.ExternalMergeToolPath;

            var tool = ExternalMergeTools.Supported.Find(x => x.Type == type);
            if (tool == null)
            {
                App123.RaiseException(_repo.FullPath, "Invalid merge tool in preference setting!");
                return;
            }

            var args = tool.Type != 0 ? tool.Cmd : Preference.Instance.ExternalMergeToolCmd;

            _repo.SetWatcherEnabled(false);
            await Task.Run(() => MergeTool.OpenForMerge(_repo.FullPath, exec, args, ctx.Change.Path));
            _repo.SetWatcherEnabled(true);
        }
    }

    public async void DoCommit(bool autoPush)
    {
        if (!PopupHost.CanCreatePopup())
        {
            App123.RaiseException(_repo.FullPath, "Repository has unfinished job! Please wait!");
            return;
        }

        if (_staged.Count == 0)
        {
            App123.RaiseException(_repo.FullPath, "No files added to commit!");
            return;
        }

        if (string.IsNullOrWhiteSpace(_commitMessage))
        {
            App123.RaiseException(_repo.FullPath, "Commit without message is NOT allowed!");
            return;
        }

        PushCommitMessage();

        SetDetail(null, false);
        IsCommitting = true;
        _repo.SetWatcherEnabled(false);
        var succ = await Task.Run(() => new Commit(_repo.FullPath, _commitMessage, _useAmend).Exec());
        if (succ)
        {
            CommitMessage = string.Empty;
            UseAmend = false;

            if (autoPush)
            {
                PopupHost.ShowAndStartPopup(new Push(_repo, null));
            }
        }

        _repo.MarkWorkingCopyDirtyManually();
        _repo.SetWatcherEnabled(true);
        IsCommitting = false;
    }

    public ContextMenu CreateContextMenuForUnstagedChanges(List<Change> changes)
    {
        if (changes.Count == 0) return null;

        var menu = new ContextMenu();
        if (changes.Count == 1)
        {
            var change = changes[0];
            var path = Path.GetFullPath(Path.Combine(_repo.FullPath, change.Path));

            var explore = new MenuItem();
            explore.Header = App123.Text("RevealFile");
            explore.Icon = App123.CreateMenuIcon("Icons.Folder.Open");
            explore.IsEnabled = File.Exists(path) || Directory.Exists(path);
            explore.Click += (_, e) =>
            {
                OS.OpenInFileManager(path, true);
                e.Handled = true;
            };

            var openWith = new MenuItem();
            openWith.Header = App123.Text("OpenWith");
            openWith.Icon = App123.CreateMenuIcon("Icons.OpenWith");
            openWith.IsEnabled = File.Exists(path);
            openWith.Click += (_, e) =>
            {
                OS.OpenWithDefaultEditor(path);
                e.Handled = true;
            };

            var stage = new MenuItem();
            stage.Header = App123.Text("FileCM.Stage");
            stage.Icon = App123.CreateMenuIcon("Icons.File.Add");
            stage.Click += (_, e) =>
            {
                StageChanges(changes);
                e.Handled = true;
            };

            var discard = new MenuItem();
            discard.Header = App123.Text("FileCM.Discard");
            discard.Icon = App123.CreateMenuIcon("Icons.Undo");
            discard.Click += (_, e) =>
            {
                Discard(changes, true);
                e.Handled = true;
            };

            var stash = new MenuItem();
            stash.Header = App123.Text("FileCM.Stash");
            stash.Icon = App123.CreateMenuIcon("Icons.Stashes");
            stash.Click += (_, e) =>
            {
                if (PopupHost.CanCreatePopup())
                {
                    PopupHost.ShowPopup(new StashChanges(_repo, changes, false));
                }

                e.Handled = true;
            };

            var patch = new MenuItem();
            patch.Header = App123.Text("FileCM.SaveAsPatch");
            patch.Icon = App123.CreateMenuIcon("Icons.Diff");
            patch.Click += async (_, e) =>
            {
                var topLevel = App123.GetTopLevel();
                if (topLevel == null) return;

                var options = new FilePickerSaveOptions();
                options.Title = App123.Text("FileCM.SaveAsPatch");
                options.DefaultExtension = ".patch";
                options.FileTypeChoices =
                [
                    new("Patch File")
                    {
                        Patterns = ["*.patch"]
                    }
                ];

                var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(options);
                if (storageFile != null)
                {
                    var succ = await Task.Run(() => SaveChangesAsPatch.Exec(_repo.FullPath, changes, true, storageFile.Path.LocalPath));
                    if (succ) App123.SendNotification(_repo.FullPath, App123.Text("SaveAsPatchSuccess"));
                }

                e.Handled = true;
            };

            var history = new MenuItem();
            history.Header = App123.Text("FileHistory");
            history.Icon = App123.CreateMenuIcon("Icons.Histories");
            history.Click += (_, e) =>
            {
                var window = new OldViews.FileHistories
                {
                    DataContext = new FileHistories(_repo.FullPath, change.Path)
                };
                window.Show();
                e.Handled = true;
            };

            var assumeUnchanged = new MenuItem();
            assumeUnchanged.Header = App123.Text("FileCM.AssumeUnchanged");
            assumeUnchanged.Icon = App123.CreateMenuIcon("Icons.File.Ignore");
            assumeUnchanged.IsEnabled = change.WorkTree != ChangeState.Untracked;
            assumeUnchanged.Click += (_, e) =>
            {
                new AssumeUnchanged(_repo.FullPath).Add(change.Path);
                e.Handled = true;
            };

            var copy = new MenuItem();
            copy.Header = App123.Text("CopyPath");
            copy.Icon = App123.CreateMenuIcon("Icons.Copy");
            copy.Click += (_, e) =>
            {
                App123.CopyText(change.Path);
                e.Handled = true;
            };

            menu.Items.Add(explore);
            menu.Items.Add(openWith);
            menu.Items.Add(new MenuItem
            {
                Header = "-"
            });
            menu.Items.Add(stage);
            menu.Items.Add(discard);
            menu.Items.Add(stash);
            menu.Items.Add(patch);
            menu.Items.Add(new MenuItem
            {
                Header = "-"
            });
            menu.Items.Add(history);
            menu.Items.Add(assumeUnchanged);
            menu.Items.Add(new MenuItem
            {
                Header = "-"
            });
            menu.Items.Add(copy);
        }
        else
        {
            var stage = new MenuItem();
            stage.Header = App123.Text("FileCM.StageMulti", changes.Count);
            stage.Icon = App123.CreateMenuIcon("Icons.File.Add");
            stage.Click += (_, e) =>
            {
                StageChanges(changes);
                e.Handled = true;
            };

            var discard = new MenuItem();
            discard.Header = App123.Text("FileCM.DiscardMulti", changes.Count);
            discard.Icon = App123.CreateMenuIcon("Icons.Undo");
            discard.Click += (_, e) =>
            {
                Discard(changes, true);
                e.Handled = true;
            };

            var stash = new MenuItem();
            stash.Header = App123.Text("FileCM.StashMulti", changes.Count);
            stash.Icon = App123.CreateMenuIcon("Icons.Stashes");
            stash.Click += (_, e) =>
            {
                if (PopupHost.CanCreatePopup())
                {
                    PopupHost.ShowPopup(new StashChanges(_repo, changes, false));
                }

                e.Handled = true;
            };

            var patch = new MenuItem();
            patch.Header = App123.Text("FileCM.SaveAsPatch");
            patch.Icon = App123.CreateMenuIcon("Icons.Diff");
            patch.Click += async (o, e) =>
            {
                var topLevel = App123.GetTopLevel();
                if (topLevel == null) return;

                var options = new FilePickerSaveOptions();
                options.Title = App123.Text("FileCM.SaveAsPatch");
                options.DefaultExtension = ".patch";
                options.FileTypeChoices =
                [
                    new("Patch File")
                    {
                        Patterns = ["*.patch"]
                    }
                ];

                var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(options);
                if (storageFile != null)
                {
                    var succ = await Task.Run(() => SaveChangesAsPatch.Exec(_repo.FullPath, changes, true, storageFile.Path.LocalPath));
                    if (succ) App123.SendNotification(_repo.FullPath, App123.Text("SaveAsPatchSuccess"));
                }

                e.Handled = true;
            };

            menu.Items.Add(stage);
            menu.Items.Add(discard);
            menu.Items.Add(stash);
            menu.Items.Add(patch);
        }

        return menu;
    }

    public ContextMenu CreateContextMenuForStagedChanges(List<Change> changes)
    {
        if (changes.Count == 0) return null;

        var menu = new ContextMenu();
        if (changes.Count == 1)
        {
            var change = changes[0];
            var path = Path.GetFullPath(Path.Combine(_repo.FullPath, change.Path));

            var explore = new MenuItem();
            explore.IsEnabled = File.Exists(path) || Directory.Exists(path);
            explore.Header = App123.Text("RevealFile");
            explore.Icon = App123.CreateMenuIcon("Icons.Folder.Open");
            explore.Click += (o, e) =>
            {
                OS.OpenInFileManager(path, true);
                e.Handled = true;
            };

            var openWith = new MenuItem();
            openWith.Header = App123.Text("OpenWith");
            openWith.Icon = App123.CreateMenuIcon("Icons.OpenWith");
            openWith.IsEnabled = File.Exists(path);
            openWith.Click += (_, e) =>
            {
                OS.OpenWithDefaultEditor(path);
                e.Handled = true;
            };

            var unstage = new MenuItem();
            unstage.Header = App123.Text("FileCM.Unstage");
            unstage.Icon = App123.CreateMenuIcon("Icons.File.Remove");
            unstage.Click += (o, e) =>
            {
                UnstageChanges(changes);
                e.Handled = true;
            };

            var discard = new MenuItem();
            discard.Header = App123.Text("FileCM.Discard");
            discard.Icon = App123.CreateMenuIcon("Icons.Undo");
            discard.Click += (_, e) =>
            {
                Discard(changes, false);
                e.Handled = true;
            };

            var stash = new MenuItem();
            stash.Header = App123.Text("FileCM.Stash");
            stash.Icon = App123.CreateMenuIcon("Icons.Stashes");
            stash.Click += (_, e) =>
            {
                if (PopupHost.CanCreatePopup())
                {
                    PopupHost.ShowPopup(new StashChanges(_repo, changes, false));
                }

                e.Handled = true;
            };

            var patch = new MenuItem();
            patch.Header = App123.Text("FileCM.SaveAsPatch");
            patch.Icon = App123.CreateMenuIcon("Icons.Diff");
            patch.Click += async (o, e) =>
            {
                var topLevel = App123.GetTopLevel();
                if (topLevel == null) return;

                var options = new FilePickerSaveOptions();
                options.Title = App123.Text("FileCM.SaveAsPatch");
                options.DefaultExtension = ".patch";
                options.FileTypeChoices =
                [
                    new("Patch File")
                    {
                        Patterns = ["*.patch"]
                    }
                ];

                var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(options);
                if (storageFile != null)
                {
                    var succ = await Task.Run(() => SaveChangesAsPatch.Exec(_repo.FullPath, changes, false, storageFile.Path.LocalPath));
                    if (succ) App123.SendNotification(_repo.FullPath, App123.Text("SaveAsPatchSuccess"));
                }

                e.Handled = true;
            };

            var copyPath = new MenuItem();
            copyPath.Header = App123.Text("CopyPath");
            copyPath.Icon = App123.CreateMenuIcon("Icons.Copy");
            copyPath.Click += (o, e) =>
            {
                App123.CopyText(change.Path);
                e.Handled = true;
            };

            menu.Items.Add(explore);
            menu.Items.Add(openWith);
            menu.Items.Add(new MenuItem
            {
                Header = "-"
            });
            menu.Items.Add(unstage);
            menu.Items.Add(discard);
            menu.Items.Add(stash);
            menu.Items.Add(patch);
            menu.Items.Add(new MenuItem
            {
                Header = "-"
            });
            menu.Items.Add(copyPath);
        }
        else
        {
            var unstage = new MenuItem();
            unstage.Header = App123.Text("FileCM.UnstageMulti", changes.Count);
            unstage.Icon = App123.CreateMenuIcon("Icons.File.Remove");
            unstage.Click += (o, e) =>
            {
                UnstageChanges(changes);
                e.Handled = true;
            };

            var discard = new MenuItem();
            discard.Header = App123.Text("FileCM.DiscardMulti", changes.Count);
            discard.Icon = App123.CreateMenuIcon("Icons.Undo");
            discard.Click += (_, e) =>
            {
                Discard(changes, false);
                e.Handled = true;
            };

            var stash = new MenuItem();
            stash.Header = App123.Text("FileCM.StashMulti", changes.Count);
            stash.Icon = App123.CreateMenuIcon("Icons.Stashes");
            stash.Click += (_, e) =>
            {
                if (PopupHost.CanCreatePopup())
                {
                    PopupHost.ShowPopup(new StashChanges(_repo, changes, false));
                }

                e.Handled = true;
            };

            var patch = new MenuItem();
            patch.Header = App123.Text("FileCM.SaveAsPatch");
            patch.Icon = App123.CreateMenuIcon("Icons.Diff");
            patch.Click += async (_, e) =>
            {
                var topLevel = App123.GetTopLevel();
                if (topLevel == null) return;

                var options = new FilePickerSaveOptions();
                options.Title = App123.Text("FileCM.SaveAsPatch");
                options.DefaultExtension = ".patch";
                options.FileTypeChoices =
                [
                    new("Patch File")
                    {
                        Patterns = ["*.patch"]
                    }
                ];

                var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(options);
                if (storageFile != null)
                {
                    var succ = await Task.Run(() => SaveChangesAsPatch.Exec(_repo.FullPath, changes, false, storageFile.Path.LocalPath));
                    if (succ) App123.SendNotification(_repo.FullPath, App123.Text("SaveAsPatchSuccess"));
                }

                e.Handled = true;
            };

            menu.Items.Add(unstage);
            menu.Items.Add(discard);
            menu.Items.Add(stash);
            menu.Items.Add(patch);
        }

        return menu;
    }

    public ContextMenu CreateContextMenuForCommitMessages()
    {
        var menu = new ContextMenu();
        if (_repo.CommitMessages.Count == 0)
        {
            var empty = new MenuItem();
            empty.Header = App123.Text("WorkingCopy.NoCommitHistories");
            empty.IsEnabled = false;
            menu.Items.Add(empty);
            return menu;
        }

        var tip = new MenuItem();
        tip.Header = App123.Text("WorkingCopy.HasCommitHistories");
        tip.IsEnabled = false;
        menu.Items.Add(tip);
        menu.Items.Add(new MenuItem
        {
            Header = "-"
        });

        foreach (var message in _repo.CommitMessages)
        {
            var dump = message;

            var item = new MenuItem();
            item.Header = dump;
            item.Click += (o, e) =>
            {
                CommitMessage = dump;
                e.Handled = true;
            };

            menu.Items.Add(item);
        }

        return menu;
    }

    void PushCommitMessage()
    {
        var existIdx = _repo.CommitMessages.IndexOf(CommitMessage);
        if (existIdx == 0)
        {
            return;
        }

        if (existIdx > 0)
        {
            _repo.CommitMessages.Move(existIdx, 0);
            return;
        }

        if (_repo.CommitMessages.Count > 9)
        {
            _repo.CommitMessages.RemoveRange(9, _repo.CommitMessages.Count - 9);
        }

        _repo.CommitMessages.Insert(0, CommitMessage);
    }

    Repository _repo;
    bool _isLoadingData;
    bool _isStaging;
    bool _isUnstaging;
    bool _isCommitting;
    bool _useAmend;
    List<Change> _unstaged;
    List<Change> _staged;
    Change _selectedUnstagedChange;
    Change _selectedStagedChange;
    List<FileTreeNode> _unstagedTree;
    List<FileTreeNode> _stagedTree;
    FileTreeNode _selectedUnstagedTreeNode;
    FileTreeNode _selectedStagedTreeNode;
    object _detailContext;
    string _commitMessage = string.Empty;
}

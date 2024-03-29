using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class DiffContext : ObservableObject
{
    public string RepositoryPath { get; } = string.Empty;

    public Change WorkingCopyChange => _option.WorkingCopyChange;

    public bool IsUnstaged => _option.IsUnstaged;

    public string FilePath => _option.Path;

    public bool IsOrgFilePathVisible => !string.IsNullOrWhiteSpace(_option.OrgPath) && _option.OrgPath != "/dev/null";

    public string OrgFilePath => _option.OrgPath;

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsTextDiff
    {
        get => _isTextDiff;
        private set => SetProperty(ref _isTextDiff, value);
    }

    public object Content
    {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public Vector SyncScrollOffset
    {
        get => _syncScrollOffset;
        set => SetProperty(ref _syncScrollOffset, value);
    }

    public DiffContext(string repo, DiffOption option, DiffContext previous = null)
    {
        RepositoryPath = repo;
        _option = option;

        if (previous != null)
        {
            _isTextDiff = previous._isTextDiff;
            _content = previous._content;
        }

        OnPropertyChanged(nameof(FilePath));
        OnPropertyChanged(nameof(IsOrgFilePathVisible));
        OnPropertyChanged(nameof(OrgFilePath));

        Task.Run(() =>
        {
            var latest = new Diff(repo, option).Result();
            var rs = null as object;

            if (latest.TextDiff != null)
            {
                latest.TextDiff.File = _option.Path;
                rs = latest.TextDiff;
            }
            else if (latest.IsBinary)
            {
                var oldPath = string.IsNullOrEmpty(_option.OrgPath) ? _option.Path : _option.OrgPath;
                var ext = Path.GetExtension(oldPath);

                if (IMG_EXTS.Contains(ext))
                {
                    var imgDiff = new ImageDiff();
                    if (option.Revisions.Count == 2)
                    {
                        imgDiff.Old = GetImageFileAsBitmap.Run(repo, option.Revisions[0], oldPath);
                        imgDiff.New = GetImageFileAsBitmap.Run(repo, option.Revisions[1], oldPath);
                    }
                    else
                    {
                        var fullPath = Path.Combine(repo, _option.Path);
                        imgDiff.Old = GetImageFileAsBitmap.Run(repo, "HEAD", oldPath);
                        imgDiff.New = File.Exists(fullPath) ? new Bitmap(fullPath) : null;
                    }

                    rs = imgDiff;
                }
                else
                {
                    var binaryDiff = new BinaryDiff();
                    if (option.Revisions.Count == 2)
                    {
                        binaryDiff.OldSize = new QueryFileSize(repo, oldPath, option.Revisions[0]).Result();
                        binaryDiff.NewSize = new QueryFileSize(repo, _option.Path, option.Revisions[1]).Result();
                    }
                    else
                    {
                        var fullPath = Path.Combine(repo, _option.Path);
                        binaryDiff.OldSize = new QueryFileSize(repo, oldPath, "HEAD").Result();
                        binaryDiff.NewSize = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
                    }

                    rs = binaryDiff;
                }
            }
            else if (latest.IsLFS)
            {
                rs = latest.LFSDiff;
            }
            else
            {
                rs = new NoOrEOLChange();
            }

            Dispatcher.UIThread.Post(() =>
            {
                Content = rs;
                IsTextDiff = latest.TextDiff != null;
                IsLoading = false;
            });
        });
    }

    public async void OpenExternalMergeTool()
    {
        var type = Preference.Instance.ExternalMergeToolType;
        var exec = Preference.Instance.ExternalMergeToolPath;

        var tool = ExternalMergeTools.Supported.Find(x => x.Type == type);
        if (tool == null || !File.Exists(exec))
        {
            App.RaiseException(RepositoryPath, "Invalid merge tool in preference setting!");
            return;
        }

        var args = tool.Type != 0 ? tool.DiffCmd : Preference.Instance.ExternalMergeToolDiffCmd;
        await Task.Run(() => MergeTool.OpenForDiff(RepositoryPath, exec, args, _option));
    }

    static readonly HashSet<string> IMG_EXTS = new()
    {
        ".ico",
        ".bmp",
        ".jpg",
        ".png",
        ".jpeg"
    };

    readonly DiffOption _option;
    bool _isLoading = true;
    bool _isTextDiff;
    object _content;
    Vector _syncScrollOffset = Vector.Zero;
}

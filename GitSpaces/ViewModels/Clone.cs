using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using GitSpaces.Commands;
using GitSpaces.Configs;

namespace GitSpaces.ViewModels;

public class Clone : Popup
{
    [Required(ErrorMessage = "Remote URL is required")]
    [CustomValidation(typeof(Clone), nameof(ValidateRemote))]
    public string Remote
    {
        get => _remote;
        set
        {
            if (SetProperty(ref _remote, value, true)) UseSSH = Models.Remote.IsSSH(value);
        }
    }

    public bool UseSSH
    {
        get => _useSSH;
        set => SetProperty(ref _useSSH, value);
    }

    public string SSHKey
    {
        get => _sshKey;
        set => SetProperty(ref _sshKey, value);
    }

    [Required(ErrorMessage = "Parent folder is required")]
    [CustomValidation(typeof(Clone), nameof(ValidateParentFolder))]
    public string ParentFolder
    {
        get => _parentFolder;
        set => SetProperty(ref _parentFolder, value, true);
    }

    public string Local
    {
        get => _local;
        set => SetProperty(ref _local, value);
    }

    public string ExtraArgs
    {
        get => _extraArgs;
        set => SetProperty(ref _extraArgs, value);
    }

    public Clone(Launcher launcher, LauncherPage page)
    {
        _launcher = launcher;
        _page = page;

        View = new OldViews.Clone
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateRemote(string remote, ValidationContext _)
    {
        if (!Models.Remote.IsValidURL(remote)) return new("Invalid remote repository URL format");
        return ValidationResult.Success;
    }

    public static ValidationResult ValidateParentFolder(string folder, ValidationContext _)
    {
        if (!Directory.Exists(folder)) return new("Given path can NOT be found");
        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        ProgressDescription = "Clone ...";

        return Task.Run(() =>
        {
            var cmd = new Commands.Clone(HostPageId, _parentFolder, _remote, _local, _useSSH ? _sshKey : "", _extraArgs, SetProgressDescription);
            if (!cmd.Exec()) return false;

            var path = _parentFolder;
            if (!string.IsNullOrEmpty(_local))
            {
                path = Path.GetFullPath(Path.Combine(path, _local));
            }
            else
            {
                var name = Path.GetFileName(_remote);
                if (name.EndsWith(".git")) name = name.Substring(0, name.Length - 4);
                path = Path.GetFullPath(Path.Combine(path, name));
            }

            if (!Directory.Exists(path))
            {
                CallUIThread(() =>
                {
                    App123.RaiseException(HostPageId, $"Folder '{path}' can NOT be found");
                });
                return false;
            }

            if (_useSSH && !string.IsNullOrEmpty(_sshKey))
            {
                var config = new Config(path);
                config.Set("remote.origin.sshkey", _sshKey);
            }

            CallUIThread(() =>
            {
                var repo = Preference.AddRepository(path, Path.Combine(path, ".git"));
                var node = new RepositoryNode
                {
                    Id = repo.FullPath, Name = Path.GetFileName(repo.FullPath), Bookmark = 0, IsRepository = true
                };
                Preference.AddNode(node);

                _launcher.OpenRepositoryInTab(node, _page);
            });

            return true;
        });
    }

    readonly Launcher _launcher;
    readonly LauncherPage _page;
    string _remote = string.Empty;
    bool _useSSH;
    string _sshKey = string.Empty;
    string _parentFolder = Preference.Instance.GitDefaultCloneDir;
    string _local = string.Empty;
    string _extraArgs = string.Empty;
}

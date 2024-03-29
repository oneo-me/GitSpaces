using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitSpaces.Commands;
using Remote = GitSpaces.Models.Remote;

namespace GitSpaces.ViewModels;

public class EditRemote : Popup
{
    [Required(ErrorMessage = "Remote name is required!!!")]
    [RegularExpression(@"^[\w\-\.]+$", ErrorMessage = "Bad remote name format!!!")]
    [CustomValidation(typeof(EditRemote), nameof(ValidateRemoteName))]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    [Required(ErrorMessage = "Remote URL is required!!!")]
    [CustomValidation(typeof(EditRemote), nameof(ValidateRemoteURL))]
    public string Url
    {
        get => _url;
        set
        {
            if (SetProperty(ref _url, value, true)) UseSSH = Remote.IsSSH(value);
        }
    }

    public bool UseSSH
    {
        get => _useSSH;
        set => SetProperty(ref _useSSH, value);
    }

    public string SSHKey { get; set; }

    public EditRemote(Repository repo, Remote remote)
    {
        _repo = repo;
        _remote = remote;
        _name = remote.Name;
        _url = remote.URL;
        _useSSH = Remote.IsSSH(remote.URL);

        if (_useSSH)
        {
            SSHKey = new Config(repo.FullPath).Get($"remote.{remote.Name}.sshkey");
        }

        View = new Views.EditRemote
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateRemoteName(string name, ValidationContext ctx)
    {
        if (ctx.ObjectInstance is EditRemote edit)
        {
            foreach (var remote in edit._repo.Remotes)
            {
                if (remote != edit._remote && name == remote.Name) new ValidationResult("A remote with given name already exists!!!");
            }
        }

        return ValidationResult.Success;
    }

    public static ValidationResult ValidateRemoteURL(string url, ValidationContext ctx)
    {
        if (ctx.ObjectInstance is EditRemote edit)
        {
            if (!Remote.IsValidURL(url)) return new("Bad remote URL format!!!");

            foreach (var remote in edit._repo.Remotes)
            {
                if (remote != edit._remote && url == remote.URL) new ValidationResult("A remote with the same url already exists!!!");
            }
        }

        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Editing remote '{_remote.Name}' ...";

        return Task.Run(() =>
        {
            if (_remote.Name != _name)
            {
                var succ = new Commands.Remote(_repo.FullPath).Rename(_remote.Name, _name);
                if (succ) _remote.Name = _name;
            }

            if (_remote.URL != _url)
            {
                var succ = new Commands.Remote(_repo.FullPath).SetURL(_name, _url);
                if (succ) _remote.URL = _url;
            }

            if (_useSSH)
            {
                SetProgressDescription("Post processing ...");
                new Config(_repo.FullPath).Set($"remote.{_name}.sshkey", SSHKey);
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
    readonly Remote _remote;
    string _name = string.Empty;
    string _url = string.Empty;
    bool _useSSH;
}

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using GitSpaces.Commands;
using Remote = GitSpaces.Models.Remote;

namespace GitSpaces.ViewModels;

public class AddSubmodule : Popup
{
    [Required(ErrorMessage = "Url is required!!!")]
    [CustomValidation(typeof(AddSubmodule), nameof(ValidateURL))]
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value, true);
    }

    [Required(ErrorMessage = "Reletive path is required!!!")]
    [CustomValidation(typeof(AddSubmodule), nameof(ValidateRelativePath))]
    public string RelativePath
    {
        get => _relativePath;
        set => SetProperty(ref _relativePath, value, true);
    }

    public bool Recursive { get; set; }

    public AddSubmodule(Repository repo)
    {
        _repo = repo;
        View = new Views.AddSubmodule
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateURL(string url, ValidationContext ctx)
    {
        if (!Remote.IsValidURL(url)) return new("Invalid repository URL format");
        return ValidationResult.Success;
    }

    public static ValidationResult ValidateRelativePath(string path, ValidationContext ctx)
    {
        if (Path.Exists(path))
        {
            return new("Give path is exists already!");
        }

        if (Path.IsPathRooted(path))
        {
            return new("Path must be relative to this repository!");
        }

        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Adding submodule...";

        return Task.Run(() =>
        {
            var succ = new Submodule(_repo.FullPath).Add(_url, _relativePath, Recursive, SetProgressDescription);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    string _url = string.Empty;
    string _relativePath = string.Empty;
}

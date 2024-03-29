using System.ComponentModel.DataAnnotations;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class RenameBranch : Popup
{
    public Branch Target { get; }

    [Required(ErrorMessage = "Branch name is required!!!")]
    [RegularExpression(@"^[\w\-/\.]+$", ErrorMessage = "Bad branch name format!")]
    [CustomValidation(typeof(RenameBranch), nameof(ValidateBranchName))]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    public RenameBranch(Repository repo, Branch target)
    {
        _repo = repo;
        _name = target.Name;
        Target = target;
        View = new OldViews.RenameBranch
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateBranchName(string name, ValidationContext ctx)
    {
        if (ctx.ObjectInstance is RenameBranch rename)
        {
            foreach (var b in rename._repo.Branches)
            {
                if (b != rename.Target && b.Name == name)
                {
                    return new("A branch with same name already exists!!!");
                }
            }
        }

        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        if (_name == Target.Name) return null;

        _repo.SetWatcherEnabled(false);
        ProgressDescription = $"Rename '{Target.Name}'";

        return Task.Run(() =>
        {
            var succ = Commands.Branch.Rename(_repo.FullPath, Target.Name, _name);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    string _name = string.Empty;
}

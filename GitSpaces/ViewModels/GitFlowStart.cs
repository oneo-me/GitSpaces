using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitSpaces.Models;
using GitFlow = GitSpaces.Commands.GitFlow;

namespace GitSpaces.ViewModels;

public class GitFlowStart : Popup
{
    [Required(ErrorMessage = "Name is required!!!")]
    [RegularExpression(@"^[\w\-/\.]+$", ErrorMessage = "Bad branch name format!")]
    [CustomValidation(typeof(GitFlowStart), nameof(ValidateBranchName))]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    public string Prefix { get; } = string.Empty;

    public bool IsFeature => _type == GitFlowBranchType.Feature;
    public bool IsRelease => _type == GitFlowBranchType.Release;
    public bool IsHotfix => _type == GitFlowBranchType.Hotfix;

    public GitFlowStart(Repository repo, GitFlowBranchType type)
    {
        _repo = repo;
        _type = type;

        switch (type)
        {
            case GitFlowBranchType.Feature:
                Prefix = repo.GitFlow.Feature;
                break;

            case GitFlowBranchType.Release:
                Prefix = repo.GitFlow.Release;
                break;

            default:
                Prefix = repo.GitFlow.Hotfix;
                break;
        }

        View = new OldViews.GitFlowStart
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateBranchName(string name, ValidationContext ctx)
    {
        if (ctx.ObjectInstance is GitFlowStart starter)
        {
            var check = $"{starter.Prefix}{name}";
            foreach (var b in starter._repo.Branches)
            {
                var test = b.IsLocal ? b.Name : $"{b.Remote}/{b.Name}";
                if (test == check) return new("A branch with same name already exists!");
            }
        }

        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        return Task.Run(() =>
        {
            var succ = new GitFlow(_repo.FullPath).Start(_type, _name);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    readonly GitFlowBranchType _type = GitFlowBranchType.Feature;
    string _name;
}

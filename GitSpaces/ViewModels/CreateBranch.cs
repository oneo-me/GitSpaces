using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitSpaces.Commands;
using Branch = GitSpaces.Models.Branch;
using Commit = GitSpaces.Models.Commit;
using Tag = GitSpaces.Models.Tag;

namespace GitSpaces.ViewModels;

public class CreateBranch : Popup
{
    [Required(ErrorMessage = "Branch name is required!")]
    [RegularExpression(@"^[\w\-/\.]+$", ErrorMessage = "Bad branch name format!")]
    [CustomValidation(typeof(CreateBranch), nameof(ValidateBranchName))]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    public object BasedOn { get; private set; }

    public bool CheckoutAfterCreated { get; set; } = true;

    public bool AutoStash { get; set; } = true;

    public CreateBranch(Repository repo, Branch branch)
    {
        _repo = repo;
        _baseOnRevision = branch.FullName;

        if (!branch.IsLocal && repo.Branches.Find(x => x.IsLocal && x.Name == branch.Name) == null)
        {
            Name = branch.Name;
        }

        BasedOn = branch;
        View = new OldViews.CreateBranch
        {
            DataContext = this
        };
    }

    public CreateBranch(Repository repo, Commit commit)
    {
        _repo = repo;
        _baseOnRevision = commit.SHA;

        BasedOn = commit;
        View = new OldViews.CreateBranch
        {
            DataContext = this
        };
    }

    public CreateBranch(Repository repo, Tag tag)
    {
        _repo = repo;
        _baseOnRevision = tag.SHA;

        BasedOn = tag;
        View = new OldViews.CreateBranch
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateBranchName(string name, ValidationContext ctx)
    {
        var creator = ctx.ObjectInstance as CreateBranch;
        if (creator == null) return new("Missing runtime context to create branch!");

        foreach (var b in creator._repo.Branches)
        {
            var test = b.IsLocal ? b.Name : $"{b.Remote}/{b.Name}";
            if (test == name) return new("A branch with same name already exists!");
        }

        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        return Task.Run(() =>
        {
            if (CheckoutAfterCreated)
            {
                var needPopStash = false;
                if (_repo.WorkingCopyChangesCount > 0)
                {
                    if (AutoStash)
                    {
                        SetProgressDescription("Adding untracked changes...");
                        var succ = new Add(_repo.FullPath).Exec();
                        if (succ)
                        {
                            SetProgressDescription("Stash local changes");
                            succ = new Stash(_repo.FullPath).Push("CREATE_BRANCH_AUTO_STASH");
                        }

                        if (!succ)
                        {
                            CallUIThread(() => _repo.SetWatcherEnabled(true));
                            return false;
                        }

                        needPopStash = true;
                    }
                    else
                    {
                        SetProgressDescription("Discard local changes...");
                        Commands.Discard.All(_repo.FullPath);
                    }
                }

                SetProgressDescription($"Create new branch '{_name}'");
                new Commands.Checkout(_repo.FullPath).Branch(_name, _baseOnRevision, SetProgressDescription);

                if (needPopStash)
                {
                    SetProgressDescription("Re-apply local changes...");
                    new Stash(_repo.FullPath).Pop("stash@{0}");
                }
            }
            else
            {
                Commands.Branch.Create(_repo.FullPath, _name, _baseOnRevision);
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
    string _name;
    readonly string _baseOnRevision;
}

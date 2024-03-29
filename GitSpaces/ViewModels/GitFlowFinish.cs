using System.Threading.Tasks;
using GitSpaces.Models;
using GitFlow = GitSpaces.Commands.GitFlow;

namespace GitSpaces.ViewModels;

public class GitFlowFinish : Popup
{
    public Branch Branch { get; }

    public bool IsFeature => _type == GitFlowBranchType.Feature;
    public bool IsRelease => _type == GitFlowBranchType.Release;
    public bool IsHotfix => _type == GitFlowBranchType.Hotfix;

    public bool KeepBranch { get; set; } = false;

    public GitFlowFinish(Repository repo, Branch branch, GitFlowBranchType type)
    {
        _repo = repo;
        Branch = branch;
        _type = type;
        View = new Views.GitFlowFinish
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        return Task.Run(() =>
        {
            var branch = Branch.Name;
            switch (_type)
            {
                case GitFlowBranchType.Feature:
                    branch = branch.Substring(_repo.GitFlow.Feature.Length);
                    break;

                case GitFlowBranchType.Release:
                    branch = branch.Substring(_repo.GitFlow.Release.Length);
                    break;

                default:
                    branch = branch.Substring(_repo.GitFlow.Hotfix.Length);
                    break;
            }

            var succ = new GitFlow(_repo.FullPath).Finish(_type, branch, KeepBranch);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    readonly GitFlowBranchType _type = GitFlowBranchType.None;
}

using System.Text;

namespace GitSpaces.Models;

public class DiffOption
{
    public Change WorkingCopyChange { get; }

    public bool IsUnstaged { get; }

    public List<string> Revisions { get; } = new();

    public string Path { get; } = string.Empty;

    public string OrgPath { get; } = string.Empty;

    /// <summary>
    ///     Only used for working copy changes
    /// </summary>
    /// <param name="change"></param>
    /// <param name="isUnstaged"></param>
    public DiffOption(Change change, bool isUnstaged)
    {
        WorkingCopyChange = change;
        IsUnstaged = isUnstaged;

        if (isUnstaged)
        {
            switch (change.WorkTree)
            {
                case ChangeState.Added:
                case ChangeState.Untracked:
                    _extra = "--no-index";
                    Path = change.Path;
                    OrgPath = "/dev/null";
                    break;

                default:
                    Path = change.Path;
                    OrgPath = change.OriginalPath;
                    break;
            }
        }
        else
        {
            _extra = "--cached";
            Path = change.Path;
            OrgPath = change.OriginalPath;
        }
    }

    /// <summary>
    ///     Only used for commit changes.
    /// </summary>
    /// <param name="commit"></param>
    /// <param name="change"></param>
    public DiffOption(Commit commit, Change change)
    {
        var baseRevision = commit.Parents.Count == 0 ? "4b825dc642cb6eb9a060e54bf8d69288fbee4904" : $"{commit.SHA}^";
        Revisions.Add(baseRevision);
        Revisions.Add(commit.SHA);
        Path = change.Path;
        OrgPath = change.OriginalPath;
    }

    /// <summary>
    ///     Diff with filepath. Used by FileHistories
    /// </summary>
    /// <param name="commit"></param>
    /// <param name="file"></param>
    public DiffOption(Commit commit, string file)
    {
        var baseRevision = commit.Parents.Count == 0 ? "4b825dc642cb6eb9a060e54bf8d69288fbee4904" : $"{commit.SHA}^";
        Revisions.Add(baseRevision);
        Revisions.Add(commit.SHA);
        Path = file;
    }

    /// <summary>
    ///     Used to show differences between two revisions.
    /// </summary>
    /// <param name="baseRevision"></param>
    /// <param name="targetRevision"></param>
    /// <param name="change"></param>
    public DiffOption(string baseRevision, string targetRevision, Change change)
    {
        Revisions.Add(baseRevision);
        Revisions.Add(targetRevision);
        Path = change.Path;
        OrgPath = change.OriginalPath;
    }

    /// <summary>
    ///     Converts to diff command arguments.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(_extra)) builder.Append($"{_extra} ");
        foreach (var r in Revisions) builder.Append($"{r} ");

        builder.Append("-- ");
        if (!string.IsNullOrEmpty(OrgPath)) builder.Append($"\"{OrgPath}\" ");
        builder.Append($"\"{Path}\"");

        return builder.ToString();
    }

    readonly string _extra = string.Empty;
}

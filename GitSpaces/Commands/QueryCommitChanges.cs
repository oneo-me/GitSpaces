using System.Text.RegularExpressions;
using GitSpaces.Models;

namespace GitSpaces.Commands;

public partial class QueryCommitChanges : Command
{
    [GeneratedRegex(@"^(\s?[\w\?]{1,4})\s+(.+)$")]
    private static partial Regex REG_FORMAT();

    public QueryCommitChanges(string repo, string commitSHA)
    {
        WorkingDirectory = repo;
        Context = repo;
        Args = $"show --name-status {commitSHA}";
    }

    public List<Change> Result()
    {
        Exec();
        _changes.Sort((l, r) => l.Path.CompareTo(r.Path));
        return _changes;
    }

    protected override void OnReadline(string line)
    {
        var match = REG_FORMAT().Match(line);
        if (!match.Success) return;

        var change = new Change
        {
            Path = match.Groups[2].Value
        };
        var status = match.Groups[1].Value;

        switch (status[0])
        {
            case 'M':
                change.Set(ChangeState.Modified);
                _changes.Add(change);
                break;

            case 'A':
                change.Set(ChangeState.Added);
                _changes.Add(change);
                break;

            case 'D':
                change.Set(ChangeState.Deleted);
                _changes.Add(change);
                break;

            case 'R':
                change.Set(ChangeState.Renamed);
                _changes.Add(change);
                break;

            case 'C':
                change.Set(ChangeState.Copied);
                _changes.Add(change);
                break;
        }
    }

    readonly List<Change> _changes = new();
}

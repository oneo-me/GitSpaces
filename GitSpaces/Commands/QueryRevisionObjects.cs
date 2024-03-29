using System.Collections.Generic;
using System.Text.RegularExpressions;
using GitSpaces.Models;
using Object = GitSpaces.Models.Object;

namespace GitSpaces.Commands;

public partial class QueryRevisionObjects : Command
{
    [GeneratedRegex(@"^\d+\s+(\w+)\s+([0-9a-f]+)\s+(.*)$")]
    private static partial Regex REG_FORMAT();

    readonly List<Object> objects = new();

    public QueryRevisionObjects(string repo, string sha)
    {
        WorkingDirectory = repo;
        Context = repo;
        Args = $"ls-tree -r {sha}";
    }

    public List<Object> Result()
    {
        Exec();
        return objects;
    }

    protected override void OnReadline(string line)
    {
        var match = REG_FORMAT().Match(line);
        if (!match.Success) return;

        var obj = new Object();
        obj.SHA = match.Groups[2].Value;
        obj.Type = ObjectType.Blob;
        obj.Path = match.Groups[3].Value;

        switch (match.Groups[1].Value)
        {
            case "blob":
                obj.Type = ObjectType.Blob;
                break;

            case "tree":
                obj.Type = ObjectType.Tree;
                break;

            case "tag":
                obj.Type = ObjectType.Tag;
                break;

            case "commit":
                obj.Type = ObjectType.Commit;
                break;
        }

        objects.Add(obj);
    }
}

﻿namespace GitSpaces.Commands;

public class QueryTags : Command
{
    public QueryTags(string repo)
    {
        Context = repo;
        WorkingDirectory = repo;
        Args = "for-each-ref --sort=-creatordate --format=\"$%(refname:short)$%(objectname)$%(*objectname)\" refs/tags";
    }

    public List<Models.Tag> Result()
    {
        Exec();
        return _loaded;
    }

    protected override void OnReadline(string line)
    {
        var subs = line.Split(new[]
        {
            '$'
        }, StringSplitOptions.RemoveEmptyEntries);
        if (subs.Length == 2)
        {
            _loaded.Add(new()
            {
                Name = subs[0], SHA = subs[1]
            });
        }
        else if (subs.Length == 3)
        {
            _loaded.Add(new()
            {
                Name = subs[0], SHA = subs[2]
            });
        }
    }

    readonly List<Models.Tag> _loaded = new();
}

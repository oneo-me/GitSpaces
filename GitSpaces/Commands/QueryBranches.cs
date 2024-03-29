﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GitSpaces.Commands;

public partial class QueryBranches : Command
{
    static readonly string PREFIX_LOCAL = "refs/heads/";
    static readonly string PREFIX_REMOTE = "refs/remotes/";

    [GeneratedRegex(@"^(\d+)\s(\d+)$")]
    private static partial Regex REG_AHEAD_BEHIND();

    public QueryBranches(string repo)
    {
        WorkingDirectory = repo;
        Context = repo;
        Args = "branch -l --all -v --format=\"%(refname)$%(objectname)$%(HEAD)$%(upstream)$%(upstream:trackshort)\"";
    }

    public List<Models.Branch> Result()
    {
        Exec();

        foreach (var b in _branches)
        {
            if (b.IsLocal && !string.IsNullOrEmpty(b.UpstreamTrackStatus))
            {
                if (b.UpstreamTrackStatus == "=")
                {
                    b.UpstreamTrackStatus = string.Empty;
                }
                else
                {
                    b.UpstreamTrackStatus = ParseTrackStatus(b.Name, b.Upstream);
                }
            }
        }

        return _branches;
    }

    protected override void OnReadline(string line)
    {
        var parts = line.Split('$');
        if (parts.Length != 5) return;

        var branch = new Models.Branch();
        var refName = parts[0];
        if (refName.EndsWith("/HEAD", StringComparison.Ordinal)) return;

        if (refName.StartsWith(PREFIX_LOCAL, StringComparison.Ordinal))
        {
            branch.Name = refName.Substring(PREFIX_LOCAL.Length);
            branch.IsLocal = true;
        }
        else if (refName.StartsWith(PREFIX_REMOTE, StringComparison.Ordinal))
        {
            var name = refName.Substring(PREFIX_REMOTE.Length);
            var shortNameIdx = name.IndexOf('/', StringComparison.Ordinal);
            if (shortNameIdx < 0) return;

            branch.Remote = name.Substring(0, shortNameIdx);
            branch.Name = name.Substring(branch.Remote.Length + 1);
            branch.IsLocal = false;
        }
        else
        {
            branch.Name = refName;
            branch.IsLocal = true;
        }

        branch.FullName = refName;
        branch.Head = parts[1];
        branch.IsCurrent = parts[2] == "*";
        branch.Upstream = parts[3];
        branch.UpstreamTrackStatus = parts[4];
        _branches.Add(branch);
    }

    string ParseTrackStatus(string local, string upstream)
    {
        var cmd = new Command();
        cmd.WorkingDirectory = WorkingDirectory;
        cmd.Context = Context;
        cmd.Args = $"rev-list --left-right --count {local}...{upstream}";

        var rs = cmd.ReadToEnd();
        if (!rs.IsSuccess) return string.Empty;

        var match = REG_AHEAD_BEHIND().Match(rs.StdOut);
        if (!match.Success) return string.Empty;

        var ahead = int.Parse(match.Groups[1].Value);
        var behind = int.Parse(match.Groups[2].Value);
        var track = "";
        if (ahead > 0) track += $"{ahead}↑";
        if (behind > 0) track += $" {behind}↓";
        return track.Trim();
    }

    readonly List<Models.Branch> _branches = new();
}

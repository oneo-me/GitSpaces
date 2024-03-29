using System;
using System.Collections.Generic;
using GitSpaces.Models;

namespace GitSpaces.Commands;

public class QueryCommits : Command
{
    static readonly string GPGSIG_START = "gpgsig -----BEGIN PGP SIGNATURE-----";
    static readonly string GPGSIG_END = " -----END PGP SIGNATURE-----";

    readonly List<Models.Commit> commits = new();
    Models.Commit current;
    bool isSkipingGpgsig;
    bool isHeadFounded;
    readonly bool findFirstMerged = true;

    public QueryCommits(string repo, string limits, bool needFindHead = true)
    {
        WorkingDirectory = repo;
        Args = "log --date-order --decorate=full --pretty=raw " + limits;
        findFirstMerged = needFindHead;
    }

    public List<Models.Commit> Result()
    {
        Exec();

        if (current != null)
        {
            current.Message = current.Message.Trim();
            commits.Add(current);
        }

        if (findFirstMerged && !isHeadFounded && commits.Count > 0)
        {
            MarkFirstMerged();
        }

        return commits;
    }

    protected override void OnReadline(string line)
    {
        if (isSkipingGpgsig)
        {
            if (line.StartsWith(GPGSIG_END, StringComparison.Ordinal)) isSkipingGpgsig = false;
            return;
        }

        if (line.StartsWith(GPGSIG_START, StringComparison.Ordinal))
        {
            isSkipingGpgsig = true;
            return;
        }

        if (line.StartsWith("commit ", StringComparison.Ordinal))
        {
            if (current != null)
            {
                current.Message = current.Message.Trim();
                commits.Add(current);
            }

            current = new();
            line = line.Substring(7);

            var decoratorStart = line.IndexOf('(', StringComparison.Ordinal);
            if (decoratorStart < 0)
            {
                current.SHA = line.Trim();
            }
            else
            {
                current.SHA = line.Substring(0, decoratorStart).Trim();
                current.IsMerged = ParseDecorators(current.Decorators, line.Substring(decoratorStart + 1));
                if (!isHeadFounded) isHeadFounded = current.IsMerged;
            }

            return;
        }

        if (current == null) return;

        if (line.StartsWith("tree ", StringComparison.Ordinal))
        {
            return;
        }

        if (line.StartsWith("parent ", StringComparison.Ordinal))
        {
            current.Parents.Add(line.Substring("parent ".Length));
        }
        else if (line.StartsWith("author ", StringComparison.Ordinal))
        {
            var user = User.Invalid;
            ulong time = 0;
            Models.Commit.ParseUserAndTime(line.Substring(7), ref user, ref time);
            current.Author = user;
            current.AuthorTime = time;
        }
        else if (line.StartsWith("committer ", StringComparison.Ordinal))
        {
            var user = User.Invalid;
            ulong time = 0;
            Models.Commit.ParseUserAndTime(line.Substring(10), ref user, ref time);
            current.Committer = user;
            current.CommitterTime = time;
        }
        else if (string.IsNullOrEmpty(current.Subject))
        {
            current.Subject = line.Trim();
        }
        else
        {
            current.Message += line.Trim() + "\n";
        }
    }

    bool ParseDecorators(List<Decorator> decorators, string data)
    {
        var isHeadOfCurrent = false;

        var subs = data.Split(new[]
        {
            ',',
            ')',
            '('
        }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var sub in subs)
        {
            var d = sub.Trim();
            if (d.StartsWith("tag: refs/tags/", StringComparison.Ordinal))
            {
                decorators.Add(new()
                {
                    Type = DecoratorType.Tag, Name = d.Substring(15).Trim()
                });
            }
            else if (d.EndsWith("/HEAD", StringComparison.Ordinal))
            {
            }
            else if (d.StartsWith("HEAD -> refs/heads/", StringComparison.Ordinal))
            {
                isHeadOfCurrent = true;
                decorators.Add(new()
                {
                    Type = DecoratorType.CurrentBranchHead, Name = d.Substring(19).Trim()
                });
            }
            else if (d.StartsWith("refs/heads/", StringComparison.Ordinal))
            {
                decorators.Add(new()
                {
                    Type = DecoratorType.LocalBranchHead, Name = d.Substring(11).Trim()
                });
            }
            else if (d.StartsWith("refs/remotes/", StringComparison.Ordinal))
            {
                decorators.Add(new()
                {
                    Type = DecoratorType.RemoteBranchHead, Name = d.Substring(13).Trim()
                });
            }
        }

        decorators.Sort((l, r) =>
        {
            if (l.Type != r.Type)
            {
                return (int)l.Type - (int)r.Type;
            }

            return l.Name.CompareTo(r.Name);
        });

        return isHeadOfCurrent;
    }

    void MarkFirstMerged()
    {
        Args = $"log --since=\"{commits[commits.Count - 1].CommitterTimeStr}\" --format=\"%H\"";

        var rs = ReadToEnd();
        var shas = rs.StdOut.Split(new[]
        {
            '\n'
        }, StringSplitOptions.RemoveEmptyEntries);
        if (shas.Length == 0) return;

        var set = new HashSet<string>();
        foreach (var sha in shas) set.Add(sha);

        foreach (var c in commits)
        {
            if (set.Contains(c.SHA))
            {
                c.IsMerged = true;
                break;
            }
        }
    }
}

﻿using System.Text;
using GitSpaces.Models;

namespace GitSpaces.Commands;

public class Add : Command
{
    public Add(string repo, List<Change> changes = null)
    {
        WorkingDirectory = repo;
        Context = repo;

        if (changes == null || changes.Count == 0)
        {
            Args = "add .";
        }
        else
        {
            var builder = new StringBuilder();
            builder.Append("add --");
            foreach (var c in changes)
            {
                builder.Append(" \"");
                builder.Append(c.Path);
                builder.Append("\"");
            }

            Args = builder.ToString();
        }
    }
}

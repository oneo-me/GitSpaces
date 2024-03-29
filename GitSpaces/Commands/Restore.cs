using System.Collections.Generic;
using System.Text;

namespace GitSpaces.Commands;

public class Restore : Command
{
    public Restore(string repo, List<string> files, string extra)
    {
        WorkingDirectory = repo;
        Context = repo;

        var builder = new StringBuilder();
        builder.Append("restore ");
        if (!string.IsNullOrEmpty(extra)) builder.Append(extra).Append(" ");
        builder.Append("--");
        foreach (var f in files) builder.Append(' ').Append('"').Append(f).Append('"');
        Args = builder.ToString();
    }
}

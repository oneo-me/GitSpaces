using System.Text;

namespace GitSpaces.Commands;

public class Clean : Command
{
    public Clean(string repo)
    {
        WorkingDirectory = repo;
        Context = repo;
        Args = "clean -qfd";
    }

    public Clean(string repo, List<string> files)
    {
        var builder = new StringBuilder();
        builder.Append("clean -qfd --");
        foreach (var f in files)
        {
            builder.Append(" \"");
            builder.Append(f);
            builder.Append("\"");
        }

        WorkingDirectory = repo;
        Context = repo;
        Args = builder.ToString();
    }
}

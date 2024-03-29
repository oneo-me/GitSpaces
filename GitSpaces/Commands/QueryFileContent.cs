using System.Text;

namespace GitSpaces.Commands;

public class QueryFileContent : Command
{
    public QueryFileContent(string repo, string revision, string file)
    {
        WorkingDirectory = repo;
        Context = repo;
        Args = $"show {revision}:\"{file}\"";
    }

    public string Result()
    {
        Exec();
        return _builder.ToString();
    }

    protected override void OnReadline(string line)
    {
        _builder.Append(line);
        _builder.Append('\n');
    }

    readonly StringBuilder _builder = new();
}

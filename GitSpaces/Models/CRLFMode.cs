namespace GitSpaces.Models;

public class CRLFMode
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Desc { get; set; }

    public static List<CRLFMode> Supported = new()
    {
        new("TRUE", "true", "Commit as LF, checkout as CRLF"), new("INPUT", "input", "Only convert for commit"), new("FALSE", "false", "Do NOT convert")
    };

    public CRLFMode(string name, string value, string desc)
    {
        Name = name;
        Value = value;
        Desc = desc;
    }
}

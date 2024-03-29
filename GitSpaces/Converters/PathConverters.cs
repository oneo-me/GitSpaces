using System.IO;
using Avalonia.Data.Converters;

namespace GitSpaces.Converters;

public static class PathConverters
{
    public static FuncValueConverter<string, string> PureFileName = new(fullpath => Path.GetFileName(fullpath) ?? "");

    public static FuncValueConverter<string, string> PureDirectoryName = new(fullpath => Path.GetDirectoryName(fullpath) ?? "");

    public static FuncValueConverter<string, string> TruncateIfTooLong =
        new(fullpath =>
        {
            if (fullpath.Length <= 50) return fullpath;
            return fullpath.Substring(0, 20) + ".../" + Path.GetFileName(fullpath);
        });
}

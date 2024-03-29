using Avalonia.Data.Converters;
using GitSpaces.Models;

namespace GitSpaces.Converters;

public static class BranchConverters
{
    public static FuncValueConverter<Branch, string> ToName = new(v => v.IsLocal ? v.Name : $"{v.Remote}/{v.Name}");
}

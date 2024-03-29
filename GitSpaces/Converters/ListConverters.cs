using System.Collections;
using Avalonia.Data.Converters;

namespace GitSpaces.Converters;

public static class ListConverters
{
    public static FuncValueConverter<IList, string> ToCount = new(v => $" ({v.Count})");

    public static FuncValueConverter<IList, bool> IsNotNullOrEmpty = new(v => v != null && v.Count > 0);
}

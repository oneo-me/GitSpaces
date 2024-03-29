using Avalonia.Data.Converters;

namespace GitSpaces.Converters;

public static class BoolConverters
{
    public static FuncValueConverter<bool, double> ToCommitOpacity = new(x => x ? 1 : 0.5);
}

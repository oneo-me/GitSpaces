using Avalonia.Data.Converters;

namespace GitSpaces.Converters;

public static class IntConverters
{
    public static FuncValueConverter<int, bool> IsGreaterThanZero = new(v => v > 0);

    public static FuncValueConverter<int, bool> IsZero = new(v => v == 0);

    public static FuncValueConverter<int, bool> IsOne = new(v => v == 1);
}

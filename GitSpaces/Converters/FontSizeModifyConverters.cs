using Avalonia.Data.Converters;

namespace GitSpaces.Converters;

public static class FontSizeModifyConverters
{
    public static FuncValueConverter<double, double> Increase = new(v => v + 1.0);

    public static FuncValueConverter<double, double> Decrease = new(v => v - 1.0);
}

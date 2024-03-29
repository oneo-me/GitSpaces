using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.Converters;

public static class ChangeViewModeConverters
{
    public static FuncValueConverter<ChangeViewMode, StreamGeometry> ToIcon =
        new(v =>
        {
            switch (v)
            {
                case ChangeViewMode.List:
                    return Application.Current?.FindResource("Icons.List") as StreamGeometry;

                case ChangeViewMode.Grid:
                    return Application.Current?.FindResource("Icons.Grid") as StreamGeometry;

                default:
                    return Application.Current?.FindResource("Icons.Tree") as StreamGeometry;
            }
        });

    public static FuncValueConverter<ChangeViewMode, bool> IsList = new(v => v == ChangeViewMode.List);

    public static FuncValueConverter<ChangeViewMode, bool> IsGrid = new(v => v == ChangeViewMode.Grid);

    public static FuncValueConverter<ChangeViewMode, bool> IsTree = new(v => v == ChangeViewMode.Tree);
}

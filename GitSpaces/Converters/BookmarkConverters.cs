using Avalonia.Data.Converters;
using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.Converters;

public static class BookmarkConverters
{
    public static FuncValueConverter<int, IBrush> ToBrush = new(bookmark => Bookmarks.Brushes[bookmark]);

    public static FuncValueConverter<int, double> ToStrokeThickness = new(bookmark => bookmark == 0 ? 1.0 : 0);
}

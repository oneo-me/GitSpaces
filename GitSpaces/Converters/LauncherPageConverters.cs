using Avalonia.Collections;
using Avalonia.Data.Converters;
using GitSpaces.ViewModels;

namespace GitSpaces.Converters;

public static class LauncherPageConverters
{
    public static FuncMultiValueConverter<object, bool> ToTabSeperatorVisible =
        new(v =>
        {
            if (v == null) return false;

            var array = new List<object>();
            array.AddRange(v);
            if (array.Count != 3) return false;

            var self = array[0] as LauncherPage;
            if (self == null) return false;

            var selected = array[1] as LauncherPage;
            var collections = array[2] as AvaloniaList<LauncherPage>;

            if (selected != null && collections != null && (self == selected || collections.IndexOf(self) + 1 == collections.IndexOf(selected)))
            {
                return false;
            }

            return true;
        });
}

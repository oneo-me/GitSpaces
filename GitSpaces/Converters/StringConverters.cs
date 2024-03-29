using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using GitSpaces.Models;

namespace GitSpaces.Converters;

public static class StringConverters
{
    public class ToLocaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Locale.Supported.Find(x => x.Key == value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Locale).Key;
        }
    }

    public static ToLocaleConverter ToLocale = new();

    public class ToThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var theme = (string)value;
            if (theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeVariant.Light;
            }

            if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeVariant.Dark;
            }

            return ThemeVariant.Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var theme = (ThemeVariant)value;
            return theme.Key;
        }
    }

    public static ToThemeConverter ToTheme = new();

    public class FormatByResourceKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = parameter as string;
            return App123.Text(key, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static FormatByResourceKeyConverter FormatByResourceKey = new();

    public static FuncValueConverter<string, string> ToShortSHA = new(v => v.Length > 10 ? v.Substring(0, 10) : v);
}

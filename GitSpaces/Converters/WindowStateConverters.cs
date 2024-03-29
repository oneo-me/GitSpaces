using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GitSpaces.Converters;

public static class WindowStateConverters
{
    public static FuncValueConverter<WindowState, Thickness> ToContentMargin =
        new(state =>
        {
            if (OperatingSystem.IsWindows() && state == WindowState.Maximized)
            {
                return new(6);
            }

            if (OperatingSystem.IsLinux() && state != WindowState.Maximized)
            {
                return new(6);
            }

            return new(0);
        });

    public static FuncValueConverter<WindowState, GridLength> ToTitleBarHeight =
        new(state =>
        {
            if (state == WindowState.Maximized)
            {
                return new(30);
            }

            return new(38);
        });

    public static FuncValueConverter<WindowState, StreamGeometry> ToMaxOrRestoreIcon =
        new(state =>
        {
            if (state == WindowState.Maximized)
            {
                return Application.Current?.FindResource("Icons.Window.Restore") as StreamGeometry;
            }

            return Application.Current?.FindResource("Icons.Window.Maximize") as StreamGeometry;
        });

    public static FuncValueConverter<WindowState, bool> IsNormal = new(state => state == WindowState.Normal);
}

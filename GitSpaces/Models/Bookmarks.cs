using Avalonia.Media;

namespace GitSpaces.Models;

public static class Bookmarks
{
    public static readonly IBrush[] Brushes =
    [
        Avalonia.Media.Brushes.Transparent,
        Avalonia.Media.Brushes.Red,
        Avalonia.Media.Brushes.Orange,
        Avalonia.Media.Brushes.Gold,
        Avalonia.Media.Brushes.ForestGreen,
        Avalonia.Media.Brushes.DarkCyan,
        Avalonia.Media.Brushes.DeepSkyBlue,
        Avalonia.Media.Brushes.Purple
    ];

    public static readonly List<int> Supported = new();

    static Bookmarks()
    {
        for (var i = 0; i < Brushes.Length; i++) Supported.Add(i);
    }
}

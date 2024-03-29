using Avalonia.Controls;

namespace GitSpaces.Configs;

public class WindowConfig
{
    public int? Left { get; set; }
    public int? Top { get; set; }
    public double Width { get; set; } = 1400;
    public double Height { get; set; } = 900;
    public WindowState WindowState { get; set; }
}

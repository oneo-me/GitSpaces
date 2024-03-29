using Avalonia;
using Avalonia.Controls;
using GitSpaces.Models;

namespace GitSpaces.OldViews;

public partial class ChangeViewModeSwitcher : UserControl
{
    public static readonly StyledProperty<ChangeViewMode> ViewModeProperty =
        AvaloniaProperty.Register<ChangeViewModeSwitcher, ChangeViewMode>(nameof(ViewMode));

    public ChangeViewMode ViewMode
    {
        get => GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    public ChangeViewModeSwitcher()
    {
        DataContext = this;
        InitializeComponent();
    }

    public void SwitchMode(object param)
    {
        ViewMode = (ChangeViewMode)param;
    }
}

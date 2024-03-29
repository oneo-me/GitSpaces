using Avalonia.Controls;
using Avalonia.Input;

namespace GitSpaces.OldViews;

public partial class CommitBaseInfo : UserControl
{
    public CommitBaseInfo()
    {
        InitializeComponent();
    }

    void OnParentSHAPressed(object sender, PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.CommitDetail detail)
        {
            detail.NavigateTo((sender as Control).DataContext as string);
        }

        e.Handled = true;
    }
}

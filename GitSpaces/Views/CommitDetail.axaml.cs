using Avalonia.Controls;
using Avalonia.Input;
using GitSpaces.Models;

namespace GitSpaces.Views;

public partial class CommitDetail : UserControl
{
    public CommitDetail()
    {
        InitializeComponent();
    }

    void OnChangeListDoubleTapped(object sender, TappedEventArgs e)
    {
        if (DataContext is ViewModels.CommitDetail detail)
        {
            var datagrid = sender as DataGrid;
            detail.ActivePageIndex = 1;
            detail.SelectedChange = datagrid.SelectedItem as Change;
        }

        e.Handled = true;
    }

    void OnChangeListContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (DataContext is ViewModels.CommitDetail detail)
        {
            var datagrid = sender as DataGrid;
            if (datagrid.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }

            var menu = detail.CreateChangeContextMenu(datagrid.SelectedItem as Change);
            menu.Open(datagrid);
        }

        e.Handled = true;
    }
}

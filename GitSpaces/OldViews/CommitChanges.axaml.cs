using Avalonia.Controls;
using GitSpaces.Models;
using GitSpaces.ViewModels;

namespace GitSpaces.OldViews;

public partial class CommitChanges : UserControl
{
    public CommitChanges()
    {
        InitializeComponent();
    }

    void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.IsVisible && datagrid.SelectedItem != null)
        {
            datagrid.ScrollIntoView(datagrid.SelectedItem, null);
        }

        e.Handled = true;
    }

    void OnDataGridContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.SelectedItem != null)
        {
            var detail = DataContext as ViewModels.CommitDetail;
            var menu = detail.CreateChangeContextMenu(datagrid.SelectedItem as Change);
            menu.Open(datagrid);
        }

        e.Handled = true;
    }

    void OnTreeViewContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (sender is TreeView view && view.SelectedItem != null)
        {
            var detail = DataContext as ViewModels.CommitDetail;
            var node = view.SelectedItem as FileTreeNode;
            if (node != null && !node.IsFolder)
            {
                var menu = detail.CreateChangeContextMenu(node.Backend as Change);
                menu.Open(view);
            }
        }

        e.Handled = true;
    }
}

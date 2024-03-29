using Avalonia.Controls;
using Avalonia.Input;
using GitSpaces.Models;
using GitSpaces.ViewModels;

namespace GitSpaces.OldViews;

public partial class RevisionCompare : UserControl
{
    public RevisionCompare()
    {
        InitializeComponent();
    }

    void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.IsVisible)
        {
            datagrid.ScrollIntoView(datagrid.SelectedItem, null);
        }

        e.Handled = true;
    }

    void OnDataGridContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (sender is DataGrid datagrid && datagrid.SelectedItem != null)
        {
            var compare = DataContext as ViewModels.RevisionCompare;
            var menu = compare.CreateChangeContextMenu(datagrid.SelectedItem as Change);
            menu.Open(datagrid);
        }

        e.Handled = true;
    }

    void OnTreeViewContextRequested(object sender, ContextRequestedEventArgs e)
    {
        if (sender is TreeView view && view.SelectedItem != null)
        {
            var compare = DataContext as ViewModels.RevisionCompare;
            var node = view.SelectedItem as FileTreeNode;
            if (node != null && !node.IsFolder)
            {
                var menu = compare.CreateChangeContextMenu(node.Backend as Change);
                menu.Open(view);
            }
        }

        e.Handled = true;
    }

    void OnPressedSHA(object sender, PointerPressedEventArgs e)
    {
        if (sender is TextBlock block)
        {
            var compare = DataContext as ViewModels.RevisionCompare;
            compare.NavigateTo(block.Text);
        }

        e.Handled = true;
    }
}

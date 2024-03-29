using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace GitSpaces.OldViews;

public partial class AddRemote : UserControl
{
    public AddRemote()
    {
        InitializeComponent();
    }

    async void SelectSSHKey(object sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new("SSHKey")
                {
                    Patterns = ["*.*"]
                }
            ]
        };
        var toplevel = TopLevel.GetTopLevel(this);
        var selected = await toplevel.StorageProvider.OpenFilePickerAsync(options);
        if (selected.Count == 1)
        {
            txtSSHKey.Text = selected[0].Path.LocalPath;
        }

        e.Handled = true;
    }
}

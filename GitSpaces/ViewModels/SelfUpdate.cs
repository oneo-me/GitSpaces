using CommunityToolkit.Mvvm.ComponentModel;

namespace GitSpaces.ViewModels;

public class SelfUpdate : ObservableObject
{
    public object Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    object _data;
}

using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Statistics : ObservableObject
{
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (SetProperty(ref _selectedIndex, value)) RefreshReport();
        }
    }

    public StatisticsReport SelectedReport
    {
        get => _selectedReport;
        private set => SetProperty(ref _selectedReport, value);
    }

    public Statistics(string repo)
    {
        _repo = repo;

        Task.Run(() =>
        {
            var result = new Commands.Statistics(_repo).Result();
            Dispatcher.UIThread.Invoke(() =>
            {
                _data = result;
                RefreshReport();
                IsLoading = false;
            });
        });
    }

    void RefreshReport()
    {
        if (_data == null) return;

        switch (_selectedIndex)
        {
            case 0:
                SelectedReport = _data.Year;
                break;

            case 1:
                SelectedReport = _data.Month;
                break;

            default:
                SelectedReport = _data.Week;
                break;
        }
    }

    readonly string _repo = string.Empty;
    bool _isLoading = true;
    Models.Statistics _data;
    StatisticsReport _selectedReport;
    int _selectedIndex;
}

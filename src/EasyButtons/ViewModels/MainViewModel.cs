using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyButtons.Models;
using EasyButtons.Repositories;
using EasyButtons.Helpers;

namespace EasyButtons.ViewModels;

public partial class MainViewModel(EasyButtonRepository repo) : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private ObservableCollection<EasyButton> _buttons = [];

    public bool IsEmpty => Buttons.Count == 0;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var all = await repo.GetAllAsync();
            Buttons = new ObservableCollection<EasyButton>(all);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task LaunchAsync(EasyButton button)
    {
        var ok = await LaunchHelper.TryLaunchAsync(button.Uri);
        if (!ok)
            await Shell.Current.DisplayAlertAsync("Can't open", $"No app found to handle:\n{button.Uri}", "OK");
    }

    [RelayCommand]
    private static async Task EditAsync(EasyButton button)
    {
        await Shell.Current.GoToAsync($"EditButtonPage?buttonId={button.Id}");
    }

    [RelayCommand]
    private static async Task AddAsync()
    {
        await Shell.Current.GoToAsync("EditButtonPage");
    }
}

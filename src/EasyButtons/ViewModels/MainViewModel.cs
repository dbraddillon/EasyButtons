using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyButtons.Models;
using EasyButtons.Repositories;
using EasyButtons.Services;
using EasyButtons.Helpers;

namespace EasyButtons.ViewModels;

public partial class MainViewModel(EasyButtonRepository repo, ProService pro, BackupService backup, SoundService sound) : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(IsAtFreeLimit))]
    private ObservableCollection<EasyButton> _buttons = [];

    public bool IsEmpty       => Buttons.Count == 0;
    public bool IsAtFreeLimit => !pro.IsPro && Buttons.Count >= ProService.FreeButtonLimit;

    // Called by MainPage code-behind after debug Pro toggle
    public void NotifyProChanged() => OnPropertyChanged(nameof(IsAtFreeLimit));

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
        // SoundPath must be a real existing file — ignore stale sentinel values (e.g. "click")
        if (!string.IsNullOrEmpty(button.SoundPath) && File.Exists(button.SoundPath))
        {
            sound.Play(button.SoundPath);
            return;
        }

        var ok = await LaunchHelper.TryLaunchAsync(button.Uri);
        if (!ok)
            await Shell.Current.DisplayAlertAsync("Can't open", $"No app found to handle:\n{button.Uri}", "OK");
    }

    [RelayCommand]
    private static Task EditAsync(EasyButton button)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync($"EditButtonPage?buttonId={button.Id}"));
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (IsAtFreeLimit)
        {
            await Shell.Current.DisplayAlertAsync(
                "Button Limit Reached",
                $"EasyButtons supports up to {ProService.FreeButtonLimit} buttons. Unlimited buttons and more are coming in a future Pro update.",
                "OK");
            return;
        }
        await Shell.Current.GoToAsync("EditButtonPage");
    }

    [RelayCommand]
    private async Task ExportBackupAsync()
    {
        await backup.ExportAsync();
    }

    [RelayCommand]
    private async Task ImportBackupAsync()
    {
        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Import Backup",
            "This will replace all your current buttons with the backup. Continue?",
            "Import", "Cancel");
        if (!confirmed) return;

        var count = await backup.ImportAsync();
        if (count < 0)
            await Shell.Current.DisplayAlertAsync("Error", "Could not read the backup file.", "OK");
        else if (count == 0)
            await Shell.Current.DisplayAlertAsync("Empty", "No buttons found in the backup.", "OK");
        else
        {
            await LoadAsync();
#if ANDROID
            WidgetHelper.RequestUpdate();
#endif
            await Shell.Current.DisplayAlertAsync("Done", $"{count} button{(count == 1 ? "" : "s")} restored.", "OK");
        }
    }
}

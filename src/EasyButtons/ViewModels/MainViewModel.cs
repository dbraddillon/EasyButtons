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

    [ObservableProperty]
    private ObservableCollection<ButtonGroup> _groupedButtons = [];

    public bool IsEmpty       => Buttons.Count == 0;
    public bool IsAtFreeLimit => !pro.IsPro && Buttons.Count >= ProService.FreeButtonLimit;

    // Called by MainPage code-behind after debug Pro toggle
    public void NotifyProChanged()
    {
        OnPropertyChanged(nameof(IsAtFreeLimit));
        RebuildGroups();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var all = await repo.GetAllAsync();
            Buttons = new ObservableCollection<EasyButton>(all);
            RebuildGroups();
        }
        finally { IsBusy = false; }
    }

    private void RebuildGroups()
    {
        var groups = new ObservableCollection<ButtonGroup>();

        if (pro.IsPro && Buttons.Any(b => !string.IsNullOrEmpty(b.GroupName)))
        {
            var named = Buttons
                .Where(b => !string.IsNullOrEmpty(b.GroupName))
                .GroupBy(b => b.GroupName!)
                .OrderBy(g => g.Key);
            foreach (var g in named)
                groups.Add(new ButtonGroup(g.Key, g));

            var ungrouped = Buttons.Where(b => string.IsNullOrEmpty(b.GroupName)).ToList();
            if (ungrouped.Count > 0)
                groups.Add(new ButtonGroup("Other", ungrouped));
        }
        else
        {
            groups.Add(new ButtonGroup("", Buttons));
        }

        GroupedButtons = groups;
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
            var upgrade = await Shell.Current.DisplayAlertAsync(
                "Upgrade to Pro",
                $"Free tier supports up to {ProService.FreeButtonLimit} buttons.\n\nEasyButtons Pro ($1.99 one-time) unlocks:\n• Unlimited buttons\n• All widget layouts\n• Widget customization\n• Groups & folders",
                "Upgrade — $1.99", "Not Now");

            if (upgrade)
            {
                var purchased = await pro.PurchaseAsync();
                if (purchased)
                {
                    NotifyProChanged();
                    await Shell.Current.GoToAsync("EditButtonPage");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Purchase", "Purchase could not be completed. Try again later.", "OK");
                }
            }
            return;
        }
        await Shell.Current.GoToAsync("EditButtonPage");
    }

    [RelayCommand]
    private async Task UpgradeToProAsync()
    {
        if (pro.IsPro)
        {
            await Shell.Current.DisplayAlertAsync("EasyButtons Pro", "You already have Pro unlocked. Thanks for your support!", "OK");
            return;
        }
        var purchased = await pro.PurchaseAsync();
        if (purchased)
        {
            NotifyProChanged();
            await Shell.Current.DisplayAlertAsync("Welcome to Pro!", "Unlimited buttons, all widget layouts, groups, and more are now unlocked.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Purchase", "Purchase could not be completed. Try again later.", "OK");
        }
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

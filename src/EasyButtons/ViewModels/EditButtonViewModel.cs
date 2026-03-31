using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyButtons.Helpers;
using EasyButtons.Models;
using EasyButtons.Repositories;
using EasyButtons.Services;

namespace EasyButtons.ViewModels;

[QueryProperty(nameof(ButtonId), "buttonId")]
public partial class EditButtonViewModel(EasyButtonRepository repo, ProService pro) : BaseViewModel
{
    private EasyButton? _existing;

    [ObservableProperty] private string _buttonId = string.Empty;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _uri = string.Empty;
    [ObservableProperty] private string _color = "#E53935";
    [ObservableProperty] private bool _isEdit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SoundMode))]
    [NotifyPropertyChangedFor(nameof(SoundIsOff))]
    [NotifyPropertyChangedFor(nameof(SoundIsClick))]
    [NotifyPropertyChangedFor(nameof(SoundIsCustom))]
    [NotifyPropertyChangedFor(nameof(CustomSoundName))]
    private string? _soundPath;

    // "off" | "click" | "custom"
    public string SoundMode =>
        string.IsNullOrEmpty(SoundPath) ? "off" :
        SoundPath == SoundService.ClickSentinel ? "click" : "custom";

    public bool SoundIsOff    => SoundMode == "off";
    public bool SoundIsClick  => SoundMode == "click";
    public bool SoundIsCustom => SoundMode == "custom";

    public string CustomSoundName => SoundIsCustom
        ? Path.GetFileNameWithoutExtension(SoundPath!)
        : "Choose file…";

    // Preset colors for the picker
    public List<string> PresetColors { get; } =
    [
        "#E53935", // Red (classic Easy Button)
        "#F57C00", // Orange
        "#F9A825", // Yellow
        "#43A047", // Green
        "#1E88E5", // Blue
        "#8E24AA", // Purple
        "#00897B", // Teal
        "#6D4C41", // Brown
        "#546E7A", // Blue-grey
        "#212121", // Near-black
    ];

    partial void OnButtonIdChanged(string value)
    {
        _ = LoadExistingAsync(value);
    }

    private async Task LoadExistingAsync(string idStr)
    {
        if (!Guid.TryParse(idStr, out var id)) return;
        _existing = await repo.GetByIdAsync(id);
        if (_existing is null) return;
        Label = _existing.Label;
        Uri = _existing.Uri;
        Color = _existing.Color;
        SoundPath = _existing.SoundPath;
        IsEdit = true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            await Shell.Current.DisplayAlertAsync("Required", "Enter a label.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(Uri))
        {
            await Shell.Current.DisplayAlertAsync("Required", "Enter a URL or URI.", "OK");
            return;
        }

        var button = _existing ?? new EasyButton();
        button.Label = Label.Trim();
        button.Uri = Uri.Trim();
        button.Color = Color;
        button.SoundPath = SoundPath;

        await repo.SaveAsync(button);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_existing is null) return;
        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Delete", $"Delete \"{_existing.Label}\"?", "Delete", "Cancel");
        if (!confirmed) return;
        await repo.DeleteAsync(_existing.Id);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void SelectColor(string color) => Color = color;

    [RelayCommand]
    private async Task SetSoundModeAsync(string mode)
    {
        switch (mode)
        {
            case "off":
                DeleteCustomFile();
                SoundPath = null;
                break;

            case "click":
                DeleteCustomFile();
                SoundPath = SoundService.ClickSentinel; // free, no Pro gate
                break;

            case "custom":
                if (!pro.IsPro)
                {
                    var upgrade = await Shell.Current.DisplayAlertAsync(
                        "Pro Feature",
                        "Custom audio files are available with EasyButtons Pro ($1.99 one-time).",
                        "Get Pro", "Not Now");
                    if (upgrade) await TryPurchaseAsync();
                    return;
                }
                await PickCustomFileAsync();
                break;
        }
    }

    private async Task PickCustomFileAsync()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Choose a sound",
            FileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, ["audio/*"] },
                }),
        });

        if (result is null) return;

        var dir = Path.Combine(FileSystem.AppDataDirectory, "sounds");
        Directory.CreateDirectory(dir);
        var dest = Path.Combine(dir, $"{(_existing?.Id ?? Guid.NewGuid())}{Path.GetExtension(result.FileName)}");
        File.Copy(result.FullPath, dest, overwrite: true);
        SoundPath = dest;
    }

    private void DeleteCustomFile()
    {
        if (!string.IsNullOrEmpty(SoundPath) &&
            SoundPath != SoundService.ClickSentinel &&
            File.Exists(SoundPath))
            try { File.Delete(SoundPath); } catch { }
    }

    private async Task TryPurchaseAsync()
    {
        var ok = await pro.PurchaseAsync();
        if (!ok)
            await Shell.Current.DisplayAlertAsync("Coming Soon",
                "Pro purchase will be available in the next update.", "OK");
    }

#if ANDROID
    public bool CanPin => LaunchHelper.CanPinShortcuts();

    [RelayCommand]
    private void PinShortcut()
    {
        if (_existing is null) return;
        LaunchHelper.PinShortcut(_existing.Id.ToString(), Label, Uri, Color);
    }
#endif
}

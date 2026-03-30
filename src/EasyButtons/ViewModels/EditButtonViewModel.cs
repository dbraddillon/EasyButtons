using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyButtons.Helpers;
using EasyButtons.Models;
using EasyButtons.Repositories;

namespace EasyButtons.ViewModels;

[QueryProperty(nameof(ButtonId), "buttonId")]
public partial class EditButtonViewModel(EasyButtonRepository repo) : BaseViewModel
{
    private EasyButton? _existing;

    [ObservableProperty] private string _buttonId = string.Empty;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _uri = string.Empty;
    [ObservableProperty] private string _color = "#E53935";
    [ObservableProperty] private bool _isEdit;

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

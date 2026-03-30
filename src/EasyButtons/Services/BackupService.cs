using System.Text.Json;
using EasyButtons.Models;
using EasyButtons.Repositories;

namespace EasyButtons.Services;

/// <summary>
/// Export buttons to a JSON file (shared via Android share sheet).
/// Import from a user-picked JSON file, replacing all existing buttons.
/// Works in both Debug and Release builds — use it before a fresh install.
/// </summary>
public class BackupService(EasyButtonRepository repo)
{
    private const string BackupFileName = "easybuttons-backup.json";

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public async Task ExportAsync()
    {
        var buttons = await repo.GetAllAsync();
        var json    = JsonSerializer.Serialize(buttons, JsonOpts);
        var path    = Path.Combine(FileSystem.CacheDirectory, BackupFileName);

        await File.WriteAllTextAsync(path, json);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "EasyButtons Backup",
            File  = new ShareFile(path, "application/json"),
        });
    }

    /// <returns>Number of buttons restored, or -1 on error.</returns>
    public async Task<int> ImportAsync()
    {
        var pickOptions = new PickOptions
        {
            PickerTitle = "Select EasyButtons Backup (.json)",
            FileTypes   = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, ["application/json", "*/*"] },
                }),
        };

        var result = await FilePicker.PickAsync(pickOptions);
        if (result is null) return -1;

        try
        {
            var json    = await File.ReadAllTextAsync(result.FullPath);
            var buttons = JsonSerializer.Deserialize<List<EasyButton>>(json, JsonOpts);
            if (buttons is null || buttons.Count == 0) return 0;

            await repo.ReplaceAllAsync(buttons);
            return buttons.Count;
        }
        catch
        {
            return -1;
        }
    }
}

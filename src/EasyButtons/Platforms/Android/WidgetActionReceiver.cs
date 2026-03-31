using Android.Appwidget;
using Android.Content;
using Android.Media;
using EasyButtons.Models;
using SQLite;
using IOPath = System.IO.Path;

namespace EasyButtons;

/// <summary>
/// Handles all widget button taps. Each slot gets a unique intent data URI (easybtn://slot/N)
/// so Android's PendingIntent system treats them as genuinely distinct even with the same action.
/// </summary>
[BroadcastReceiver(Exported = false)]
public class WidgetActionReceiver : BroadcastReceiver
{
    public const string Action = "com.voluntarytransactions.easybuttons.WIDGET_TAP";

    private static readonly List<MediaPlayer> _activePlayers = [];

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null) return;

        var widgetId  = intent.GetIntExtra("widget_id", AppWidgetManager.InvalidAppwidgetId);
        var slotIndex = intent.GetIntExtra("slot_index", -1);
        if (slotIndex < 0) return;

        var button = LoadButton(context, widgetId, slotIndex);
        if (button is null) return;

        // SoundPath must be a real existing file — ignore stale sentinel values (e.g. "click")
        var hasSoundFile = !string.IsNullOrEmpty(button.SoundPath) && File.Exists(button.SoundPath);

        if (hasSoundFile)
            PlaySound(button.SoundPath!);
        else if (!string.IsNullOrEmpty(button.Uri))
            LaunchUri(context, button.Uri);
    }

    private static EasyButton? LoadButton(Context context, int widgetId, int slotIndex)
    {
        var prefs = context.GetSharedPreferences(WidgetConfigureActivity.PrefsName, FileCreationMode.Private);
        var config = prefs?.GetString($"config_{widgetId}", null);

        var dbPath = IOPath.Combine(context.FilesDir!.AbsolutePath, "easybuttons.db3");
        if (!File.Exists(dbPath)) return null;

        try
        {
            using var db = new SQLiteConnection(dbPath);

            if (!string.IsNullOrEmpty(config))
            {
                var parts = config.Split(',');
                if (slotIndex < parts.Length && Guid.TryParse(parts[slotIndex], out var id))
                    return db.Find<EasyButton>(id);
                return null;
            }
            else
            {
                return db.Table<EasyButton>().OrderBy(b => b.SortOrder).Skip(slotIndex).FirstOrDefault();
            }
        }
        catch { return null; }
    }

    private static void PlaySound(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        try
        {
            var player = new MediaPlayer();
            _activePlayers.Add(player);
            player.SetDataSource(path);
            player.Prepare();
            player.Start();
            player.Completion += (_, _) =>
            {
                player.Release();
                _activePlayers.Remove(player);
            };
        }
        catch { }
    }

    private static void LaunchUri(Context context, string uri)
    {
        try
        {
            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch { }
    }
}

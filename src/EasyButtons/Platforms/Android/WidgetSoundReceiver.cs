using Android.Content;
using Android.Media;

namespace EasyButtons;

/// <summary>
/// Receives a broadcast from a widget sound-button tap and plays the audio file.
/// Uses a static list to keep players alive through GC until playback completes.
/// </summary>
[BroadcastReceiver(Exported = false)]
public class WidgetSoundReceiver : BroadcastReceiver
{
    public const string ActionPlaySound = "com.voluntarytransactions.easybuttons.PLAY_SOUND";

    private static readonly List<MediaPlayer> _active = [];

    public override void OnReceive(Context? context, Intent? intent)
    {
        var path = intent?.GetStringExtra("sound_path");
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        try
        {
            var player = new MediaPlayer();
            _active.Add(player);
            player.SetDataSource(path);
            player.Prepare();
            player.Start();
            player.Completion += (_, _) =>
            {
                player.Release();
                _active.Remove(player);
            };
        }
        catch { }
    }
}

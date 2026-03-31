using Android.Media;

namespace EasyButtons.Services;

/// <summary>
/// Plays a sound file when a button is tapped.
/// Fire-and-forget — errors never break a launch.
/// </summary>
public class SoundService : IDisposable
{
    private MediaPlayer? _player;

    public void Play(string? soundPath)
    {
        if (string.IsNullOrEmpty(soundPath) || !File.Exists(soundPath)) return;
        try
        {
            _player?.Stop();
            _player?.Release();
            _player = new MediaPlayer();
            _player.SetDataSource(soundPath);
            _player.Prepare();
            _player.Start();
        }
        catch { }
    }

    public void Dispose()
    {
        try { _player?.Release(); } catch { }
        _player = null;
    }
}

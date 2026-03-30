using Android.Media;

namespace EasyButtons.Services;

/// <summary>
/// Plays a local audio file on button tap.
/// Uses Android MediaPlayer directly — no extra NuGet required.
/// Fire-and-forget: errors are swallowed so a bad sound never breaks a launch.
/// </summary>
public class SoundService : IDisposable
{
    private MediaPlayer? _player;

    public void Play(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;
        try
        {
            _player?.Stop();
            _player?.Release();
            _player = new MediaPlayer();
            _player.SetDataSource(filePath);
            _player.Prepare();
            _player.Start();
        }
        catch { /* never crash because of a sound */ }
    }

    public void Dispose()
    {
        try { _player?.Release(); } catch { }
        _player = null;
    }
}

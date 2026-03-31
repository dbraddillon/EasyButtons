using Android.Media;

namespace EasyButtons.Services;

/// <summary>
/// Plays sound on button tap.
/// SoundPath values:
///   null / ""  → silent
///   "click"    → free built-in click (system sound effect)
///   any path   → custom audio file (Pro feature)
/// Fire-and-forget — errors never break a launch.
/// </summary>
public class SoundService : IDisposable
{
    public const string ClickSentinel = "click";

    private MediaPlayer? _player;

    public void Play(string? soundPath)
    {
        if (string.IsNullOrEmpty(soundPath)) return;

        if (soundPath == ClickSentinel)
        {
            PlaySystemClick();
            return;
        }

        PlayFile(soundPath);
    }

    private static void PlaySystemClick()
    {
        try
        {
            var ctx = Android.App.Application.Context;
            var audio = (AudioManager?)ctx.GetSystemService(Android.Content.Context.AudioService);
            audio?.PlaySoundEffect(SoundEffect.KeyClick, 1.0f);
        }
        catch { }
    }

    private void PlayFile(string filePath)
    {
        if (!File.Exists(filePath)) return;
        try
        {
            _player?.Stop();
            _player?.Release();
            _player = new MediaPlayer();
            _player.SetDataSource(filePath);
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

#if ANDROID
using Android.Content;
using AndroidX.Core.Content.PM;
using Android.Content.PM;
using Android.Graphics.Drawables;
#endif

namespace EasyButtons.Helpers;

public static class LaunchHelper
{
    public static Task<bool> TryLaunchAsync(string uri)
    {
        try
        {
#if ANDROID
            // Use native intent directly — MAUI Launcher can fail for http/https URIs
            // that require Android to pick between browser and installed apps (e.g. Spotify)
            var intent = new Android.Content.Intent(
                Android.Content.Intent.ActionView,
                Android.Net.Uri.Parse(uri));
            intent.AddFlags(Android.Content.ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
            return Task.FromResult(true);
#else
            return Launcher.Default.TryOpenAsync(uri);
#endif
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

#if ANDROID
    public static bool CanPinShortcuts()
    {
        var manager = Android.App.Application.Context
            .GetSystemService(Context.ShortcutService) as Android.Content.PM.ShortcutManager;
        return manager?.IsRequestPinShortcutSupported == true;
    }

    public static bool PinShortcut(string id, string label, string uri, string hexColor)
    {
        var context = Android.App.Application.Context;
        var manager = (Android.Content.PM.ShortcutManager)
            context.GetSystemService(Context.ShortcutService)!;

        if (!manager.IsRequestPinShortcutSupported) return false;

        var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri));
        intent.SetPackage(context.PackageName);
        // If this app can't handle it, let the system pick — remove package restriction
        var launchIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri));

        var info = new ShortcutInfoCompat.Builder(context, id)
            .SetShortLabel(label)
            .SetLongLabel(label)
            .SetIntent(launchIntent)
            .Build();

        return ShortcutManagerCompat.RequestPinShortcut(context, info, null);
    }
#endif
}

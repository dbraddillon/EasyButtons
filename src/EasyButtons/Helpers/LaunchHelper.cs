#if ANDROID
using Android.Content;
using AndroidX.Core.Content.PM;
using Android.Content.PM;
using Android.Graphics.Drawables;
#endif

namespace EasyButtons.Helpers;

public static class LaunchHelper
{
    public static async Task<bool> TryLaunchAsync(string uri)
    {
        try
        {
            return await Launcher.Default.TryOpenAsync(uri);
        }
        catch
        {
            return false;
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

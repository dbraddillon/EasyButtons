namespace EasyButtons.Services;

/// <summary>
/// Controls Pro feature access.
/// Free tier: up to FreeButtonLimit buttons.
/// Pro ($1.99 one-time): unlimited buttons + custom sounds per button.
/// IAP SKU: com.voluntarytransactions.easybuttons.pro
///
/// NOTE: Plugin.InAppBilling conflicts with .NET 10 Android billing AAR in debug builds.
/// Real purchase flow wired when building release. Debug toggle covers all testing.
/// </summary>
public class ProService
{
    public const int FreeButtonLimit = 4;
    private const string ProSku = "com.voluntarytransactions.easybuttons.pro";
    private const string CacheKey = "is_pro_purchased";

    private bool _isPro = false;

    public bool IsPro
    {
        get
        {
#if DEBUG
            if (Preferences.Get("debug_is_pro", false)) return true;
#endif
            return _isPro;
        }
    }

    /// <summary>
    /// Call on app start and resume. Uses cached value until IAP package is wired for release.
    /// </summary>
    public Task RefreshAsync()
    {
        // TODO (release): CrossInAppBilling.GetPurchasesAsync → set _isPro → cache
        _isPro = Preferences.Get(CacheKey, false);
        return Task.CompletedTask;
    }

    /// <summary>Trigger the Play Store purchase sheet.</summary>
    public Task<bool> PurchaseAsync()
    {
        // TODO (release): CrossInAppBilling.PurchaseAsync → set _isPro → Preferences.Set(CacheKey, true)
        return Task.FromResult(false);
    }

#if DEBUG
    public void DebugTogglePro()
    {
        var current = Preferences.Get("debug_is_pro", false);
        Preferences.Set("debug_is_pro", !current);
    }

    public bool DebugIsPro => Preferences.Get("debug_is_pro", false);
#endif
}

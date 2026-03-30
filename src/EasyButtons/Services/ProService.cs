namespace EasyButtons.Services;

/// <summary>
/// Controls Pro feature access. Free tier allows up to FreeButtonLimit buttons.
/// Pro unlocks: unlimited buttons + custom sounds per button.
/// IAP SKU: com.voluntarytransactions.easybuttons.pro ($1.99 one-time)
/// </summary>
public class ProService
{
    public const int FreeButtonLimit = 4;

    // TODO: wire to Plugin.InAppBilling purchase verification
    public bool IsPro
    {
        get
        {
#if DEBUG
            // Debug override: toggle via Settings > Debug in the app, persisted in Preferences
            if (Preferences.Get("debug_is_pro", false)) return true;
#endif
            return _isPro;
        }
    }
    private bool _isPro = false;

    /// <summary>Reload purchase state from the store on app resume.</summary>
    public Task RefreshAsync()
    {
        // TODO: check Plugin.InAppBilling for existing purchase
        return Task.CompletedTask;
    }

    /// <summary>Trigger the IAP purchase flow.</summary>
    public Task<bool> PurchaseAsync()
    {
        // TODO: implement Plugin.InAppBilling purchase
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

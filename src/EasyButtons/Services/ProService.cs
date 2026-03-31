#if !DEBUG
using Plugin.InAppBilling;
#endif

namespace EasyButtons.Services;

/// <summary>
/// Controls Pro feature access.
/// Free tier: up to FreeButtonLimit buttons, 2×2 widget (auto-fill only).
/// Pro ($1.99 one-time): unlimited buttons, all widget layouts, widget configure, groups/folders.
/// IAP SKU: com.voluntarytransactions.easybuttons.pro
/// </summary>
public class ProService
{
    public const int FreeButtonLimit = 4;
    public const string ProSku = "com.voluntarytransactions.easybuttons.pro";
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

    /// <summary>Call on app start and resume.</summary>
    public async Task RefreshAsync()
    {
#if DEBUG
        _isPro = Preferences.Get(CacheKey, false);
        await Task.CompletedTask;
#else
        // Fast path: cached purchase → no network call needed
        if (Preferences.Get(CacheKey, false)) { _isPro = true; return; }
        try
        {
            if (!CrossInAppBilling.IsSupported) return;
            var billing = CrossInAppBilling.Current;
            if (!await billing.ConnectAsync()) return;
            var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
            _isPro = purchases?.Any(p =>
                p.ProductId == ProSku &&
                p.State == PurchaseState.Purchased) == true;
            if (_isPro) Preferences.Set(CacheKey, true);
        }
        catch { _isPro = Preferences.Get(CacheKey, false); }
        finally
        {
            try { await CrossInAppBilling.Current.DisconnectAsync(); } catch { }
        }
#endif
    }

    /// <summary>Trigger the Play Store purchase sheet. Returns true if purchase succeeded.</summary>
    public async Task<bool> PurchaseAsync()
    {
#if DEBUG
        return false; // Use debug toggle in the debug bar
#else
        if (_isPro) return true;
        try
        {
            if (!CrossInAppBilling.IsSupported) return false;
            var billing = CrossInAppBilling.Current;
            if (!await billing.ConnectAsync()) return false;
            var purchase = await billing.PurchaseAsync(ProSku, ItemType.InAppPurchase);
            if (purchase?.State == PurchaseState.Purchased)
            {
                _isPro = true;
                Preferences.Set(CacheKey, true);
                return true;
            }
            return false;
        }
        catch { return false; }
        finally
        {
            try { await CrossInAppBilling.Current.DisconnectAsync(); } catch { }
        }
#endif
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

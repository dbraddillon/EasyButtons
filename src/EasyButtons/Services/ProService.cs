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
    public bool IsPro { get; private set; } = false;

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
}

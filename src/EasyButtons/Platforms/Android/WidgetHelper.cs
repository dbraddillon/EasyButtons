using Android.Content;

namespace EasyButtons;

/// <summary>Called from ViewModels to push a widget refresh after any button change.</summary>
public static class WidgetHelper
{
    public static void RequestUpdate()
    {
        var ctx = Android.App.Application.Context;
        EasyButtonWidgetProvider.UpdateAll(ctx);
    }
}

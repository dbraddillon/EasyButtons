using Android.App;
using Android.Appwidget;
using Android.Content;

namespace EasyButtons;

/// <summary>4×1 vertical strip widget variant.</summary>
[BroadcastReceiver(Label = "EasyButtons ↕ Vertical", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/easy_button_widget_info_vertical")]
public class EasyButtonWidgetProviderV : EasyButtonWidgetProvider
{
    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context is null || appWidgetManager is null || appWidgetIds is null) return;
        foreach (var id in appWidgetIds)
            UpdateWidget(context, appWidgetManager, id, Resource.Layout.easy_button_widget_vertical);
    }

    public static new void UpdateAll(Context context)
        => UpdateAllFor(context, typeof(EasyButtonWidgetProviderV), Resource.Layout.easy_button_widget_vertical);
}

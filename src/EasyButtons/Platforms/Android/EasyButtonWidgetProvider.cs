using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using EasyButtons.Models;
using SQLite;
using AGPaint = Android.Graphics.Paint;
using IOPath = System.IO.Path;

namespace EasyButtons;

[BroadcastReceiver(Label = "EasyButtons", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/easy_button_widget_info")]
public class EasyButtonWidgetProvider : AppWidgetProvider
{
    protected static readonly int[] SlotViewIds =
    [
        Resource.Id.widget_btn_0,
        Resource.Id.widget_btn_1,
        Resource.Id.widget_btn_2,
        Resource.Id.widget_btn_3,
    ];

    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context is null || appWidgetManager is null || appWidgetIds is null) return;
        foreach (var id in appWidgetIds)
            UpdateWidget(context, appWidgetManager, id, Resource.Layout.easy_button_widget);
    }

    public static void UpdateAll(Context context)
        => UpdateAllFor(context, typeof(EasyButtonWidgetProvider), Resource.Layout.easy_button_widget);

    /// <summary>Updates a single widget by ID, auto-detecting which layout it uses.</summary>
    public static void UpdateWidgetById(Context context, int widgetId)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null) return;

        var info = manager.GetAppWidgetInfo(widgetId);
        var className = info?.Provider?.ClassName ?? "";
        var layoutResource = className switch
        {
            var n when n.Contains("ProviderV") => Resource.Layout.easy_button_widget_vertical,
            var n when n.Contains("ProviderH") => Resource.Layout.easy_button_widget_horizontal,
            _ => Resource.Layout.easy_button_widget
        };

        UpdateWidget(context, manager, widgetId, layoutResource);
    }

    // ── Shared helpers used by all layout variants ──────────────────────────────

    protected static void UpdateAllFor(Context context, Type providerType, int layoutResource)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null) return;
        var provider = new ComponentName(context, Java.Lang.Class.FromType(providerType));
        var ids = manager.GetAppWidgetIds(provider);
        if (ids is null || ids.Length == 0) return;
        foreach (var id in ids)
            UpdateWidget(context, manager, id, layoutResource);
    }

    protected static void UpdateWidget(Context context, AppWidgetManager manager, int widgetId, int layoutResource)
    {
        var buttons = LoadButtonsForWidget(context, widgetId);
        var views = new RemoteViews(context.PackageName!, layoutResource);

        for (int i = 0; i < 4; i++)
        {
            var viewId = SlotViewIds[i];
            var btn = buttons[i];

            // URI includes widgetId so each widget instance gets its own PendingIntents
            var tapIntent = new Intent(context, typeof(WidgetActionReceiver));
            tapIntent.SetAction(WidgetActionReceiver.Action);
            tapIntent.SetData(Android.Net.Uri.Parse($"easybtn://widget/{widgetId}/slot/{i}"));
            tapIntent.PutExtra("slot_index", i);
            tapIntent.PutExtra("widget_id", widgetId);
            var pi = PendingIntent.GetBroadcast(context, 0, tapIntent,
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)!;

            if (btn is not null)
            {
                views.SetImageViewBitmap(viewId, CreateButtonBitmap(btn.Label, btn.Color, 200));
                views.SetOnClickPendingIntent(viewId, pi);
            }
            else
            {
                views.SetImageViewBitmap(viewId, CreateEmptyBitmap(200));
                views.SetOnClickPendingIntent(viewId, pi);
            }
        }

        manager.UpdateAppWidget(widgetId, views);
    }

    // ── Data ────────────────────────────────────────────────────────────────────

    /// <summary>Returns 4 slots for the widget; null = empty slot. Uses per-widget config if set.</summary>
    private static List<EasyButton?> LoadButtonsForWidget(Context context, int widgetId)
    {
        var result = new List<EasyButton?> { null, null, null, null };

        var prefs = context.GetSharedPreferences(WidgetConfigureActivity.PrefsName, FileCreationMode.Private);
        var config = prefs?.GetString($"config_{widgetId}", null);

        var dbPath = IOPath.Combine(context.FilesDir!.AbsolutePath, "easybuttons.db3");
        if (!File.Exists(dbPath)) return result;

        try
        {
            using var db = new SQLiteConnection(dbPath);

            if (!string.IsNullOrEmpty(config))
            {
                var parts = config.Split(',');
                for (int i = 0; i < Math.Min(4, parts.Length); i++)
                    if (Guid.TryParse(parts[i], out var id))
                        result[i] = db.Find<EasyButton>(id);
            }
            else
            {
                // No config yet — fall back to first 4 by sort order
                var buttons = db.Table<EasyButton>().OrderBy(b => b.SortOrder).Take(4).ToList();
                for (int i = 0; i < buttons.Count; i++)
                    result[i] = buttons[i];
            }
        }
        catch { }

        return result;
    }

    // ── Bitmap rendering ────────────────────────────────────────────────────────

    internal static Bitmap CreateButtonBitmap(string label, string hexColor, int size)
    {
        var bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888!)!;
        using var canvas = new Canvas(bitmap);

        float cx = size / 2f;
        float cy = size / 2f;
        float r  = size / 2f - 4;

        Android.Graphics.Color baseColor;
        try   { baseColor = Android.Graphics.Color.ParseColor(hexColor); }
        catch { baseColor = Android.Graphics.Color.ParseColor("#E53935"); }

        using var domePaint = new AGPaint { AntiAlias = true };
        domePaint.Color = baseColor;
        canvas.DrawCircle(cx, cy, r, domePaint);

        using var glossPaint = new AGPaint { AntiAlias = true };
        using var shader = new Android.Graphics.RadialGradient(
            cx - r * 0.2f, cy - r * 0.32f, r * 0.7f,
            Android.Graphics.Color.Argb(85, 255, 255, 255),
            Android.Graphics.Color.Argb(0,  255, 255, 255),
            Android.Graphics.Shader.TileMode.Clamp!);
        glossPaint.SetShader(shader);
        canvas.DrawCircle(cx, cy, r, glossPaint);

        var text = label.Length > 10 ? label[..9].TrimEnd() + "…" : label;
        using var textPaint = new AGPaint { AntiAlias = true };
        textPaint.Color = Android.Graphics.Color.White;
        textPaint.TextAlign = AGPaint.Align.Center;
        textPaint.TextSize = size * 0.155f;
        textPaint.SetTypeface(Typeface.DefaultBold);
        var metrics = textPaint.GetFontMetrics()!;
        float textY = cy - (metrics.Ascent + metrics.Descent) / 2f;
        canvas.DrawText(text, cx, textY, textPaint);

        return bitmap;
    }

    internal static Bitmap CreateEmptyBitmap(int size)
    {
        var bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888!)!;
        using var canvas = new Canvas(bitmap);
        using var paint = new AGPaint { AntiAlias = true };
        paint.Color = Android.Graphics.Color.Argb(35, 255, 255, 255);
        paint.SetStyle(AGPaint.Style.Stroke);
        paint.StrokeWidth = 3f;
        canvas.DrawCircle(size / 2f, size / 2f, size / 2f - 6f, paint);
        return bitmap;
    }
}

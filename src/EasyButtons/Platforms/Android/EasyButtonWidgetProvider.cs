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
    private static readonly int[] SlotViewIds =
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
            UpdateWidget(context, appWidgetManager, id);
    }

    /// <summary>Called from the app whenever buttons change so widgets stay current.</summary>
    public static void UpdateAll(Context context)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null) return;
        var provider = new ComponentName(context, Java.Lang.Class.FromType(typeof(EasyButtonWidgetProvider)));
        var ids = manager.GetAppWidgetIds(provider);
        if (ids is null || ids.Length == 0) return;
        foreach (var id in ids)
            UpdateWidget(context, manager, id);
    }

    private static void UpdateWidget(Context context, AppWidgetManager manager, int widgetId)
    {
        var buttons = LoadButtons(context);
        var views = new RemoteViews(context.PackageName!, Resource.Layout.easy_button_widget);

        for (int i = 0; i < 4; i++)
        {
            var viewId = SlotViewIds[i];
            if (i < buttons.Count)
            {
                var btn = buttons[i];
                views.SetImageViewBitmap(viewId, CreateButtonBitmap(btn.Label, btn.Color, 200));

                PendingIntent pi;
                if (!string.IsNullOrEmpty(btn.SoundPath))
                {
                    var intent = new Intent(context, typeof(WidgetSoundReceiver));
                    intent.SetAction(WidgetSoundReceiver.ActionPlaySound);
                    intent.PutExtra("sound_path", btn.SoundPath);
                    pi = PendingIntent.GetBroadcast(context, i, intent,
                        PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)!;
                }
                else
                {
                    var uri = Android.Net.Uri.Parse(btn.Uri);
                    var intent = new Intent(Intent.ActionView, uri);
                    intent.AddFlags(ActivityFlags.NewTask);
                    pi = PendingIntent.GetActivity(context, i + 100, intent,
                        PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)!;
                }
                views.SetOnClickPendingIntent(viewId, pi);
            }
            else
            {
                views.SetImageViewBitmap(viewId, CreateEmptyBitmap(200));
                views.SetOnClickPendingIntent(viewId, null);
            }
        }

        manager.UpdateAppWidget(widgetId, views);
    }

    // ── Data ────────────────────────────────────────────────────────────────────

    private static List<EasyButton> LoadButtons(Context context)
    {
        var dbPath = IOPath.Combine(context.FilesDir!.AbsolutePath, "easybuttons.db3");
        if (!File.Exists(dbPath)) return [];
        try
        {
            using var db = new SQLiteConnection(dbPath);
            return [.. db.Table<EasyButton>().OrderBy(b => b.SortOrder).Take(4)];
        }
        catch { return []; }
    }

    // ── Bitmap rendering ────────────────────────────────────────────────────────

    private static Bitmap CreateButtonBitmap(string label, string hexColor, int size)
    {
        var bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888!)!;
        using var canvas = new Canvas(bitmap);

        float cx = size / 2f;
        float cy = size / 2f;
        float r  = size / 2f - 4;

        // Parse hex color
        Android.Graphics.Color baseColor;
        try   { baseColor = Android.Graphics.Color.ParseColor(hexColor); }
        catch { baseColor = Android.Graphics.Color.ParseColor("#E53935"); }

        // Dome circle
        using var domePaint = new AGPaint { AntiAlias = true };
        domePaint.Color = baseColor;
        canvas.DrawCircle(cx, cy, r, domePaint);

        // Gloss highlight — upper-left radial fade
        using var glossPaint = new AGPaint { AntiAlias = true };
        using var shader = new Android.Graphics.RadialGradient(
            cx - r * 0.2f, cy - r * 0.32f, r * 0.7f,
            Android.Graphics.Color.Argb(85, 255, 255, 255),
            Android.Graphics.Color.Argb(0,  255, 255, 255),
            Android.Graphics.Shader.TileMode.Clamp!);
        glossPaint.SetShader(shader);
        canvas.DrawCircle(cx, cy, r, glossPaint);

        // Label — truncate long names
        var text = label.Length > 10 ? label[..9].TrimEnd() + "…" : label;
        using var textPaint = new AGPaint { AntiAlias = true };
        textPaint.Color = Android.Graphics.Color.White;
        textPaint.TextAlign = AGPaint.Align.Center;
        textPaint.TextSize = size * 0.155f;
        textPaint.SetTypeface(Typeface.DefaultBold);
        // Vertical center: DrawText baseline is at y, so offset up slightly
        var metrics = textPaint.GetFontMetrics()!;
        float textY = cy - (metrics.Ascent + metrics.Descent) / 2f;
        canvas.DrawText(text, cx, textY, textPaint);

        return bitmap;
    }

    private static Bitmap CreateEmptyBitmap(int size)
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

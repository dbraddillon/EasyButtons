using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using EasyButtons.Models;
using SQLite;
using IOPath = System.IO.Path;

namespace EasyButtons;

[Activity(
    Name = "com.voluntarytransactions.easybuttons.WidgetConfigureActivity",
    Label = "Configure Widget",
    Exported = true,
    Theme = "@android:style/Theme.DeviceDefault.NoActionBar")]
public class WidgetConfigureActivity : Activity
{
    internal const string PrefsName = "EasyButtonsWidgetPrefs";

    private int _appWidgetId = AppWidgetManager.InvalidAppwidgetId;
    private readonly List<EasyButton> _allButtons = [];
    private readonly List<Guid> _selectedIds = [];     // ordered: index 0 = slot 0
    private readonly Dictionary<Guid, TextView> _badges = new();
    private Android.Widget.Button? _saveButton;

    protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // If user backs out, widget is NOT placed
        SetResult(Result.Canceled);

        _appWidgetId = Intent?.GetIntExtra(AppWidgetManager.ExtraAppwidgetId,
            AppWidgetManager.InvalidAppwidgetId) ?? AppWidgetManager.InvalidAppwidgetId;

        if (_appWidgetId == AppWidgetManager.InvalidAppwidgetId)
        {
            Finish();
            return;
        }

        // V/H widget layouts are Pro-only — gate before showing configure UI
        if (IsProOnlyWidget() && !IsProPurchased())
        {
            ShowProUpsellAndExit();
            return;
        }

        // Pre-load any existing config for this widget (re-configure case)
        var prefs = GetSharedPreferences(PrefsName, FileCreationMode.Private);
        var existing = prefs?.GetString($"config_{_appWidgetId}", null);
        if (!string.IsNullOrEmpty(existing))
            foreach (var s in existing.Split(','))
                if (Guid.TryParse(s, out var g)) _selectedIds.Add(g);

        SetContentView(Resource.Layout.widget_configure);

        _saveButton = FindViewById<Android.Widget.Button>(Resource.Id.btn_save);
        if (_saveButton == null) { Finish(); return; }
        _saveButton.Click += OnSaveClick;

        _allButtons.AddRange(LoadAllButtons());

        // Convenience: auto-select all when there are 4 or fewer and no prior config
        if (_allButtons.Count > 0 && _allButtons.Count <= 4 && string.IsNullOrEmpty(existing))
            foreach (var b in _allButtons) _selectedIds.Add(b.Id);

        BuildRows();
        UpdateSaveButton();
    }

    // ── Row building ────────────────────────────────────────────────────────────

    private void BuildRows()
    {
        var container = FindViewById<LinearLayout>(Resource.Id.button_list)!;
        container.RemoveAllViews();
        _badges.Clear();

        if (_allButtons.Count == 0)
        {
            var empty = new TextView(this)
            {
                Text = "No buttons yet.\nAdd buttons in EasyButtons first.",
                TextAlignment = Android.Views.TextAlignment.Center,
                LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent)
            };
            empty.SetTextColor(Android.Graphics.Color.ParseColor("#888888"));
            empty.SetPadding(0, Dp(40), 0, Dp(40));
            container.AddView(empty);
            return;
        }

        foreach (var btn in _allButtons)
            container.AddView(BuildRow(btn));
    }

    private LinearLayout BuildRow(EasyButton btn)
    {
        var row = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal,
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        row.SetPadding(Dp(16), Dp(10), Dp(16), Dp(10));
        row.SetGravity(GravityFlags.CenterVertical);

        // Ripple so it feels tappable
        var ripple = new Android.Util.TypedValue();
        Theme?.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackground, ripple, true);
        if (ripple.ResourceId != 0)
            row.SetBackgroundResource(ripple.ResourceId);

        // Dome image
        var imgView = new ImageView(this);
        var bmp = EasyButtonWidgetProvider.CreateButtonBitmap(btn.Label, btn.Color, 120);
        imgView.SetImageBitmap(bmp);
        var imgLp = new LinearLayout.LayoutParams(Dp(48), Dp(48));
        imgLp.SetMargins(0, 0, Dp(14), 0);
        imgView.LayoutParameters = imgLp;
        imgView.SetScaleType(ImageView.ScaleType.FitCenter);
        row.AddView(imgView);

        // Label
        var labelView = new TextView(this)
        {
            Text = btn.Label,
            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f),
            Gravity = GravityFlags.CenterVertical
        };
        labelView.SetTextColor(Android.Graphics.Color.White);
        labelView.TextSize = 16f;
        row.AddView(labelView);

        // Selection badge
        var badge = new TextView(this)
        {
            Gravity = GravityFlags.Center,
            LayoutParameters = new LinearLayout.LayoutParams(Dp(28), Dp(28))
        };
        badge.TextSize = 13f;
        badge.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
        _badges[btn.Id] = badge;
        RefreshBadge(btn.Id);
        row.AddView(badge);

        row.Click += (_, _) => OnRowTapped(btn);
        return row;
    }

    private void RefreshBadge(Guid id)
    {
        if (!_badges.TryGetValue(id, out var badge)) return;

        var idx = _selectedIds.IndexOf(id);
        var shape = new GradientDrawable();
        shape.SetCornerRadius(1000f); // fully rounded = circle

        if (idx >= 0)
        {
            badge.Text = (idx + 1).ToString();
            badge.SetTextColor(Android.Graphics.Color.White);
            shape.SetColor(Android.Graphics.Color.ParseColor("#E53935"));
        }
        else
        {
            badge.Text = "";
            shape.SetColor(Android.Graphics.Color.Transparent);
            shape.SetStroke(Dp(2), Android.Graphics.Color.Argb(80, 255, 255, 255));
        }
        badge.Background = shape;
    }

    // ── Interaction ─────────────────────────────────────────────────────────────

    private void OnRowTapped(EasyButton btn)
    {
        if (_selectedIds.Contains(btn.Id))
        {
            _selectedIds.Remove(btn.Id);
            // Refresh all badges because slot numbers shifted
            foreach (var id in _badges.Keys) RefreshBadge(id);
        }
        else if (_selectedIds.Count < 4)
        {
            _selectedIds.Add(btn.Id);
            RefreshBadge(btn.Id);
        }
        UpdateSaveButton();
    }

    private void UpdateSaveButton()
    {
        if (_saveButton is null) return;
        _saveButton.Enabled = _selectedIds.Count > 0 && _allButtons.Count > 0;
        _saveButton.Alpha = _saveButton.Enabled ? 1f : 0.38f;
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        if (_selectedIds.Count == 0) return;

        // Save config
        var prefs = GetSharedPreferences(PrefsName, FileCreationMode.Private)!;
        prefs.Edit()!
            .PutString($"config_{_appWidgetId}", string.Join(",", _selectedIds))
            .Apply();

        // Push widget update
        EasyButtonWidgetProvider.UpdateWidgetById(this, _appWidgetId);

        // Tell the system to place the widget
        var result = new Intent();
        result.PutExtra(AppWidgetManager.ExtraAppwidgetId, _appWidgetId);
        SetResult(Result.Ok, result);
        Finish();
    }

    // ── Data ────────────────────────────────────────────────────────────────────

    private List<EasyButton> LoadAllButtons()
    {
        var dbPath = IOPath.Combine(FilesDir!.AbsolutePath, "easybuttons.db3");
        if (!File.Exists(dbPath)) return [];
        try
        {
            using var db = new SQLiteConnection(dbPath);
            return [.. db.Table<EasyButton>().OrderBy(b => b.SortOrder)];
        }
        catch { return []; }
    }

    // ── Pro gating ──────────────────────────────────────────────────────────────

    /// <summary>V/H layouts require Pro; the standard 2×2 is free.</summary>
    private bool IsProOnlyWidget()
    {
        var manager = AppWidgetManager.GetInstance(this);
        var info = manager?.GetAppWidgetInfo(_appWidgetId);
        var className = info?.Provider?.ClassName ?? "";
        return className.Contains("ProviderV") || className.Contains("ProviderH");
    }

    // Uses native SharedPreferences directly — MAUI.Storage.Preferences requires the
    // MAUI runtime to be initialized, which hasn't happened when this Activity is launched
    // cold from the home screen widget picker.
    private bool IsProPurchased()
    {
        // MAUI stores Preferences in {packageName}_preferences via PreferenceManager defaults
        var prefsFile = PackageName + "_preferences";
#if DEBUG
        try
        {
            var p = GetSharedPreferences(prefsFile, FileCreationMode.Private);
            if (p?.GetBoolean("debug_is_pro", false) == true) return true;
        }
        catch { }
#endif
        try
        {
            var p = GetSharedPreferences(prefsFile, FileCreationMode.Private);
            return p?.GetBoolean("is_pro_purchased", false) == true;
        }
        catch { return false; }
    }

    private void ShowProUpsellAndExit()
    {
        new AlertDialog.Builder(this)!
            .SetTitle("EasyButtons Pro")!
            .SetMessage("Vertical and horizontal widget layouts are included in EasyButtons Pro ($1.99 one-time). Upgrade in the app to unlock all widget types.")!
            .SetPositiveButton("OK", (_, _) => Finish())!
            .SetCancelable(false)!
            .Show();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private int Dp(int dp) => (int)(dp * (Resources?.DisplayMetrics?.Density ?? 2f));
}

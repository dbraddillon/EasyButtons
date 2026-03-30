# EasyButtons

Android app that puts big, round, Staples-style "easy buttons" on your phone. Each button has a label, a color, and a URI — tap to launch it instantly. Long press to edit. Think of it as a personal shortcut launcher with personality.

## Stack

| Layer | Tech |
|---|---|
| Platform | .NET MAUI, Android only (`net10.0-android`) |
| MVVM | CommunityToolkit.Mvvm |
| Local DB | SQLite via sqlite-net-pcl |
| Package ID | `com.voluntarytransactions.easybuttons` |

## Solution layout

```
EasyButtons.sln
src/EasyButtons/
  Models/
    EasyButton.cs              ← id, label, color (hex string), uri, sortOrder
  Data/
    DatabaseContext.cs         ← SQLite setup
  Repositories/
    EasyButtonRepository.cs    ← CRUD
  ViewModels/
    MainViewModel.cs           ← Buttons list, LaunchCommand, EditCommand, AddCommand
    EditButtonViewModel.cs     ← Form state, SaveCommand, DeleteCommand, PinShortcut, preset colors
  Pages/
    MainPage.xaml              ← 2-col CollectionView grid of round buttons
    EditButtonPage.xaml        ← Label + URI fields, color swatch picker, delete, pin
  Helpers/
    ButtonGestureBehavior.cs   ← Native GestureDetector: tap → launch, long press → edit
    HexToColorConverter.cs     ← IValueConverter: hex string → MAUI Color
    LaunchHelper.cs            ← Native Android Intent for URI launch + PinShortcut()
    InvertBoolConverter.cs     ← bool → !bool for empty state visibility
```

## Build / deploy

```bash
# Debug build for ADB deploy
dotnet build src/EasyButtons/EasyButtons.csproj -f net10.0-android -c Debug \
  -p:EmbedAssembliesIntoApk=true

ADB="/c/Program Files (x86)/Android/android-sdk/platform-tools/adb.exe"
"$ADB" install -r "src/EasyButtons/bin/Debug/net10.0-android/com.voluntarytransactions.easybuttons-Signed.apk"
"$ADB" shell monkey -p com.voluntarytransactions.easybuttons -c android.intent.category.LAUNCHER 1
```

## Key architecture decisions

**ButtonGestureBehavior** — MAUI's `TapGestureRecognizer` sets its own `OnTouchListener` which consumes touch events and preventing Android's native long-click from firing. The fix: a single native `GestureDetector.SimpleOnGestureListener` handling both `OnSingleTapUp` (tap) and `OnLongPress`. Replaces MAUI's gesture system entirely for button grid items. Bound via `x:Reference` to the page (not `RelativeSource`) because behaviors don't support `AncestorType` binding.

**LaunchHelper** — Uses native `Android.Content.Intent(ActionView, Uri)` instead of `Launcher.Default.TryOpenAsync()`. MAUI's launcher fails for `https://` URIs that require Android to pick between browser and installed apps (e.g. Spotify). Native intent fires the system chooser correctly.

**HexToColorConverter** — Buttons store color as a hex string in SQLite. MAUI `BackgroundColor` expects a `Color` object. The converter bridges this so the binding just works.

**EditCommand navigation** — Shell navigation must be on the main thread. Wrapped in `MainThread.InvokeOnMainThreadAsync()` to prevent ANR when invoked from a native touch callback.

**CommunityToolkit.Maui was tried and removed** — `TouchBehavior.LongPressCommand` inside a CollectionView DataTemplate caused crashes (`RelativeSource AncestorType` not supported on behaviors) and ANRs (command invoked off main thread). Replaced with the native `GestureDetector` behavior above.

## URI schemes that work out of the box

Any URI Android can resolve via `Intent.ActionView`. Examples:

| What | URI |
|---|---|
| Spotify track/playlist | `spotify:track:4uLU6hMCjMI75M1A2tKUQC` |
| Spotify (web URL) | `https://open.spotify.com/playlist/...` |
| YouTube video | `https://youtube.com/watch?v=...` |
| Google Maps navigation | `google.navigation:q=Times+Square,New+York` |
| Maps search | `geo:0,0?q=coffee+near+me` |
| Phone call | `tel:+15551234567` |
| SMS | `sms:+15551234567` |
| Website | `https://example.com` |
| Any app's deep link | Whatever scheme that app registers |

If you can share a link to it from any Android app, it works here.

## Home screen shortcut pinning (live)

Long press a button → edit → "📌 Pin to Home Screen" appears at the bottom (if device supports it). Android prompts placement. The shortcut gets a colored circle icon matching the button and fires the URI directly — no app UI involved.

Implementation: `LaunchHelper.PinShortcut()` via `ShortcutManagerCompat.RequestPinShortcut`. Icon is a programmatically-drawn colored circle bitmap via `IconCompat.CreateWithBitmap`. `EditButtonViewModel.CanPin` gates visibility. `Android.Graphics.Paint`/`Color` must be fully qualified to avoid ambiguity with `Microsoft.Maui.Graphics` types.

## Future ideas / next session

**Quick Launch strip activity** — the most interesting open idea. A second transparent Activity that slides up as a bottom sheet showing a horizontal scroll of buttons. Add its launcher to the home screen as a shortcut. Tap → strip appears → tap a button → fires URI and dismisses. Self-contained, nothing touches the existing app. Medium effort.

**Android App Widget** — a 1×N strip of buttons on the home screen. Pure Android platform code (`RemoteViews`, `AppWidgetProvider`, `BroadcastReceiver`) — MAUI doesn't help here at all. Reads from the same SQLite DB. Bigger project, but the "always on home screen" version of the strip idea.

**App icon long-press shortcuts** — already enabled by `PinShortcut`. Long-pressing the EasyButtons icon in the app drawer shows up to 5 pinned shortcuts. No extra work needed — just pin some buttons.

## Play Store

Not yet submitted. `store/` folder not yet created.

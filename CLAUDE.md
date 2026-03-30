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
    EditButtonViewModel.cs     ← Form state, SaveCommand, DeleteCommand, preset colors
  Pages/
    MainPage.xaml              ← 2-col CollectionView grid of round buttons
    EditButtonPage.xaml        ← Label + URI fields, color swatch picker, delete
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

**ButtonGestureBehavior** — MAUI's `TapGestureRecognizer` sets its own `OnTouchListener` which consumes touch events and prevents Android's native long-click from firing. The fix: a single native `GestureDetector.SimpleOnGestureListener` handling both `OnSingleTapUp` (tap) and `OnLongPress`. This replaces MAUI's gesture system entirely for the button grid items. Bound via `x:Reference` to the page (not `RelativeSource`) because behaviors don't support `AncestorType` binding.

**LaunchHelper** — Uses native `Android.Content.Intent(ActionView, Uri)` instead of `Launcher.Default.TryOpenAsync()`. MAUI's launcher fails for `https://` URIs that require Android to pick between browser and installed apps (e.g. Spotify web URLs). Native intent fires the system chooser correctly.

**HexToColorConverter** — Buttons store color as a hex string in SQLite. MAUI `BackgroundColor` expects a `Color` object. The converter bridges this so the binding just works.

**EditCommand navigation** — Shell navigation must be on the main thread. Wrapping in `MainThread.InvokeOnMainThreadAsync()` prevents an ANR when the command is invoked from a native touch callback.

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

## Home screen shortcut pinning

`LaunchHelper.PinShortcut(id, label, uri, hexColor)` is implemented via `ShortcutManagerCompat.RequestPinShortcut`. Not yet exposed in the UI — could be a toggle/button on the EditButtonPage.

## Play Store

Not yet submitted. `store/` folder not yet created.

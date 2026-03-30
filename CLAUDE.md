# EasyButtons

Android shortcut launcher — configure big round buttons that fire URIs (deep links, https, spotify:, etc). Tap to launch, double-tap to edit. Pin to home screen as Android shortcuts.

## Stack
- .NET MAUI Android (net10.0-android)
- SQLite via sqlite-net-pcl
- CommunityToolkit.Mvvm

## Package
`com.voluntarytransactions.easybuttons`

## Build & deploy
```bash
dotnet build src/EasyButtons/EasyButtons.csproj -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
ADB="/c/Program Files (x86)/Android/android-sdk/platform-tools/adb.exe"
"$ADB" install -r "src/EasyButtons/bin/Debug/net10.0-android/com.voluntarytransactions.easybuttons-Signed.apk"
"$ADB" shell monkey -p com.voluntarytransactions.easybuttons -c android.intent.category.LAUNCHER 1
```

## URI examples
- Spotify track: `spotify:track:TRACK_ID`
- YouTube video: `https://youtu.be/VIDEO_ID`
- Navigation: `google.navigation:q=1600+Pennsylvania+Ave`
- Phone call: `tel:+15555555555`
- Website: `https://example.com`

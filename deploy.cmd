@echo off
:: Builds and deploys EasyButtons to the connected Android device (USB or WiFi).
:: For WiFi: run wifi-adb.cmd first to connect, then run this.
:: For USB:  just plug in and run this.

set ADB=C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe
set PKG=com.voluntarytransactions.easybuttons
set APK=src\EasyButtons\bin\Debug\net10.0-android\%PKG%-Signed.apk
set PROJ=src\EasyButtons\EasyButtons.csproj

echo.
echo === Building ===
dotnet build %PROJ% -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
if errorlevel 1 goto :error

echo.
echo === Installing ===
"%ADB%" install -r %APK%
if errorlevel 1 goto :error

echo.
echo === Launching ===
"%ADB%" shell monkey -p %PKG% -c android.intent.category.LAUNCHER 1

echo.
echo Done.
goto :end

:error
echo.
echo FAILED. Check output above.
pause

:end

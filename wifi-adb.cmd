@echo off
:: Connects to the Pixel over WiFi ADB (port locked to 5555 via USB tcpip).
:: Run this after USB has been used once this session to set tcpip mode.
:: If it stops working after a reboot, plug in USB and run:
::   adb tcpip 5555
:: then unplug and run this script again.

set ADB=C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe
set PHONE_IP=192.168.50.247

echo Connecting to %PHONE_IP%:5555...
"%ADB%" connect %PHONE_IP%:5555
"%ADB%" devices

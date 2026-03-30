#!/usr/bin/env bash
# Deploy EasyButtons to the connected Android device (USB, LAN WiFi, or Tailscale).
# Delegates to the global adb-deploy.sh for connection detection logic.
# Requires: adb tcpip 5555 run once over USB per phone reboot.

bash "$HOME/.claude/scripts/adb-deploy.sh" \
    "src/EasyButtons/EasyButtons.csproj" \
    "com.voluntarytransactions.easybuttons"

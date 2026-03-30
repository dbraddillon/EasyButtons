#!/usr/bin/env bash
# EasyButtons deploy script
# Auto-detects ADB connection: USB → LAN WiFi → Tailscale
# Requires: adb tcpip 5555 run once over USB (already done 2026-03-30)

ADB="/c/Program Files (x86)/Android/android-sdk/platform-tools/adb.exe"
PKG="com.voluntarytransactions.easybuttons"
APK="src/EasyButtons/bin/Debug/net10.0-android/${PKG}-Signed.apk"
PROJ="src/EasyButtons/EasyButtons.csproj"

LAN_IP="192.168.50.247"
TAILSCALE_IP="100.117.148.117"
PORT="5555"

# ── 1. Ensure exactly one device is targeted ─────────────────────────────────

pick_device() {
    local devices
    devices=$("$ADB" devices | tail -n +2 | grep "device$")

    local usb
    usb=$(echo "$devices" | grep -v ":")   # USB serials have no colon

    local wifi
    wifi=$(echo "$devices" | grep ":")     # WiFi/Tailscale are IP:port

    if [ -n "$usb" ]; then
        echo "→ USB device connected"
        # If WiFi also connected, disconnect it to avoid "more than one device" error
        if [ -n "$wifi" ]; then
            echo "  (also saw WiFi — disconnecting to avoid ambiguity)"
            "$ADB" disconnect > /dev/null 2>&1
        fi
        return 0
    fi

    if [ -n "$wifi" ]; then
        echo "→ Already connected via WiFi/Tailscale"
        return 0
    fi

    # Nothing connected — try LAN
    echo "→ No device, trying LAN ($LAN_IP:$PORT)..."
    "$ADB" connect "$LAN_IP:$PORT" > /dev/null 2>&1
    if "$ADB" devices | grep -q "$LAN_IP"; then
        echo "→ Connected via LAN"
        return 0
    fi

    # Try Tailscale
    echo "→ LAN not reachable, trying Tailscale ($TAILSCALE_IP:$PORT)..."
    "$ADB" connect "$TAILSCALE_IP:$PORT" > /dev/null 2>&1
    if "$ADB" devices | grep -q "$TAILSCALE_IP"; then
        echo "→ Connected via Tailscale"
        return 0
    fi

    echo "✗ No device found (USB, LAN, or Tailscale)."
    echo ""
    echo "  If ADB tcpip mode was lost (phone rebooted), plug in USB and run:"
    echo "    \"$ADB\" tcpip 5555"
    echo "  then unplug and re-run this script."
    return 1
}

pick_device || exit 1

# ── 2. Build ─────────────────────────────────────────────────────────────────

echo ""
echo "=== Building ==="
dotnet build "$PROJ" -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
[ $? -ne 0 ] && echo "✗ Build failed" && exit 1

# ── 3. Install + launch ───────────────────────────────────────────────────────

echo ""
echo "=== Installing ==="
"$ADB" install -r "$APK"
[ $? -ne 0 ] && echo "✗ Install failed" && exit 1

echo ""
echo "=== Launching ==="
"$ADB" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1

echo ""
echo "✓ Done"

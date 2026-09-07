#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
APP="$ROOT/dist/macos/CryptoMonitor.app"
CONTENTS="$APP/Contents"
MACOS="$CONTENTS/MacOS"
RESOURCES="$CONTENTS/Resources"

rm -rf "$APP"
mkdir -p "$MACOS" "$RESOURCES"

swiftc "$ROOT/macos/CryptoMonitor.swift" \
  -framework Cocoa \
  -o "$MACOS/CryptoMonitor"

cp "$ROOT/config.example.json" "$RESOURCES/config.example.json"

cat > "$CONTENTS/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleExecutable</key>
  <string>CryptoMonitor</string>
  <key>CFBundleIdentifier</key>
  <string>com.cryptomonitor.CryptoMonitor</string>
  <key>CFBundleName</key>
  <string>CryptoMonitor</string>
  <key>CFBundleDisplayName</key>
  <string>CryptoMonitor</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>0.1.0</string>
  <key>CFBundleVersion</key>
  <string>1</string>
  <key>LSMinimumSystemVersion</key>
  <string>11.0</string>
  <key>LSUIElement</key>
  <true/>
</dict>
</plist>
PLIST

echo "Built $APP"

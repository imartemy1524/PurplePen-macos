#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_BIN="${DOTNET_BIN:-$HOME/.dotnet/dotnet}"
CONFIGURATION="${CONFIGURATION:-Release}"
RUNTIME_IDENTIFIER="${RUNTIME_IDENTIFIER:-osx-x64}"
OUTPUT_DIR="${OUTPUT_DIR:-$ROOT_DIR/artifacts/AvPurplePen-${RUNTIME_IDENTIFIER}}"
APP_DIR="${APP_DIR:-$ROOT_DIR/artifacts/PurplePen.app}"
ICONSET_DIR="${ICONSET_DIR:-$ROOT_DIR/artifacts/PurplePen.iconset}"
APP_ICON_FILE="${APP_ICON_FILE:-PurplePen.icns}"
APP_ICON_SOURCE="${APP_ICON_SOURCE:-$ROOT_DIR/src/AvPurplePen/Assets/PurplePenIcon.png}"
APP_NAME="${APP_NAME:-Purple Pen (ß)}"
EXECUTABLE_NAME="${EXECUTABLE_NAME:-AvPurplePen}"
IDENTITY="${CODESIGN_IDENTITY:-${1:--}}"

if [[ ! -x "$DOTNET_BIN" ]]; then
    echo "dotnet not found at: $DOTNET_BIN" >&2
    exit 1
fi

export PATH="$(dirname "$DOTNET_BIN"):$PATH"

cd "$ROOT_DIR"

echo "Restoring and publishing $APP_NAME for $RUNTIME_IDENTIFIER..."
"$DOTNET_BIN" restore src/AvPurplePen/AvPurplePen.csproj -r "$RUNTIME_IDENTIFIER" -v:minimal
"$DOTNET_BIN" publish src/AvPurplePen/AvPurplePen.csproj \
    -c "$CONFIGURATION" \
    -f net10.0 \
    -r "$RUNTIME_IDENTIFIER" \
    --self-contained true \
    --no-restore \
    -o "$OUTPUT_DIR"

echo "Assembling .app bundle..."
rm -rf "$APP_DIR"
rm -rf "$ICONSET_DIR"
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

echo "Generating macOS app icon from $APP_ICON_SOURCE..."
mkdir -p "$ICONSET_DIR"
sips -z 16 16 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_16x16.png" >/dev/null
sips -z 32 32 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_16x16@2x.png" >/dev/null
sips -z 32 32 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_32x32.png" >/dev/null
sips -z 64 64 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_32x32@2x.png" >/dev/null
sips -z 128 128 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_128x128.png" >/dev/null
sips -z 256 256 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_128x128@2x.png" >/dev/null
sips -z 256 256 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_256x256.png" >/dev/null
sips -z 512 512 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_256x256@2x.png" >/dev/null
sips -z 512 512 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_512x512.png" >/dev/null
sips -z 1024 1024 "$APP_ICON_SOURCE" --out "$ICONSET_DIR/icon_512x512@2x.png" >/dev/null
iconutil -c icns "$ICONSET_DIR" -o "$APP_DIR/Contents/Resources/$APP_ICON_FILE"

cat > "$APP_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>org.purplepen.AvPurplePen</string>
    <key>CFBundleExecutable</key>
    <string>$EXECUTABLE_NAME</string>
    <key>CFBundleIconFile</key>
    <string>${APP_ICON_FILE%.icns}</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>4.0.0.110</string>
    <key>CFBundleVersion</key>
    <string>4.0.0.110</string>
    <key>LSMinimumSystemVersion</key>
    <string>13.0</string>
</dict>
</plist>
EOF

cat > "$APP_DIR/Contents/PkgInfo" <<'EOF'
APPL???? 
EOF

cp -R "$OUTPUT_DIR"/. "$APP_DIR/Contents/MacOS/"

if [[ ! -f "$APP_DIR/Contents/MacOS/$EXECUTABLE_NAME" ]]; then
    echo "Expected executable not found: $APP_DIR/Contents/MacOS/$EXECUTABLE_NAME" >&2
    exit 1
fi

echo "Signing bundle with codesign..."
codesign --deep --force --sign "$IDENTITY" "$APP_DIR"

echo "Verifying signature..."
codesign --verify --deep --strict --verbose=2 "$APP_DIR"

echo "Done:"
echo "  App bundle: $APP_DIR"
echo "  Publish dir: $OUTPUT_DIR"

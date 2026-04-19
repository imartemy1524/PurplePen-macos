#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_BIN="${DOTNET_BIN:-$HOME/.dotnet/dotnet}"
CONFIGURATION="${CONFIGURATION:-Release}"
RUNTIME_IDENTIFIER="${RUNTIME_IDENTIFIER:-osx-arm64}"
OUTPUT_DIR="${OUTPUT_DIR:-$ROOT_DIR/artifacts/AvPurplePen-${RUNTIME_IDENTIFIER}}"
APP_DIR="${APP_DIR:-$ROOT_DIR/artifacts/PurplePen.app}"
APP_NAME="${APP_NAME:-PurplePen}"
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
mkdir -p "$APP_DIR/Contents/MacOS"

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

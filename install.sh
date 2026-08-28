#!/bin/bash
set -e

echo "👁️  Installing EyeSaver for macOS..."

TMP_DIR=$(mktemp -d)
ZIP_URL="https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver-v1.0.0-macOS.zip"

echo "⬇️  Downloading latest EyeSaver..."
curl -fsSL "$ZIP_URL" -o "$TMP_DIR/EyeSaver.zip"

echo "📦  Installing to /Applications..."
pkill -f "EyeSaver.app" 2>/dev/null || true
rm -rf /Applications/EyeSaver.app

ditto -xk "$TMP_DIR/EyeSaver.zip" "$TMP_DIR"
cp -R "$TMP_DIR/EyeSaver.app" /Applications/

echo "🛡️  Removing quarantine & setting permissions..."
xattr -cr /Applications/EyeSaver.app
chmod -R 755 /Applications/EyeSaver.app

rm -rf "$TMP_DIR"

echo "✅ EyeSaver successfully installed!"
echo "🚀 Starting EyeSaver..."
open /Applications/EyeSaver.app

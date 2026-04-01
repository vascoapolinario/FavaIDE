#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${1:-Release}"
OUTPUT_DIR="${2:-./publish/win-x64}"

dotnet publish ./src/FavaStudio/FavaStudio.csproj \
  -c "$CONFIGURATION" \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$OUTPUT_DIR"

echo "Done. Output: $OUTPUT_DIR"

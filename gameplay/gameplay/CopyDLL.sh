#!/usr/bin/env sh
set -e
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
OUT="$SCRIPT_DIR/bin/Output/netstandard2.1"
TARGET="$SCRIPT_DIR/../../client/Assets/Plugins"
mkdir -p "$TARGET"
cp "$OUT/game.gameplay.dll" "$TARGET/"
if [ -f "$OUT/game.gameplay.pdb" ]; then
  cp "$OUT/game.gameplay.pdb" "$TARGET/"
fi

#!/usr/bin/env sh
set -e
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
OUT="$SCRIPT_DIR/bin/Output/netstandard2.1"
TARGET="$SCRIPT_DIR/../../client/Assets/Packages/com.game.framework.bsk.core/Plugins"
mkdir -p "$TARGET"
cp "$OUT/game.core.dll" "$TARGET/"
if [ -f "$OUT/game.core.pdb" ]; then cp "$OUT/game.core.pdb" "$TARGET/"; fi
cp "$OUT/Game.Math.dll" "$TARGET/"
if [ -f "$OUT/Game.Math.pdb" ]; then cp "$OUT/Game.Math.pdb" "$TARGET/"; fi

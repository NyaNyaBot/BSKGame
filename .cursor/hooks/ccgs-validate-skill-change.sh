#!/bin/bash
# CCGS Hook: Advise testing after skill file changes
# Cursor event: afterFileEdit
# Fires when files in .cursor/skills/ccgs-* are modified

INPUT=$(cat)

if command -v jq >/dev/null 2>&1; then
    FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // .file_path // empty')
else
    FILE_PATH=$(echo "$INPUT" | grep -oE '"file_path"\s*:\s*"[^"]*"' | head -1 | sed 's/"file_path"\s*:\s*"//;s/"$//')
fi

FILE_PATH=$(echo "$FILE_PATH" | sed 's|\\|/|g')

if ! echo "$FILE_PATH" | grep -qE '(^|/)\.cursor/skills/ccgs-'; then
    exit 0
fi

SKILL_NAME=$(echo "$FILE_PATH" | grep -oE '\.cursor/skills/ccgs-[^/]+' | sed 's|\.cursor/skills/||')

if [ -z "$SKILL_NAME" ]; then
    exit 0
fi

echo "{\"additional_context\":\"CCGS skill '$SKILL_NAME' was modified. Consider validating structural compliance.\"}"
exit 0

#!/bin/bash
# CCGS Hook: Validate asset files after edits
# Cursor event: afterFileEdit
# Checks naming conventions and JSON validity for files in assets/ directory
#
# Cursor input: { "tool_name": "Write", "tool_input": { "file_path": "...", "content": "..." } }
# Cursor output: { "additional_context": "..." } or empty

INPUT=$(cat)

if command -v jq >/dev/null 2>&1; then
    FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // .file_path // empty')
else
    FILE_PATH=$(echo "$INPUT" | grep -oE '"file_path"\s*:\s*"[^"]*"' | head -1 | sed 's/"file_path"\s*:\s*"//;s/"$//')
fi

FILE_PATH=$(echo "$FILE_PATH" | sed 's|\\|/|g')

if ! echo "$FILE_PATH" | grep -qE '(^|/)assets/'; then
    exit 0
fi

FILENAME=$(basename "$FILE_PATH")
MESSAGES=""

if echo "$FILENAME" | grep -qE '[A-Z[:space:]-]'; then
    MESSAGES="NAMING: $FILE_PATH should be lowercase with underscores (got: $FILENAME)."
fi

if echo "$FILE_PATH" | grep -qE '\.json$'; then
    if [ -f "$FILE_PATH" ]; then
        PYTHON_CMD=""
        for cmd in python python3 py; do
            if command -v "$cmd" >/dev/null 2>&1; then
                PYTHON_CMD="$cmd"
                break
            fi
        done
        if [ -n "$PYTHON_CMD" ]; then
            if ! "$PYTHON_CMD" -m json.tool "$FILE_PATH" > /dev/null 2>&1; then
                MESSAGES="$MESSAGES FORMAT: $FILE_PATH is not valid JSON."
            fi
        fi
    fi
fi

if [ -n "$MESSAGES" ]; then
    SAFE=$(echo "$MESSAGES" | sed 's/"/\\"/g')
    echo "{\"additional_context\":\"Asset validation: $SAFE\"}"
fi
exit 0

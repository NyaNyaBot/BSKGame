#!/bin/bash
# CCGS Hook: Validate git commit commands
# Cursor event: beforeShellExecution (matcher: git\s+commit)
# Checks: GDD required sections, JSON validity, hardcoded gameplay values, TODO format
#
# Cursor input: { "command": "git commit -m ..." }
# Cursor output: { "permission": "allow", "agent_message": "..." }

INPUT=$(cat)

if command -v jq >/dev/null 2>&1; then
    COMMAND=$(echo "$INPUT" | jq -r '.command // empty')
else
    COMMAND=$(echo "$INPUT" | grep -oE '"command"\s*:\s*"[^"]*"' | sed 's/"command"\s*:\s*"//;s/"$//')
fi

if ! echo "$COMMAND" | grep -qE 'git\s+commit'; then
    echo '{"permission":"allow"}'
    exit 0
fi

STAGED=$(git diff --cached --name-only 2>/dev/null)
if [ -z "$STAGED" ]; then
    echo '{"permission":"allow"}'
    exit 0
fi

WARNINGS=""

DESIGN_FILES=$(echo "$STAGED" | grep -E '^design/gdd/')
if [ -n "$DESIGN_FILES" ]; then
    while IFS= read -r file; do
        if [[ "$file" == *.md ]] && [ -f "$file" ]; then
            for section in "Overview" "Player Fantasy" "Detailed" "Formulas" "Edge Cases" "Dependencies" "Tuning Knobs" "Acceptance Criteria"; do
                if ! grep -qi "$section" "$file"; then
                    WARNINGS="$WARNINGS DESIGN: $file missing section: $section."
                fi
            done
        fi
    done <<< "$DESIGN_FILES"
fi

DATA_FILES=$(echo "$STAGED" | grep -E '\.(json)$')
if [ -n "$DATA_FILES" ]; then
    PYTHON_CMD=""
    for cmd in python python3 py; do
        if command -v "$cmd" >/dev/null 2>&1; then
            PYTHON_CMD="$cmd"
            break
        fi
    done
    while IFS= read -r file; do
        if [ -f "$file" ] && [ -n "$PYTHON_CMD" ]; then
            if ! "$PYTHON_CMD" -m json.tool "$file" > /dev/null 2>&1; then
                echo "{\"permission\":\"deny\",\"user_message\":\"BLOCKED: $file is not valid JSON\"}"
                exit 0
            fi
        fi
    done <<< "$DATA_FILES"
fi

CODE_FILES=$(echo "$STAGED" | grep -E '\.(cs|gd|cpp|py|ts|js)$')
if [ -n "$CODE_FILES" ]; then
    while IFS= read -r file; do
        if [ -f "$file" ]; then
            if grep -nE '(damage|health|speed|rate|chance|cost|duration)\s*[:=]\s*[0-9]+' "$file" 2>/dev/null | head -1 >/dev/null 2>&1; then
                WARNINGS="$WARNINGS CODE: $file may have hardcoded gameplay values."
            fi
        fi
    done <<< "$CODE_FILES"
fi

SRC_FILES=$(echo "$STAGED" | grep -E '\.(cs|gd|cpp|py|ts|js)$')
if [ -n "$SRC_FILES" ]; then
    while IFS= read -r file; do
        if [ -f "$file" ]; then
            if grep -nE '(TODO|FIXME|HACK)[^(]' "$file" >/dev/null 2>&1; then
                WARNINGS="$WARNINGS STYLE: $file has TODO/FIXME without owner tag."
            fi
        fi
    done <<< "$SRC_FILES"
fi

if [ -n "$WARNINGS" ]; then
    SAFE_WARNINGS=$(echo "$WARNINGS" | sed 's/"/\\"/g' | tr '\n' ' ')
    echo "{\"permission\":\"allow\",\"agent_message\":\"Commit warnings:${SAFE_WARNINGS}\"}"
else
    echo '{"permission":"allow"}'
fi
exit 0

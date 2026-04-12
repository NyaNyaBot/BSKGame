#!/bin/bash
# CCGS Hook: Dump session state before context compaction
# Cursor event: preCompact
# Outputs critical state so it survives summarization

echo "=== CCGS SESSION STATE BEFORE COMPACTION ==="
echo "Timestamp: $(date)"

STATE_FILE="production/session-state/active.md"
if [ -f "$STATE_FILE" ]; then
    echo ""
    echo "## Active Session State"
    STATE_LINES=$(wc -l < "$STATE_FILE" 2>/dev/null | tr -d ' ')
    if [ "$STATE_LINES" -gt 80 ]; then
        head -n 80 "$STATE_FILE"
        echo "... (truncated — $STATE_LINES total lines)"
    else
        cat "$STATE_FILE"
    fi
fi

echo ""
echo "## Files Modified (git working tree)"
CHANGED=$(git diff --name-only 2>/dev/null)
STAGED=$(git diff --staged --name-only 2>/dev/null)
if [ -n "$CHANGED" ]; then
    echo "Unstaged:"
    echo "$CHANGED" | while read -r f; do echo "  - $f"; done
fi
if [ -n "$STAGED" ]; then
    echo "Staged:"
    echo "$STAGED" | while read -r f; do echo "  - $f"; done
fi
if [ -z "$CHANGED" ] && [ -z "$STAGED" ]; then
    echo "  (no uncommitted changes)"
fi

echo ""
echo "## Recovery: Read $STATE_FILE after compaction to restore context."
echo "=== END SESSION STATE ==="

SESSION_LOG_DIR="production/session-logs"
mkdir -p "$SESSION_LOG_DIR" 2>/dev/null
echo "Context compaction at $(date)" >> "$SESSION_LOG_DIR/compaction-log.txt" 2>/dev/null

exit 0

#!/bin/bash
# CCGS Hook: Log subagent completion for audit trail
# Cursor event: subagentStop

INPUT=$(cat)

if command -v jq >/dev/null 2>&1; then
    AGENT_TYPE=$(echo "$INPUT" | jq -r '.agent_type // .subagent_type // "unknown"' 2>/dev/null)
else
    AGENT_TYPE=$(echo "$INPUT" | grep -oE '"agent_type"\s*:\s*"[^"]*"' | sed 's/"agent_type"\s*:\s*"//;s/"$//')
    [ -z "$AGENT_TYPE" ] && AGENT_TYPE="unknown"
fi

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
SESSION_LOG_DIR="production/session-logs"
mkdir -p "$SESSION_LOG_DIR" 2>/dev/null

echo "$TIMESTAMP | Agent completed: $AGENT_TYPE" >> "$SESSION_LOG_DIR/agent-audit.log" 2>/dev/null

exit 0

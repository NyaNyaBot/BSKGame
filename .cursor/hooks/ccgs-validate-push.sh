#!/bin/bash
# CCGS Hook: Validate git push commands
# Cursor event: beforeShellExecution (matcher: git\s+push)
# Warns on pushes to protected branches
#
# Cursor input: { "command": "git push origin main" }
# Cursor output: { "permission": "allow/ask", "user_message": "..." }

INPUT=$(cat)

if command -v jq >/dev/null 2>&1; then
    COMMAND=$(echo "$INPUT" | jq -r '.command // empty')
else
    COMMAND=$(echo "$INPUT" | grep -oE '"command"\s*:\s*"[^"]*"' | sed 's/"command"\s*:\s*"//;s/"$//')
fi

if ! echo "$COMMAND" | grep -qE 'git\s+push'; then
    echo '{"permission":"allow"}'
    exit 0
fi

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)
MATCHED_BRANCH=""

for branch in develop main master; do
    if [ "$CURRENT_BRANCH" = "$branch" ]; then
        MATCHED_BRANCH="$branch"
        break
    fi
    if echo "$COMMAND" | grep -qE "\s${branch}(\s|$)"; then
        MATCHED_BRANCH="$branch"
        break
    fi
done

if [ -n "$MATCHED_BRANCH" ]; then
    echo "{\"permission\":\"ask\",\"user_message\":\"Push to protected branch '$MATCHED_BRANCH' detected. Ensure build passes and no S1/S2 bugs exist.\",\"agent_message\":\"Hook flagged push to protected branch '$MATCHED_BRANCH'.\"}"
else
    echo '{"permission":"allow"}'
fi
exit 0

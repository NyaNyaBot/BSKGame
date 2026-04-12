#!/bin/bash
# CCGS Hook: Detect documentation gaps at session start
# Cursor event: sessionStart
# Detects missing design docs when code/prototypes exist

set +e

HAS_GAPS=false

if [ -d "design/gdd" ]; then
    DESIGN_FILES=$(find design/gdd -type f -name "*.md" 2>/dev/null | wc -l | tr -d ' ')
else
    DESIGN_FILES=0
fi

SRC_FILES=0
for src_dir in src client/Assets/Game/Scripts; do
    if [ -d "$src_dir" ]; then
        count=$(find "$src_dir" -type f \( -name "*.cs" -o -name "*.gd" -o -name "*.cpp" -o -name "*.py" -o -name "*.ts" -o -name "*.js" \) 2>/dev/null | wc -l | tr -d ' ')
        SRC_FILES=$((SRC_FILES + count))
    fi
done

if [ "$SRC_FILES" -gt 50 ] && [ "$DESIGN_FILES" -lt 5 ]; then
    echo "GAP: Substantial codebase ($SRC_FILES source files) but sparse design docs ($DESIGN_FILES)."
    echo "  Suggestion: use ccgs-reverse-document skill to generate docs from code."
    HAS_GAPS=true
fi

if [ -d "prototypes" ]; then
    for proto_dir in prototypes/*/; do
        if [ -d "$proto_dir" ]; then
            if [ ! -f "${proto_dir}README.md" ] && [ ! -f "${proto_dir}CONCEPT.md" ]; then
                echo "GAP: Undocumented prototype: $proto_dir"
                HAS_GAPS=true
            fi
        fi
    done
fi

if [ "$SRC_FILES" -gt 100 ]; then
    if [ ! -d "production/sprints" ] && [ ! -d "production/milestones" ]; then
        echo "GAP: Large codebase ($SRC_FILES files) but no production planning found."
        echo "  Suggestion: use ccgs-sprint-plan skill."
        HAS_GAPS=true
    fi
fi

if [ "$HAS_GAPS" = false ]; then
    exit 0
fi

echo "Use ccgs-project-stage-detect for comprehensive analysis."
exit 0

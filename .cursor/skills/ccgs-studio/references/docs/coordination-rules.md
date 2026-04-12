# Agent Coordination Rules

1. **Vertical Delegation**: Leadership agents delegate to department leads, who
   delegate to specialists. Never skip a tier for complex decisions.
2. **Horizontal Consultation**: Agents at the same tier may consult each other
   but must not make binding decisions outside their domain.
3. **Conflict Resolution**: When two agents disagree, escalate to the shared
   parent. If no shared parent, escalate to `creative-director` for design
   conflicts or `technical-director` for technical conflicts.
4. **Change Propagation**: When a design change affects multiple domains, the
   `producer` agent coordinates the propagation.
5. **No Unilateral Cross-Domain Changes**: An agent must never modify files
   outside its designated directories without explicit delegation.

## Model routing (Cursor / Task subagents)

In Cursor, delegated work runs through the **Task** tool (subagents). The only
model hint in this stack is `model: "fast"` for a faster, lower-cost subagent;
**omit** `model` (or do not set `fast`) for the default subagent run, which should
be used for the heaviest reasoning.

| Route | Task invocation | When to use (maps from legacy Claude tiers) |
|-------|-----------------|----------------------------------------------|
| **Default** | Main chat, or Task **without** `model: "fast"` | Multi-document synthesis, high-stakes phase gates, cross-system holistic review — former **Opus** tier |
| **Fast** | Task with `model: "fast"` | Read-only status checks, formatting, simple lookups, implementation, design authoring, single-system analysis — former **Haiku** and **Sonnet** tiers |

Skills that should spawn **fast** subagents when delegating: `/help`, `/sprint-status`,
`/story-readiness`, `/scope-check`, `/project-stage-detect`, `/changelog`,
`/patch-notes`, `/onboard`, and **all other skills** unless listed below.

Skills that must use **default** (no `fast`): `/review-all-gdds`, `/architecture-review`,
`/gate-check`

When creating new skills: prefer **fast** for read-only formatting, narrow lookups,
and routine implementation; use **default** only if the skill must synthesize many
documents with high-stakes verdicts.

## Subagents vs Agent Teams

This project uses two distinct multi-agent patterns:

### Subagents (current, always active)
Spawned via **Cursor `Task`** within a session. Used by all `team-*` skills and
orchestration skills. Subagents share the session's permission context, run
sequentially or in parallel within the session, and return results to the parent.
Pass `model: "fast"` when the spawned work matches the **Fast** row above; omit it
for **Default** (leadership / gate / holistic review).

**When to spawn in parallel**: If two subagents' inputs are independent (neither
needs the other's output to begin), spawn both Task calls simultaneously rather
than waiting. Example: `/review-all-gdds` Phase 1 (consistency) and Phase 2
(design theory) are independent — spawn both at the same time.

### Agent Teams (experimental — opt-in)
Multiple independent Claude Code *sessions* running simultaneously, coordinated
via a shared task list. Each session has its own context window and token budget.
Requires `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` environment variable.

**Use agent teams when**:
- Work spans multiple subsystems that will not touch the same files
- Each workstream would take >30 minutes and benefits from true parallelism
- A senior agent (technical-director, producer) needs to coordinate 3+ specialist
  sessions working on different epics simultaneously

**Do not use agent teams when**:
- One session's output is required as input for another (use sequential subagents)
- The task fits in a single session's context (use subagents instead)
- Cost is a concern — each team member burns tokens independently

**Current status**: Not yet used in this project. Document usage here when first adopted.

## Parallel Task Protocol

When an orchestration skill spawns multiple independent agents:

1. Issue all independent Task calls before waiting for any result
2. Collect all results before proceeding to dependent phases
3. If any agent is BLOCKED, surface it immediately — do not silently skip
4. Always produce a partial report if some agents complete and others block

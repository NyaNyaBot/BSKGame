# BSKGame — Echo of Blades (回响之刃)

## Technology Stack

- **Engine**: Unity 2022.3.17f1
- **Language**: C#
- **Build System**: Unity Build Pipeline
- **Asset Pipeline**: Unity Asset Import Pipeline + Addressables

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## gstack

Use the `/browse` skill from gstack for all web browsing. Never use `mcp__claude-in-chrome__*` tools.

Available gstack skills: `/office-hours`, `/plan-ceo-review`, `/plan-eng-review`, `/plan-design-review`, `/design-consultation`, `/design-shotgun`, `/design-html`, `/review`, `/ship`, `/land-and-deploy`, `/canary`, `/benchmark`, `/browse`, `/connect-chrome`, `/qa`, `/qa-only`, `/design-review`, `/setup-browser-cookies`, `/setup-deploy`, `/setup-gbrain`, `/retro`, `/investigate`, `/document-release`, `/codex`, `/cso`, `/autoplan`, `/plan-devex-review`, `/devex-review`, `/careful`, `/freeze`, `/guard`, `/unfreeze`, `/gstack-upgrade`, `/learn`.

## GBrain Configuration (configured by /setup-gbrain)
- Engine: Supabase (Postgres + pgvector)
- Config file: ~/.gbrain/config.json (mode 0600)
- Setup date: 2026-05-03
- MCP registered: yes
- Memory sync: full
- Current repo policy: read-write

# Battle HUD UX Spec

> Status: Initial  
> Last Updated: 2026-04-29  
> Related GDD: `design/gdd/战斗 UI.md`  
> Accessibility Tier: Basic

## Purpose

The Battle HUD gives the player enough information to act confidently without replacing enemy telegraph readability. It shows HP, current phase, basic action availability, counter availability, pause/result states, and development diagnostics when enabled.

## Player Goals

- Know whether it is safe to choose an action.
- Know when parry input is armed without seeing exact countdown timing.
- Notice PerfectParry counter availability quickly.
- Track HP and low-health risk.
- Pause, retry, or exit without stale inputs leaking into battle.

## Layout Modules

| Module | Layer | Responsibility |
|---|---|---|
| `BattleHudWindow` | Window / Screen | Root battle HUD scope |
| `PlayerStatusPanel` | Panel | Player HP, low HP, controllability |
| `EnemyStatusPanel` | Panel | Current enemy summary |
| `ActionCommandPanel` | Panel | `BasicAttackAction` entry |
| `ParryTouchPanel` | Panel | Parry hit area registration and light state |
| `CounterPromptPanel` | Panel | Counter hit area and short availability cue |
| `BattleDebugPanel` | Panel | Development-only clock, phase, timestamp source, IDs |
| `BattleResultWindow` | Window / Overlay | Victory, defeat, retry, exit |

## State Model

| State | Trigger | UI Behavior |
|---|---|---|
| Hidden | No active battle | No combat hit areas registered |
| Entering | `BattleStarted` | Bind view model, register baseline HUD |
| PlayerCommand | `inputPhase=PlayerCommand` | Enable basic action button |
| EnemyAttack | `inputPhase=Parry` | Enable parry touch area; suppress action button |
| CounterAvailable | `CounterEntryOpened` | Show counter prompt; register priority 300 area |
| Suspended | Pause / background / transition | Disable battle hit areas; show overlay when player-facing |
| BattleResult | Victory / defeat | Show result window and retry/exit options |
| Disposed | Scene unload | Unregister all hit areas and subscriptions |

## Interaction Rules

- Formal HUD must not display exact parry window countdowns, QTE rings, or "press now" prompts.
- Parry and counter touch areas register through the input system.
- Counter hit area priority must be higher than parry.
- UI must refresh from events and immutable snapshots; it must not poll mutable gameplay objects every frame.
- Debug overlay must be disabled in release builds.

## Accessibility Requirements

- Parry and counter areas must be at least `88dp`.
- Critical states cannot be color-only:
  - Perfect: warm flash + hit stop + audio cue.
  - Normal: cooler block flash + lighter cue.
  - Failed: red/orange damage flash + HP change.
  - Counter: prompt highlight + distinct motion/audio.
- HUD text must remain readable on mobile screens.
- Pause overlay must prevent battle input and clearly show resume/exit paths.

## Test Evidence

| Evidence | Location |
|---|---|
| HUD lifecycle PlayMode test | `client/Assets/Tests/PlayMode/` |
| Hit area priority test | `tests/integration/input/` or PlayMode |
| Safe-area screenshots | `tests/evidence/` |
| Manual readability note | `tests/evidence/` |

## Open Follow-Ups

- Add final visual wireframe once combat prototype layout exists.
- Add damage number decision after HUD prototype playtest.
- Add left/right hand preference only if playtest shows reachability issues.

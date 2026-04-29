# Interaction Patterns

> Status: Initial  
> Last Updated: 2026-04-29  
> Applies To: MVP combat and Pre-Production prototypes

## Principles

1. **Touch is the only primary input.** Do not design hover, keyboard, mouse-right-click, or gamepad-only interactions for MVP.
2. **Timing inputs use PointerDown / touch start.** Parry and counter judgment must not depend on UGUI `onClick`.
3. **UI registers, input resolves.** UI owns layout and registration lifecycle; the input system owns hit testing, priority, occlusion, timestamps, and duplicate filtering.
4. **Readability comes from enemy and feedback cues.** Formal HUD must not show exact parry countdowns or QTE rings.
5. **Debug and player UI are separate.** Combat clock, window bounds, input source, and event IDs are development-only.

## Input Area Priority

| Priority | Area Type | Use |
|---:|---|---|
| 300 | Counter | PerfectParry counter entry |
| 250 | Modal / Pause Blocker | Pause, system overlay, blocking dialog |
| 200 | UI Button | Basic attack, pause, retry |
| 100 | Parry | Active parry touch area |
| 0 | Background | Non-interactive area |

Counter always wins over parry while counter is active. Parry must not set `blocksUnderlying=true` during an active counter window.

## Standard Patterns

### Basic Action Button

- Appears during `BattleInputPhase=PlayerCommand`.
- Uses UI button semantics for presentation, but sends a structured action request through the action/input boundary.
- Disabled state must explain the reason in debug builds.

### Parry Touch Area

- Enabled only during `BattleInputPhase=Parry`.
- Minimum size: `88dp`; preferred size: `112dp`.
- Does not display exact timing windows.
- Emits structured `ParryAttempt` with timestamp diagnostics.

### Counter Prompt

- Appears only after `CounterEntryOpened`.
- Registers a higher priority area than parry.
- Must remain reachable inside safe areas.
- Late input after expiry must be rejected by action service, not by UI visibility alone.

### Pause Overlay

- Disables battle hit areas and registers modal blockers.
- Resuming must rebuild hit areas from current battle/input phase, not from cached UI assumptions.

### Result Window

- Victory, defeat, retry, and exit actions are explicit.
- Retry must start a fresh scene/battle context and never reuse stale input or event subscriptions.

## Evidence Requirements

- Safe-area screenshots for at least two target mobile aspect ratios.
- Hit area priority test covering counter over parry.
- Development overlay evidence for timestamp source and ignored input reason.
- Manual playtest note confirming HUD does not obscure enemy read cues.

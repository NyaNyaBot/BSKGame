# ADR-0010: UIFrame 战斗 HUD 组成方式

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | UI / Input / Presentation |
| **Knowledge Risk** | LOW for Unity version; MEDIUM for UGUI safe area and WebGL mobile layout behavior |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0006-mobile-touch-timestamp-hit-area-strategy.md`, `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | PlayMode HUD lifecycle tests; hit area priority tests; safe-area device evidence; readability playtest |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | ADR-0001, ADR-0002, ADR-0006, ADR-0007, ADR-0009 |
| **Enables** | Battle HUD implementation, action/counter UI, parry touch area integration |
| **Blocks** | Battle UI implementation stories |
| **Ordering Note** | Must follow input ownership: UI owns layout and registration lifecycle; input system owns registry, priority, occlusion, and hit testing. |

## Context

### Problem Statement

The battle UI must show HP, phase, action buttons, counter availability, result state, and debug data while staying out of gameplay authority. It also creates the visual/touch surfaces players use for actions, parry, and counter, but hit testing and timestamp authority belong to the input system.

### Constraints

- Existing UI architecture uses UIFrame Window + Panel composition.
- Formal HUD must not display exact parry countdowns or QTE prompts.
- Mobile touch targets must respect safe areas and minimum dp sizes.
- UI events can arrive late after scene unload and must be context-filtered.

### Requirements

- `TR-ui-001`: render from events and snapshots, not mutable gameplay objects.
- `TR-ui-002`: register hit areas through input system; counter priority above parry.
- `TR-ui-003`: do not show exact parry countdown; readability comes from enemy/feedback cues.
- `TR-input-004`: input system owns hit area registry behavior.

## Decision

Implement battle HUD as a UIFrame Window/Screen composed of Panels. UI owns layout, view-model binding, panel lifecycle, and hit area registration/unregistration lifecycle. The input system owns the actual `InputHitAreaRegistry`, priority resolution, occlusion, hit testing, duplicate filtering, and timestamp mapping.

The formal HUD may show action availability, HP, phase-readable labels, recent parry result, and counter availability. It must not show exact millisecond parry windows, QTE rings, or "press now" prompts. Development builds may show debug timing overlays.

### Architecture Diagram

```text
Battle events/snapshots
   -> BattleHudPresenter
        -> BattleHudWindow
             -> PlayerStatusPanel
             -> EnemyStatusPanel
             -> ActionCommandPanel
             -> ParryTouchPanel
             -> CounterPromptPanel
             -> BattleResultWindow/Overlay
        -> InputHitArea registration lifecycle
             -> InputSystem registry owns hit test
```

### Key Interfaces

```csharp
public readonly struct BattleHudViewModel
{
    public readonly SceneEventContext Context;
    public readonly BattlePhase BattlePhase;
    public readonly BattleInputPhase InputPhase;
    public readonly int PlayerHp;
    public readonly int PlayerMaxHp;
    public readonly bool CanUseBasicAttack;
    public readonly bool CounterAvailable;
    public readonly ParryGrade? LastParryGrade;
}

public interface IBattleHudPresenter
{
    void Bind(BattleHudViewModel viewModel);
    void RegisterInputAreas(BattleHudInputLayout layout);
    void Dispose();
}
```

Rules:

- UI reads immutable view models and event DTOs only.
- UI must unregister all hit areas and subscriptions on scene unload/dispose.
- Counter area registers at priority `300`; parry area registers at priority `100`.
- When counter is active, parry area must not block the counter area.
- Debug overlays may show `CombatClockMs`, attack IDs, and window bounds only in development builds.
- UI cannot infer parry success from animation or button state.

## Alternatives Considered

### Alternative 1: Put all battle UI in one monolithic window

- **Description**: Single window class handles HP, buttons, parry touch, counter, and result.
- **Pros**: Simple initial setup.
- **Cons**: Harder to test, dispose, and iterate; hit area lifecycle becomes tangled.
- **Rejection Reason**: Existing UIFrame style favors Window + Panel composition.

### Alternative 2: Let UI panels perform hit testing directly

- **Description**: Each panel handles pointer callbacks and dispatches gameplay inputs.
- **Pros**: Familiar UGUI implementation.
- **Cons**: Conflicts with centralized input timestamp and priority ownership.
- **Rejection Reason**: Violates ADR-0006.

### Alternative 3: UIFrame composition with input registry integration

- **Description**: UI owns layout and registration lifecycle; input system owns hit testing.
- **Pros**: Matches project UI style and preserves input authority.
- **Cons**: Requires explicit view model and registry adapter code.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- HUD remains testable and event-driven.
- Counter/parry priority is centralized and verifiable.
- Formal UI stays focused and avoids replacing enemy readability.
- UI cleanup is tied to scene context disposal.

### Negative

- Requires view-model mapping from combat events.
- Debug and formal UI paths must stay separated.
- Designers cannot rely on exact countdown UI for timing clarity.

### Risks

- **Risk**: UI accidentally displays exact parry timing in production.
  - **Mitigation**: Gate debug overlay by build flag and test production HUD state.
- **Risk**: UI claims hit test authority.
  - **Mitigation**: Keep registry APIs in input system and assert registration-only UI behavior.
- **Risk**: Safe area shrinks counter/parry targets below minimum.
  - **Mitigation**: Device evidence and registration validation reject undersized areas.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `战斗 UI.md` | Render from events and snapshots | Defines `BattleHudViewModel` and event-driven presenter |
| `战斗 UI.md` | Register hit areas through input system | Defines UI registration lifecycle only |
| `战斗 UI.md` | No exact parry countdown in formal HUD | Bans QTE/countdown prompts outside debug |
| `输入系统.md` | Input owns hit area behavior | Keeps hit test authority in input system |
| `实时弹反系统.md` | Counter handoff requires UI/input coordination | Defines counter/parry priority expectations |

## Performance Implications

- **CPU**: Event-driven updates avoid per-frame polling of mutable gameplay state.
- **Memory**: View models and UI subscriptions are battle-scoped and must be disposed.
- **Load Time**: Battle HUD prefabs should be preloaded with battle scene UI.
- **Network**: Not applicable.

## Migration Plan

1. Define `BattleHudViewModel` and UI input layout DTOs.
2. Implement BattleHudWindow and panels using existing UIFrame patterns.
3. Integrate input area registration lifecycle with input system.
4. Add PlayMode tests for open/update/dispose and hit area priority.
5. Add device evidence for safe area and touch size.

## Validation Criteria

- HUD updates HP and phase from events/snapshots only.
- Counter area wins over parry area when both are present.
- Production HUD does not show exact parry window countdown.
- Scene unload disposes subscriptions and unregisters all hit areas.
- Safe-area tests keep parry and counter areas above minimum dp size.

## Related Decisions

- `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`
- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0006-mobile-touch-timestamp-hit-area-strategy.md`
- `docs/architecture/adr-0007-parry-timeline-counter-handoff.md`
- `design/gdd/战斗 UI.md`
- `design/gdd/输入系统.md`

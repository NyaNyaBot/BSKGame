# ADR-0012: 技能与行动服务边界

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | Gameplay / Combat Actions |
| **Knowledge Risk** | LOW |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`, `docs/architecture/adr-0005-damage-hp-ownership.md`, `docs/architecture/adr-0007-parry-timeline-counter-handoff.md`, `docs/architecture/adr-0011-character-schema-snapshot-ownership.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Pure C# action authorization tests; counter authorization tests; damage request idempotency integration tests |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | ADR-0002, ADR-0004, ADR-0005, ADR-0007, ADR-0011 |
| **Enables** | Basic attack, counter action, action UI integration stories |
| **Blocks** | Skill/action MVP implementation |
| **Ordering Note** | Requires damage ownership and counter authorization before action effects can be implemented. |

## Context

### Problem Statement

The MVP needs only `BasicAttackAction` and `CounterAction`, but these actions still touch phase authorization, target legality, idempotency, damage requests, UI input, and feedback. Without an action service boundary, UI, turn, parry, and damage could each start applying action consequences directly.

### Constraints

- MVP does not implement a skill tree, mana, cooldowns, equipment, or complex targeting.
- Actions must not write HP directly.
- `CounterAction` is valid only after `CounterEntryOpened`.
- Action timing/animation is not gameplay fact authority.
- Action service must run in pure C# and consume snapshots/DTOs.

### Requirements

- `TR-action-001`: MVP supports only `BasicAttackAction` and `CounterAction`.
- `TR-action-002`: actions submit `DamageRequest` and keep requests idempotent.
- `TR-action-003`: counter requires `CounterEntryOpened` authorization.
- `TR-dmg-001`: damage system remains the only HP write path.

## Decision

Create a pure C# `IBattleActionService` that owns action authorization, action request idempotency, target legality checks, action definition lookup, and conversion from valid action intent into `DamageRequest`. The action service never mutates HP, turn phase, or UI state directly.

MVP action catalog contains exactly:

- `BasicAttackAction`: allowed only during `BattleInputPhase.PlayerCommand`.
- `CounterAction`: allowed only while a current `CounterEntryOpened` authorization exists and the turn manager has entered `CounterWindow`.

### Architecture Diagram

```text
UI/Input -> BattleActionRequest
              |
              v
        BattleActionService
          phase + actor + target validation
          idempotency check
          action definition lookup
              |
              v
        DamageRequest -> DamageService
              |
              v
        Action events -> Turn/UI/Feedback
```

### Key Interfaces

```csharp
public interface IBattleActionService
{
    ActionSubmitResult Submit(BattleActionRequest request);
    ActionSubmitResult SubmitCounter(CounterActionRequest request);
}

public readonly struct ActionDefinition
{
    public readonly string ActionId;
    public readonly ActionKind Kind;
    public readonly int PowerMultiplierBps;
    public readonly long CommitDelayMs;
    public readonly bool RequiresCounterEntry;
    public readonly TargetRule TargetRule;
}
```

Rules:

- `BattleActionRequestId` is the action idempotency key.
- Valid actions emit action result events and at most one `DamageRequest`.
- Duplicate action requests return the original result and do not create new damage.
- Target legality comes from current `CharacterSnapshot`.
- `CounterAction` must reference the source `CounterEntryOpened` / `resolutionId`.
- Action animation completion cannot roll back committed damage.
- Future skills require extending the action catalog, not bypassing the service.

## Alternatives Considered

### Alternative 1: UI calls damage service directly

- **Description**: Action buttons construct `DamageRequest` themselves.
- **Pros**: Minimal code for MVP.
- **Cons**: UI becomes gameplay authority and counter authorization can be bypassed.
- **Rejection Reason**: Violates UI and damage ownership boundaries.

### Alternative 2: Turn manager owns all player actions

- **Description**: Turn manager validates and executes basic/counter actions.
- **Pros**: Phase information is local.
- **Cons**: Turn manager becomes too broad and starts owning action effects.
- **Rejection Reason**: Keeps turn focused on flow and time.

### Alternative 3: Dedicated action service

- **Description**: A pure service validates action intent and emits damage requests.
- **Pros**: Clear boundary, testable, and extensible for future skills.
- **Cons**: Adds a service and action DTOs for a small MVP action set.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- UI/input cannot bypass action rules.
- Counter authorization is explicit.
- Damage service remains the only HP write path.
- Future actions can share validation/idempotency infrastructure.

### Negative

- Requires action definitions even for two MVP actions.
- Turn/action integration must avoid duplicate phase authority.
- Counter expiry races need boundary tests.

### Risks

- **Risk**: Counter input arrives after counter expiry but before UI unregisters area.
  - **Mitigation**: Action service validates current counter authorization and context, not UI visibility.
- **Risk**: Duplicate action creates duplicate damage.
  - **Mitigation**: Action-level idempotency plus damage-level idempotency.
- **Risk**: Animation timing becomes effect authority.
  - **Mitigation**: Commit timing is domain data; animation only represents it.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `技能与行动系统.md` | MVP only BasicAttack and CounterAction | Defines MVP action catalog |
| `技能与行动系统.md` | Submit damage through `DamageRequest` | Defines service conversion to damage requests |
| `技能与行动系统.md` | Counter requires authorization | Requires current `CounterEntryOpened` reference |
| `伤害与生命系统.md` | Damage system owns HP mutation | Action service never writes HP |
| `战斗 UI.md` | UI submits action input but owns no facts | UI calls action service through DTOs |

## Performance Implications

- **CPU**: Constant-time action validation for MVP.
- **Memory**: Small battle-scoped idempotency cache.
- **Load Time**: Minimal action catalog load.
- **Network**: Not applicable.

## Migration Plan

1. Define action request/result DTOs and `ActionDefinition`.
2. Implement MVP action catalog.
3. Integrate turn phase and counter authorization queries.
4. Convert valid actions to `DamageRequest`.
5. Add action, counter, duplicate, and target invalid tests.

## Validation Criteria

- Basic attack is accepted only in `PlayerCommand`.
- Counter action is accepted only with current counter authorization.
- Duplicate action request does not create duplicate damage.
- Dead or untargetable targets reject action.
- Action service never writes HP directly.
- Action animation cancellation does not roll back committed damage.

## Related Decisions

- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/adr-0005-damage-hp-ownership.md`
- `docs/architecture/adr-0007-parry-timeline-counter-handoff.md`
- `docs/architecture/adr-0011-character-schema-snapshot-ownership.md`
- `design/gdd/技能与行动系统.md`

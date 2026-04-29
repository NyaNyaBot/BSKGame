# ADR-0005: 伤害与 HP 所有权

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility


| Field                     | Value                                                                                                                                                                                                             |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Engine**                | Unity 2022.3.17f1                                                                                                                                                                                                 |
| **Domain**                | Core / Combat State / Damage                                                                                                                                                                                      |
| **Knowledge Risk**        | LOW                                                                                                                                                                                                               |
| **References Consulted**  | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/architecture.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md` |
| **Post-Cutoff APIs Used** | None                                                                                                                                                                                                              |
| **Verification Required** | Pure C# idempotency tests; same-tick defeat ordering tests; integration tests for parry multiplier consumption                                                                                                    |


## ADR Dependencies


| Field             | Value                                                                                                      |
| ----------------- | ---------------------------------------------------------------------------------------------------------- |
| **Depends On**    | ADR-0002, ADR-0003                                                                                         |
| **Enables**       | Turn victory/defeat evaluation, parry damage resolution, basic/counter action damage                       |
| **Blocks**        | Any feature that mutates HP, applies attack damage, emits defeat events, or renders authoritative HP state |
| **Ordering Note** | Damage ownership must be accepted before parry, enemy, and action implementations submit real damage.      |


## Context

### Problem Statement

Parry, enemy attacks, player actions, UI, and feedback all need to react to damage outcomes, but only one system can own HP mutation. If parry directly reduces HP, enemy AI applies damage, and UI reads mutable objects, the project will quickly create double damage, missed deaths, and inconsistent battle results. The MVP needs a single deterministic damage service that applies all HP changes, clamps values, handles idempotency, and publishes observable damage/death events.

### Constraints

- Character data model owns stable identity and snapshots, but not arbitrary external HP writes.
- Damage must be deterministic and testable in pure C#.
- Duplicate events can occur after retry, low FPS, or repeated input; damage must be idempotent.
- Parry supplies a multiplier; it does not own final HP consequences.

### Requirements

- `TR-dmg-001`: damage/HP system is the only HP write entry point.
- `TR-dmg-002`: damage consumes idempotent `DamageRequest`.
- `TR-dmg-003`: damage precisely applies parry `damageMultiplier`.
- `TR-dmg-004`: damage emits `DamageApplied`, `DamageRejected`, and `CharacterDefeated`.
- `TR-char-003`: character queries include damage/target legality flags.
- `TR-char-004`: defeated character instance remains until explicit cleanup.
- `TR-turn-005`: turn manager reads HP state for victory/defeat and never writes HP.

## Decision

All HP mutation must go through `IDamageService.ApplyDamage(DamageRequest request)`. The damage system validates context, idempotency, target state, source state, and damage rules before modifying HP through the character repository's controlled mutation path. It publishes exactly one terminal result per `DamageRequestId`: `DamageApplied` or `DamageRejected`. If applied damage transitions a character to defeated, it also publishes `CharacterDefeated`.

No other system may write HP directly. Parry, enemy AI, action system, turn manager, UI, and feedback must submit requests or consume events/snapshots.

### Architecture Diagram

```text
Parry / Enemy / Action
  -> DamageRequest(requestId, context, source, target, baseDamage, multiplier)
       -> IDamageService
            -> context check
            -> duplicate request check
            -> target/source legality
            -> final damage calculation
            -> HP clamp and defeated transition
            -> DamageApplied / DamageRejected
            -> CharacterDefeated if needed
  -> Turn/UI/Feedback consume events and snapshots
```

### Key Interfaces

```csharp
public readonly struct DamageRequest
{
    public readonly string DamageRequestId;
    public readonly SceneEventContext Context;
    public readonly string SourceId;
    public readonly string TargetId;
    public readonly int BaseDamage;
    public readonly int DamageMultiplierBps;
    public readonly DamageSourceKind SourceKind;
    public readonly string SourceEventId;
}

public readonly struct DamageResult
{
    public readonly string DamageRequestId;
    public readonly DamageResultKind Kind;
    public readonly int FinalDamage;
    public readonly int HpBefore;
    public readonly int HpAfter;
    public readonly bool TargetDefeated;
    public readonly DamageRejectReason RejectReason;
}

public interface IDamageService
{
    DamageResult ApplyDamage(DamageRequest request);
}
```

Rules:

- `DamageRequestId` is globally unique within one battle context.
- Duplicate `DamageRequestId` returns the original result and never applies HP twice.
- Final damage is clamped to `>= 0`.
- HP is clamped to `[0, MaxHp]`.
- Defeat is emitted once per character defeat transition.
- Damage multipliers are represented as basis points to avoid float drift in deterministic tests.
- `PerfectParry` multiplier is `0`, `NormalParry` is `5000`, `FailedParry` is `10000`.
- If upstream GDD/event terminology uses `damageMultiplier` as `0.0 / 0.5 / 1.0`, the action/parry integration boundary converts it to `DamageMultiplierBps` before calling `IDamageService`.
- Damage system may reject requests for stale context, invalid target, already defeated target, duplicate mismatch, or illegal source.

## Alternatives Considered

### Alternative 1: Each combat feature applies its own damage

- **Description**: Enemy attacks, parry, counter, and skills directly modify HP.
- **Pros**: Feature-local code is simple.
- **Cons**: Double damage, inconsistent death ordering, and poor testability.
- **Rejection Reason**: Violates unique HP ownership.

### Alternative 2: Character model owns all damage formulas

- **Description**: Character repository exposes `TakeDamage` and owns damage calculation.
- **Pros**: Keeps HP near character data.
- **Cons**: Mixes storage/snapshot responsibility with combat consequence rules.
- **Rejection Reason**: Character data should expose facts and controlled mutation hooks, not own all battle consequences.

### Alternative 3: Dedicated damage service owns HP mutation

- **Description**: All systems submit `DamageRequest`; one service applies and publishes outcomes.
- **Pros**: Deterministic, idempotent, testable, and clear.
- **Cons**: Requires every damage source to construct valid requests.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- HP ownership is unambiguous.
- Parry and action systems stay focused on authorization and intent.
- UI and feedback can trust damage events as authoritative.
- Duplicate damage bugs become testable.

### Negative

- Damage source systems cannot take shortcuts.
- Character repository needs a controlled internal mutation path.
- More event/request DTOs are required.

### Risks

- **Risk**: A future feature writes HP directly for convenience.
  - **Mitigation**: Keep HP setter internal and enforce through code review/tests.
- **Risk**: Duplicate request IDs collide across systems.
  - **Mitigation**: Require source-prefixed IDs and battle context in idempotency key.
- **Risk**: Same-tick defeat ordering becomes ambiguous.
  - **Mitigation**: Use ADR-0004 deterministic ordering and test simultaneous defeat cases.
- **Risk**: Floating-point damage multipliers diverge across replay or later cross-platform validation.
  - **Mitigation**: Store multipliers as integer basis points in `DamageRequest`; presentation may still display percentages.

## GDD Requirements Addressed


| GDD System   | Requirement                                  | How This ADR Addresses It                                                           |
| ------------ | -------------------------------------------- | ----------------------------------------------------------------------------------- |
| `伤害与生命系统.md` | Unique HP write entry point                  | Defines `IDamageService` as the only HP mutation path                               |
| `伤害与生命系统.md` | Idempotent `DamageRequest`                   | Requires `DamageRequestId` and duplicate result reuse                               |
| `伤害与生命系统.md` | Consume parry `damageMultiplier`             | Defines multiplier field and accepted parry values                                  |
| `伤害与生命系统.md` | Emit damage and defeat events                | Defines `DamageApplied`, `DamageRejected`, and `CharacterDefeated` responsibilities |
| `角色数据模型.md`  | Defeated character remains until cleanup     | Damage sets defeated state but does not remove instances                            |
| `回合管理器.md`   | Read HP/death state for victory              | Turn consumes damage/defeat facts and does not write HP                             |
| `实时弹反系统.md`  | Parry does not write HP                      | Parry only contributes source event and multiplier                                  |
| `技能与行动系统.md` | Basic/counter actions submit damage requests | Action system cannot mutate HP directly                                             |


## Performance Implications

- **CPU**: Constant-time validation and simple formula calculation per request.
- **Memory**: Small idempotency cache per battle; clear on battle dispose.
- **Load Time**: No impact.
- **Network**: Not applicable.

## Migration Plan

1. Add `DamageRequest`, `DamageResult`, and `IDamageService` to pure gameplay layer.
2. Add battle-scoped idempotency cache.
3. Add controlled HP mutation hook in character repository/model.
4. Update parry, enemy, and action systems to submit damage requests.
5. Update UI/feedback/turn to consume events and snapshots.

## Validation Criteria

- Duplicate `DamageRequestId` applies HP once.
- `PerfectParry` damage multiplier produces zero damage.
- `NormalParry` applies half damage according to MVP formula.
- `FailedParry` applies full damage.
- `CharacterDefeated` is emitted once for a defeat transition.
- Turn manager can determine victory/defeat by reading snapshots/events only.

## Related Decisions

- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/architecture.md`
- `design/gdd/伤害与生命系统.md`
- `design/gdd/角色数据模型.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/技能与行动系统.md`


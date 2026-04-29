# ADR-0007: 弹反时间轴派生与 Counter Handoff

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | Gameplay / Timing / Input |
| **Knowledge Risk** | LOW for Unity version; HIGH for WebGL touch timing |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`, `docs/architecture/adr-0006-mobile-touch-timestamp-hit-area-strategy.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Pure timing boundary tests; low-FPS replay tests; WebGL/WeChat touch timestamp evidence; counter handoff input priority tests |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | ADR-0001, ADR-0002, ADR-0004, ADR-0005, ADR-0006 |
| **Enables** | ADR-0008, ADR-0010, ADR-0012, ADR-0013 |
| **Blocks** | Real-time parry implementation, counter action implementation, parry/counter integration stories |
| **Ordering Note** | Requires context routing, event DTOs, combat clock, damage ownership, and input timestamp mapping before implementation. |

## Context

### Problem Statement

The core combat mechanic depends on deriving reliable parry windows from enemy attack facts and resolving touch inputs against a deterministic combat clock. Existing ADRs define the clock, input timestamp mapping, event bus, and HP ownership, but no ADR owns the frozen parry timeline, result authority, or PerfectParry counter handoff.

### Constraints

- Parry judgment must not use Unity frame time or animation progress as rule authority.
- Enemy attack patterns provide content facts, but do not own final parry windows.
- The damage system owns HP mutation; parry only emits result and multiplier facts.
- Counter input overlaps with parry input and must have explicit handoff rules.
- WebGL low FPS can process input after the moment it occurred, so input timestamp must be authoritative.

### Requirements

- `TR-parry-001`: derive and freeze parry windows from `AttackSegmentTimelineSeed`.
- `TR-parry-002`: resolve `ParryAttempt` using `CombatClockMs`.
- `TR-parry-003`: emit resolved, cancelled, overlap, and timeline rejection events.
- `TR-parry-004`: only PerfectParry opens counter and parry never writes HP.
- `TR-ui-002`: counter input priority must be higher than parry input.

## Decision

Create a pure C# parry domain service that owns `FrozenAttackSegmentTimeline`, parry result resolution, and counter authorization events. The service accepts `AttackSegmentTimelineSeed`, validates timeline invariants, derives a frozen immutable timeline, and resolves at most one result for each `attackSegmentId + timelineSequenceId`.

`CounterEntryOpened` is produced only from `ParryResolved(PerfectParry)`. Counter input becomes valid from the next deterministic queue phase after `CounterEntryOpened` is drained. Same-tick touch events that happened before counter authorization remain parry/action inputs and must not be retroactively converted to counter input.

### Architecture Diagram

```text
Enemy Pattern -> Turn Manager -> AttackSegmentTimelineSeed
                                   |
                                   v
                          Parry Resolver
                    validates + freezes timeline
                                   |
Input System -> ParryAttempt ------+
                                   |
                                   v
                       ParryResolved / Rejected
                                   |
                Perfect only -> CounterEntryOpened
                                   |
                      Turn Manager enters CounterWindow
                                   |
              UI registers counter area through InputSystem
```

### Key Interfaces

```csharp
public readonly struct FrozenAttackSegmentTimeline
{
    public readonly SceneEventContext Context;
    public readonly string AttackSegmentId;
    public readonly int TimelineSequenceId;
    public readonly long WindupStartMs;
    public readonly long WindowOpenMs;
    public readonly long PerfectStartMs;
    public readonly long ParryCenterMs;
    public readonly long PerfectEndMs;
    public readonly long NormalEndMs;
    public readonly long DamageCommitMs;
    public readonly long RecoveryEndMs;
}

public interface IParryResolver
{
    TimelineValidationResult AcceptTimelineSeed(AttackSegmentTimelineSeed seed);
    ParryResolutionResult ResolveAttempt(ParryAttempt attempt);
    ParryResolutionResult ResolveNoInput(string attackSegmentId, int timelineSequenceId);
    void CancelSegment(string attackSegmentId, int timelineSequenceId, ParryCancelReason reason);
}
```

Rules:

- `FrozenAttackSegmentTimeline` is immutable after acceptance.
- Timeline validation failure emits `AttackSegmentTimelineRejected` and does not create a failed parry.
- A valid segment can emit exactly one `ParryResolved`, `ParryCancelled`, or `AttackSegmentTimelineRejected` terminal event.
- `PerfectParry` emits `damageMultiplier=0` and `CounterEntryOpened`.
- `NormalParry` emits `damageMultiplier=5000 bps` and no counter.
- `FailedParry` emits `damageMultiplier=10000 bps` and no counter.
- Counter closure is owned by the turn manager, not the parry resolver.
- Parry DTOs use stable gameplay IDs and primitive/pure math fields only.

## Alternatives Considered

### Alternative 1: Enemy attack patterns define exact parry windows

- **Description**: Pattern data includes `windowOpenMs`, `perfectStartMs`, and all final window fields.
- **Pros**: Designers can tune every attack directly.
- **Cons**: Makes enemy content own parry rules and allows invalid timing relationships.
- **Rejection Reason**: Violates parry ownership and makes deterministic validation weaker.

### Alternative 2: Unity animation events define windows

- **Description**: Animation clips or MonoBehaviours fire events to open/close parry windows.
- **Pros**: Easy visual authoring.
- **Cons**: Animation timing and frame processing become rule authority, which is unsafe under low FPS/WebGL.
- **Rejection Reason**: Fails `CombatClockMs` authority requirements.

### Alternative 3: Parry resolver derives frozen timelines from seed facts

- **Description**: Enemy content supplies segment facts; parry resolver derives and freezes windows.
- **Pros**: Deterministic, testable, and keeps ownership clear.
- **Cons**: Requires explicit profile data and validation tests.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Parry timing is deterministic and testable outside Unity.
- Enemy content cannot accidentally create invalid windows.
- Counter authorization has one event source.
- Damage and UI consume parry facts without owning judgment.

### Negative

- Attack pattern authors cannot hand-author every final window field.
- Counter handoff requires careful same-tick ordering tests.
- More DTOs and validation paths are required.

### Risks

- **Risk**: Counter touch is processed in the same tick as `CounterEntryOpened`.
  - **Mitigation**: Counter input is valid only after the queue phase that drains `CounterEntryOpened`; tests must cover same-tick boundary inputs.
- **Risk**: Latency compensation double-counts bridge delay.
  - **Mitigation**: Input handles bridge delay; parry applies only explicit `latency_compensation_ms`.
- **Risk**: Low FPS skips over window edges.
  - **Mitigation**: Resolve by mapped input timestamp and scheduled logical times, not frame edges.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `实时弹反系统.md` | Derive and freeze parry windows from seed facts | Defines `FrozenAttackSegmentTimeline` and validation ownership |
| `实时弹反系统.md` | Resolve using `CombatClockMs` | Requires mapped timestamps and read-only combat clock |
| `实时弹反系统.md` | Perfect opens counter and parry does not write HP | Defines `CounterEntryOpened` source and damage multiplier handoff |
| `输入系统.md` | Structured `ParryAttempt` with timestamp diagnostics | Consumes input DTO without using Unity click/frame timing |
| `战斗 UI.md` | Counter input priority above parry | Requires explicit post-Perfect counter handoff |

## Performance Implications

- **CPU**: Constant-time timeline validation and result calculation per active segment.
- **Memory**: One frozen timeline and small idempotency record per active segment.
- **Load Time**: No impact.
- **Network**: Not applicable.

## Migration Plan

1. Define timeline seed, frozen timeline, parry result, cancel, overlap, and counter DTOs in pure gameplay.
2. Implement `IParryResolver` with injected `ICombatClock`.
3. Add boundary tests for timeline invariants and result grades.
4. Integrate turn manager seed submission and counter state transitions.
5. Integrate input adapter and UI counter area registration.

## Validation Criteria

- Invalid timeline seed emits `AttackSegmentTimelineRejected`.
- Same input timestamp produces the same parry grade at 15, 30, and 60 FPS simulations.
- Each segment emits only one terminal parry event.
- PerfectParry emits `CounterEntryOpened` and zero damage multiplier.
- Same-tick counter boundary tests prove counter input is not valid before authorization.
- WebGL/WeChat device evidence records raw timestamp, bridge delay, battle timestamp, and timing offset.

## Related Decisions

- `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`
- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/adr-0005-damage-hp-ownership.md`
- `docs/architecture/adr-0006-mobile-touch-timestamp-hit-area-strategy.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/输入系统.md`
- `design/gdd/战斗 UI.md`

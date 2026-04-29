# ADR-0004: 战斗时钟与确定性战斗排序

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility


| Field                     | Value                                                                                                                                                                                                             |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Engine**                | Unity 2022.3.17f1                                                                                                                                                                                                 |
| **Domain**                | Core / Timing / Determinism                                                                                                                                                                                       |
| **Knowledge Risk**        | LOW for Unity version; HIGH for WebGL low-FPS and touch timing behavior                                                                                                                                           |
| **References Consulted**  | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/architecture.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md` |
| **Post-Cutoff APIs Used** | None                                                                                                                                                                                                              |
| **Verification Required** | EditMode deterministic ordering tests; low-FPS simulation; WebGL touch timestamp evidence                                                                                                                         |


## ADR Dependencies


| Field             | Value                                                                                   |
| ----------------- | --------------------------------------------------------------------------------------- |
| **Depends On**    | ADR-0002, ADR-0003                                                                      |
| **Enables**       | ADR-0006, P1 parry timeline ADR, P1 battle feedback hit stop ADR                        |
| **Blocks**        | Turn manager, parry timing, hit stop, and enemy attack scheduling implementation        |
| **Ordering Note** | Requires event contracts and pure gameplay layering before time control is implemented. |


## Context

### Problem Statement

The core mechanic depends on precise timing: enemy attack windows, touch input timestamps, parry grades, hit stop, counter windows, and turn phase transitions. If each system reads Unity frame time or advances time independently, low FPS, pause, hit stop, and delayed WebGL callbacks will produce non-deterministic outcomes. The game needs one authoritative logical combat clock and a deterministic ordering policy for events within the same tick.

### Constraints

- Unity frame time is not reliable enough for parry resolution under low FPS.
- WebGL callback timing may differ from the frame that processes the input.
- Hit stop should pause gameplay logic while allowing some unscaled presentation.
- Tests must be able to inject time and replay event sequences.

### Requirements

- `TR-turn-001`: turn manager owns `CombatClockMs` and battle phase state.
- `TR-turn-002`: turn manager publishes input phase changes.
- `TR-turn-003`: turn manager schedules enemy attack segments and timeline sequence identity.
- `TR-turn-004`: `CounterWindow` is a first-class battle state.
- `TR-input-003`: input maps platform timestamps into `CombatClockMs`.
- `TR-parry-002`: parry resolves attempts using `CombatClockMs`, not Unity frame time.
- `TR-fx-002`: feedback may request hit stop; turn manager controls clock pause.

## Decision

The turn manager owns the only mutable `CombatClockMs` through `ICombatClockController`. All other systems receive read-only `ICombatClock` or event timestamps. The clock advances only during active battle phases and is paused by battle pause, scene transition, and accepted hit stop. Input, parry, damage, UI, and feedback must not advance or rewind the clock.

Within the same `CombatClockMs` tick, event processing follows a deterministic priority order:

1. Scene/battle disposal and context invalidation
2. Pause/resume and hit stop state changes
3. Input phase changes
4. Input attempts mapped to battle time
5. Parry timeline validation and parry resolution
6. Damage requests and HP mutation
7. Death/victory/defeat evaluation
8. Counter window open/close
9. UI and feedback read-model updates
10. Debug/event trace recording

Events generated during a tick are appended to the ADR-0002 event bus queue and drained by this ordering policy. Handlers must not directly re-enter gameplay state mutation outside the queue. The turn manager / combat tick runner owns the gameplay drain loop; the event bus owns typed DTO delivery mechanics and stale-context filtering only. The drain loop must detect excessive recursive event production.

### Architecture Diagram

```text
Unity Update / Test Tick
  -> TurnManager.Advance(deltaMs)
       -> CombatClockMs advances if not paused
       -> scheduled enemy segments become active
       -> input phase changes are published
       -> parry/damage/action events are processed in deterministic order
       -> UI/feedback consume final facts
```

### Key Interfaces

```csharp
public enum CombatPauseReason
{
    None,
    SceneTransition,
    SystemPause,
    HitStop,
    BattleEnded
}

public interface ICombatClock
{
    long NowMs { get; }
    bool IsPaused { get; }
    CombatPauseReason PauseReason { get; }
}

public interface ICombatClockController : ICombatClock
{
    void Advance(long deltaMs);
    void Pause(CombatPauseReason reason);
    void Resume(CombatPauseReason reason);
    bool TryBeginHitStop(HitStopRequest request);
}

public readonly struct HitStopRequest
{
    public readonly string RequestId;
    public readonly SceneEventContext Context;
    public readonly int DurationMs;
    public readonly string SourceEventId;
    public readonly HitStopStrength Strength;
}
```

Rules:

- `Advance` clamps negative deltas to rejection and very large deltas to a configured maximum step.
- Hit stop pauses `CombatClockMs`; presentation may continue on unscaled Unity time.
- Hit stop requests are accepted only by turn manager.
- A parry result is never recalculated after hit stop starts.
- `OccurredAtCombatClockMs` records the logical event time, not Unity processing time.
- Tests may inject a fake clock/controller.
- Tick-local event queues have a maximum drain count in debug/test builds to detect event cycles.
- ADR-0002 event bus implementations must delegate gameplay drain ordering to this ADR's priority policy.

## Alternatives Considered

### Alternative 1: Use Unity `Time.time` / `deltaTime` directly everywhere

- **Description**: Systems read Unity time independently.
- **Pros**: Easy to implement.
- **Cons**: Non-deterministic under low FPS, pause, and hit stop; hard to test.
- **Rejection Reason**: Fails timing authority requirements.

### Alternative 2: Each system owns its own logical timer

- **Description**: Input, parry, feedback, and turn each track local elapsed time.
- **Pros**: Local logic is simple.
- **Cons**: Timers drift and disagree about boundary inputs.
- **Rejection Reason**: Violates one-owner rule for combat time.

### Alternative 3: Turn-owned combat clock with deterministic event ordering

- **Description**: Turn manager owns the controller; all systems consume read-only time or event timestamps.
- **Pros**: Deterministic, replayable, testable, and compatible with hit stop.
- **Cons**: Requires strict integration discipline.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Parry boundary tests can be deterministic.
- Hit stop cannot accidentally alter already-resolved outcomes.
- UI and feedback can stay visually rich while core time remains controlled.
- Replay/debug traces can be built around one clock.

### Negative

- Systems must be written around injected clock/time rather than Unity globals.
- Bugs in turn manager timing affect all combat systems.
- More test coverage is required around same-tick ordering.

### Risks

- **Risk**: A Unity adapter reads frame time and bypasses `CombatClockMs`.
  - **Mitigation**: Code review and tests for boundary DTOs; no Unity time in domain logic.
- **Risk**: Hit stop creates unexpected input buffering behavior.
  - **Mitigation**: Hit stop pauses logical input window progression; input events still carry mapped timestamp and are resolved only if phase/window is valid.
- **Risk**: Large frame spikes skip scheduled boundaries.
  - **Mitigation**: Process scheduled transitions by timestamp, not by frame edge.
- **Risk**: Recursive event production breaks deterministic same-tick ordering.
  - **Mitigation**: Drain tick-local event queues by declared priority and fail tests on recursion depth or drain count overflow.

## GDD Requirements Addressed


| GDD System   | Requirement                                                                       | How This ADR Addresses It                                                   |
| ------------ | --------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| `回合管理器.md`   | Own `CombatClockMs`, battle phases, input phase changes, attack segment lifecycle | Assigns mutable clock controller and deterministic ordering to turn manager |
| `输入系统.md`    | Map platform timestamps to `CombatClockMs`                                        | Requires input to output mapped battle timestamps instead of frame time     |
| `实时弹反系统.md`  | Resolve `ParryAttempt` using `CombatClockMs`                                      | Makes parry consume read-only clock/timestamps                              |
| `战斗反馈系统.md`  | Feedback can request hit stop; turn controls clock                                | Defines `HitStopRequest` and turn-manager acceptance                        |
| `技能与行动系统.md` | Counter window is time-bounded                                                    | Defines first-class tick ordering and clock-based expiration                |


## Performance Implications

- **CPU**: Small scheduling overhead per tick; deterministic trace should be optional in release builds.
- **Memory**: Event trace can grow; keep bounded ring buffer for runtime diagnostics.
- **Load Time**: No impact.
- **Network**: Not applicable.

## Migration Plan

1. Define `ICombatClock` and `ICombatClockController` in pure gameplay layer.
2. Implement turn-manager-owned clock and fake test clock.
3. Update input, parry, feedback, and UI contracts to accept read-only clock or event timestamps.
4. Add deterministic ordering tests for same-tick input/parry/damage/counter flows.
5. Add low-FPS and hit-stop PlayMode tests.

## Validation Criteria

- Same input timestamp produces same parry grade at 15, 30, and 60 FPS simulations.
- Hit stop pauses `CombatClockMs` and does not recalculate existing `ParryResolved`.
- Counter window expires by logical combat time.
- Turn manager is the only implementation that receives `ICombatClockController`.
- Event trace order matches the priority order defined in this ADR.

## Related Decisions

- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`
- `docs/architecture/architecture.md`
- `design/gdd/回合管理器.md`
- `design/gdd/输入系统.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/战斗反馈系统.md`


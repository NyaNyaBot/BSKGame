# ADR-0006: 移动触摸时间戳与输入触区策略

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility


| Field                     | Value                                                                                                                                                                                                                                                                                      |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Engine**                | Unity 2022.3.17f1                                                                                                                                                                                                                                                                          |
| **Domain**                | Input / UI / WebGL                                                                                                                                                                                                                                                                         |
| **Knowledge Risk**        | LOW for Unity version; HIGH for WeChat WebGL touch timestamp behavior                                                                                                                                                                                                                      |
| **References Consulted**  | `docs/engine-reference/unity/VERSION.md`, `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md`, `docs/architecture/architecture.md`, `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`, `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md` |
| **Post-Cutoff APIs Used** | None                                                                                                                                                                                                                                                                                       |
| **Verification Required** | WeChat/WebGL device evidence for timestamp source, bridge delay, fallback rate, safe area, and input-to-result latency                                                                                                                                                                     |


## ADR Dependencies


| Field             | Value                                                                                                     |
| ----------------- | --------------------------------------------------------------------------------------------------------- |
| **Depends On**    | ADR-0001, ADR-0002, ADR-0004                                                                              |
| **Enables**       | Parry implementation, Battle UI touch areas, counter input, mobile device evidence collection             |
| **Blocks**        | Any combat input implementation that feeds parry/counter/action systems                                   |
| **Ordering Note** | Requires context routing and combat clock definitions before timestamp mapping can be implemented safely. |


## Context

### Problem Statement

The MVP's core feel depends on precise touch timing. Unity UI click events are too late and too synthesized for parry resolution. WebGL/WeChat touch timestamps may have bridge delay or fallback behavior. UI and gameplay touch areas also overlap: counter input must override parry input after a PerfectParry, while normal HUD buttons must not steal parry touches unexpectedly. The project needs one touch timestamp adapter and one hit area registry with explicit priority and diagnostics.

### Constraints

- Target input is touch-only on WeChat Mini Game / WebGL mobile runtime.
- Hover, keyboard, and gamepad are not MVP inputs.
- Parry must use PointerDown/Touch timing rather than UGUI click synthesis.
- UI may register regions, but input system owns hit testing and priority resolution.
- Low-end devices and safe areas can change perceived timing and touch availability.

### Requirements

- `TR-input-001`: battle input comes from Touch / PointerDown, not UGUI click as the judgment source.
- `TR-input-002`: input outputs structured `ParryAttempt` with timestamp diagnostics.
- `TR-input-003`: platform timestamps map to `CombatClockMs` and mark fallback source.
- `TR-input-004`: input owns hit area registration, priority, occlusion, and duplicate filtering.
- `TR-ui-002`: battle UI registers hit areas through input system; counter priority is higher than parry.
- `TR-parry-002`: parry resolves using `CombatClockMs`.

## Decision

Create a battle input subsystem with two explicit responsibilities:

1. `TouchTimestampAdapter`: converts platform touch/PointerDown data into `battleTimestampMs` and diagnostics.
2. `InputHitAreaRegistry`: owns registered touch areas, priority, occlusion, state versions, duplicate filtering, and dispatch to domain input events.

UGUI click events may be used for normal UI buttons only when they do not feed combat timing judgments. Parry and counter judgment inputs must originate from touch/PointerDown sampling and include diagnostic metadata.

### Architecture Diagram

```text
WeChat/Unity Touch or PointerDown
  -> Unity Input Adapter
       -> TouchTimestampAdapter
            rawTimestampMs
            receivedRealtimeMs
            bridgeDelayMs
            timestampSource
            battleTimestampMs
       -> InputHitAreaRegistry
            priority / blocksUnderlying / stateVersion
       -> ParryAttempt or Counter input event
       -> Battle event bus
       -> Parry / Action system
```

### Key Interfaces

```csharp
public enum TimestampSource
{
    NativeTouchTimestamp,
    JsBridgeTimestamp,
    ReceivedFrameFallback,
    TestInjection
}

public readonly struct TouchTimestampSample
{
    public readonly long RawTimestampMs;
    public readonly long ReceivedRealtimeMs;
    public readonly long BridgeDelayMs;
    public readonly long BattleTimestampMs;
    public readonly TimestampSource Source;
}

public interface ITouchTimestampAdapter
{
    TouchTimestampSample MapToCombatClock(
        RawTouchSample raw,
        ICombatClock combatClock);
}

public readonly struct InputHitArea
{
    public readonly string AreaId;
    public readonly SceneEventContext Context;
    public readonly InputHitAreaKind Kind;
    public readonly int Priority;
    public readonly int StateVersion;
    public readonly bool BlocksUnderlying;
    public readonly RectDp Bounds;
}

public interface IInputHitAreaRegistry
{
    void Register(InputHitArea area);
    void Update(InputHitArea area);
    void Unregister(string areaId, int stateVersion);
    InputHitResult HitTest(ScreenPositionDp position);
}
```

Priority rules:

- Counter area priority: `300`
- Modal/pause/safe blocking area priority: `250`
- Standard UI action button priority: `200`
- Parry area priority: `100`
- Background/no-op area priority: `0`

Counter-specific rule:

- When `CounterEntryOpened` is active, counter area must have higher priority than parry.
- Parry area must not use `BlocksUnderlying=true` while counter area is active.
- Stale area versions are ignored.

Timestamp rules:

```text
bridgeDelayMs = max(0, receivedRealtimeMs - normalizedRawTimestampMs - calibratedBridgeOffsetMs)
battleTimestampMs = max(0, combatClock.NowMs - bridgeDelayMs)
```

`latency_compensation_ms` is not applied by the input system. It remains a parry/system tuning parameter and must not double-compensate bridge delay.

The adapter must normalize timestamp units and epoch before computing `bridgeDelayMs`. It must reject or clamp samples that are non-monotonic, negative after normalization, or above the configured maximum plausible bridge delay. Fallback behavior is part of the evidence contract: if `ReceivedFrameFallback` exceeds the configured threshold for a target device/build, parry tuning must either widen the relevant windows for that quality tier or block sign-off until the bridge is fixed.

## Alternatives Considered

### Alternative 1: Use UGUI `Button.onClick` for combat input

- **Description**: Treat parry/counter as normal UI button clicks.
- **Pros**: Easy with existing UI pipeline.
- **Cons**: Click synthesis is delayed and hides original touch timestamp.
- **Rejection Reason**: Fails parry timing requirements.

### Alternative 2: Let each UI panel handle its own touch logic

- **Description**: Battle HUD, counter prompt, and parry area each process their own pointer callbacks.
- **Pros**: Local UI code is simple.
- **Cons**: Priority and occlusion become inconsistent; counter/parry overlap bugs likely.
- **Rejection Reason**: Violates input ownership.

### Alternative 3: Central timestamp adapter and hit area registry

- **Description**: Input system owns time mapping and hit area resolution; UI only registers areas.
- **Pros**: Deterministic, testable, and aligned with GDD ownership.
- **Cons**: Requires UI/input contract and diagnostic logging.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Parry receives the earliest practical touch timestamp.
- Counter/parry priority is explicit and testable.
- Device timestamp issues are visible in logs/evidence.
- UI remains responsible for layout, not timing authority.

### Negative

- UI implementation must register/update/unregister hit areas carefully.
- More diagnostic fields are carried through `ParryAttempt`.
- WeChat/WebGL device testing is mandatory before tuning final windows.

### Risks

- **Risk**: Platform timestamp source is unavailable or inconsistent.
  - **Mitigation**: Normalize units/epoch, run monotonicity checks, mark `ReceivedFrameFallback`, log bridge delay, track fallback ratio, and tune or block release from device evidence.
- **Risk**: Counter area is blocked by parry area.
  - **Mitigation**: Enforce priority and `BlocksUnderlying` rules in registry tests.
- **Risk**: Safe area/system gestures reduce usable touch region.
  - **Mitigation**: Register bounds in dp/safe-area-adjusted coordinates and test on target devices.

## GDD Requirements Addressed


| GDD System  | Requirement                                                  | How This ADR Addresses It                               |
| ----------- | ------------------------------------------------------------ | ------------------------------------------------------- |
| `输入系统.md`   | Touch/PointerDown is the combat input source                 | Bans UGUI click as judgment source for parry/counter    |
| `输入系统.md`   | Structured `ParryAttempt` with diagnostics                   | Defines timestamp sample and required diagnostic fields |
| `输入系统.md`   | Map platform timestamps to `CombatClockMs`                   | Defines `ITouchTimestampAdapter` and mapping formula    |
| `输入系统.md`   | Own hit area priority, occlusion, duplicate filtering        | Defines central `IInputHitAreaRegistry`                 |
| `战斗 UI.md`  | UI registers input areas; counter priority > parry           | Defines priority constants and counter-specific rules   |
| `实时弹反系统.md` | Parry uses mapped combat time and avoids double compensation | Separates bridge delay from latency compensation        |


## Performance Implications

- **CPU**: Hit testing is O(number of active areas). MVP area count is small; keep registry sorted by priority.
- **Memory**: Small registry and diagnostic samples per input. Avoid unbounded input logs in release.
- **Load Time**: No impact.
- **Network**: Not applicable.

## Migration Plan

1. Define `ITouchTimestampAdapter`, `IInputHitAreaRegistry`, and DTOs in pure/adapter boundary.
2. Implement Unity touch/PointerDown adapter in hot-update Unity layer.
3. Update Battle UI to register parry, counter, and action areas through registry.
4. Update parry system to consume `ParryAttempt` with timestamp diagnostics.
5. Add WebGL/WeChat device evidence collection.

## Validation Criteria

- Parry input is generated from PointerDown/touch before UGUI click.
- `ParryAttempt` includes timestamp source, raw timestamp, received realtime, bridge delay, and battle timestamp.
- Counter area wins over parry area while counter window is active.
- Stale area `stateVersion` does not receive input.
- `ReceivedFrameFallback` is recorded when raw timestamp is unavailable.
- Fallback ratio and maximum plausible bridge delay are reported per target device/build.
- Device evidence records input-to-result latency on at least two target devices.

## Related Decisions

- `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`
- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/architecture.md`
- `design/gdd/输入系统.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/战斗 UI.md`


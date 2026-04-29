# ADR-0009: 战斗反馈质量分级与 WebGL 降级策略

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | Presentation / Performance / WebGL |
| **Knowledge Risk** | LOW for Unity version; HIGH for WeChat WebGL performance variance |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`, `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | PlayMode feedback idempotency tests; WebGL device FPS/GC/latency evidence; hit stop request integration tests |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | ADR-0002, ADR-0003, ADR-0004, ADR-0007 |
| **Enables** | Battle feedback implementation, Perfect/Normal/Failed readability validation, WebGL performance evidence |
| **Blocks** | Feedback/VFX/audio cue stories for MVP combat |
| **Ordering Note** | Requires event DTOs and combat clock authority before feedback can request hit stop safely. |

## Context

### Problem Statement

The combat loop depends on strong visual/audio feedback, but WebGL and WeChat Mini Game constraints make heavy VFX, camera shake, vibration, and audio cue overlap risky. Feedback must make parry outcomes readable without becoming rule authority or introducing frame spikes that change gameplay timing.

### Constraints

- Feedback consumes events and must never change parry grade, damage, HP, or turn order.
- Hit stop can pause `CombatClockMs` only if accepted by the turn manager.
- WebGL devices vary widely in GPU, memory, audio onset, and vibration support.
- MVP performance budgets are not yet fully configured.

### Requirements

- `TR-fx-001`: feedback only consumes events and does not modify battle facts.
- `TR-fx-002`: feedback may request hit stop; turn manager controls `CombatClockMs`.
- `TR-fx-003`: VFX and vibration may degrade by quality tier without changing results.
- `TR-concept-002`: presentation amplifies precision without becoming rule authority.

## Decision

Create a battle feedback presentation service in the Unity hot-update adapter layer. It consumes immutable battle events and builds presentation-only `FeedbackRequest` and `HitStopRequest` DTOs. Quality tiers cap or disable expensive feedback elements. Degradation can reduce particles, shake, screen flash, vibration, and audio cue density, but cannot modify combat clocks, parry windows, damage, HP, action eligibility, or event ordering.

Hit stop remains a request. The feedback service may build `HitStopRequest`; only the turn manager may accept it and pause `CombatClockMs`.

### Architecture Diagram

```text
Battle Event Bus facts
  -> BattleFeedbackService
       -> FeedbackProfile lookup
       -> quality tier clamp
       -> VFX / camera / audio / vibration adapters
       -> optional HitStopRequest
              |
              v
        TurnManager accepts or rejects hit stop
```

### Key Interfaces

```csharp
public readonly struct FeedbackRequest
{
    public readonly string FeedbackRequestId;
    public readonly SceneEventContext Context;
    public readonly string SourceEventId;
    public readonly FeedbackKind Kind;
    public readonly FeedbackIntensity Intensity;
    public readonly string ProfileId;
    public readonly string TargetActorId;
}

public readonly struct FeedbackQualityCaps
{
    public readonly FeedbackQualityTier Tier;
    public readonly int MaxParticlesPerBurst;
    public readonly bool AllowCameraShake;
    public readonly bool AllowVibration;
    public readonly int MaxConcurrentAudioCues;
    public readonly int MaxHitStopMs;
}
```

Rules:

- Feedback handlers drop stale `SceneEventContext`.
- Key feedback is idempotent by `SourceEventId`.
- Quality downgrade cannot alter gameplay facts.
- Hit stop duration is clamped by quality tier and ADR-0004 timing rules.
- Presentation may use unscaled Unity time while `CombatClockMs` is paused.
- Release builds keep bounded feedback logs only; diagnostic detail is for dev/test evidence.

## Alternatives Considered

### Alternative 1: Let each combat system play its own feedback

- **Description**: Parry, damage, action, and turn systems directly trigger VFX/audio.
- **Pros**: Simple local implementation.
- **Cons**: Couples presentation to gameplay facts and makes degradation inconsistent.
- **Rejection Reason**: Violates presentation separation.

### Alternative 2: Always play full-intensity feedback

- **Description**: Perfect/Normal/Failed always use configured full effects.
- **Pros**: Strongest visual impact on high-end devices.
- **Cons**: Unsafe for WebGL performance and memory variance.
- **Rejection Reason**: Fails platform budget risk management.

### Alternative 3: Event-driven feedback with quality caps

- **Description**: Feedback consumes events, maps profiles, and clamps output by tier.
- **Pros**: Keeps rule authority clean while supporting WebGL degradation.
- **Cons**: Requires device evidence and profile management.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Feedback remains powerful but non-authoritative.
- Low-end device degradation is explicit and testable.
- Hit stop is integrated without bypassing the combat clock owner.
- Feedback idempotency reduces duplicate VFX/audio bugs.

### Negative

- Requires quality tier heuristics and device profiling.
- Designers must author profiles with scalable intensity.
- Some low-end devices may get less spectacular feedback.

### Risks

- **Risk**: VFX or audio allocation creates GC spikes.
  - **Mitigation**: Pool runtime presentation objects and cap concurrent cue count.
- **Risk**: Hit stop request stacking creates long perceived freezes.
  - **Mitigation**: Quality caps and turn-manager acceptance rules limit duration and stacking.
- **Risk**: Degradation hides critical readability.
  - **Mitigation**: Critical telegraph and parry outcome cues must retain at least one visual or audio signal per tier.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `战斗反馈系统.md` | Feedback consumes events and never modifies facts | Defines event-driven presentation service |
| `战斗反馈系统.md` | Feedback may request hit stop | Defines request-only hit stop flow |
| `战斗反馈系统.md` | VFX and vibration can degrade without changing results | Defines quality caps and degradation invariants |
| `实时弹反系统.md` | Perfect/Normal/Failed must be readable | Requires profile mapping by parry grade |
| `game-concept.md` | Precision should feel rewarding | Keeps high-intensity feedback for Perfect within budgets |

## Performance Implications

- **CPU**: Profile lookup and request building are small; presentation adapters must be profiled.
- **Memory**: Requires bounded pools for VFX/audio request objects and logs.
- **Load Time**: Feedback profile assets may need preload for battle scenes.
- **Network**: Not applicable.

## Migration Plan

1. Define feedback request DTOs and quality tier data.
2. Implement event consumers in Unity hot-update adapter layer.
3. Add idempotency and stale-context checks.
4. Integrate hit stop request flow with turn manager.
5. Capture WebGL device evidence for FPS, GC spikes, audio onset, vibration support, and feedback downgrade behavior.

## Validation Criteria

- Duplicate parry/damage/action event does not replay key feedback.
- Low quality tier reduces particles/shake/vibration but leaves battle result unchanged.
- Hit stop request is ignored unless accepted by turn manager.
- WebGL device evidence records FPS and GC during repeated PerfectParry bursts.
- HUD/debug logs can show degradation reason in development builds.

## Related Decisions

- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/adr-0007-parry-timeline-counter-handoff.md`
- `design/gdd/战斗反馈系统.md`
- `design/gdd/实时弹反系统.md`

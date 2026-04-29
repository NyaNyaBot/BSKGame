# ADR-0002: 战斗事件总线与 DTO 版本策略

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility


| Field                     | Value                                                                                                                                          |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| **Engine**                | Unity 2022.3.17f1                                                                                                                              |
| **Domain**                | Core / Events / Data Contracts                                                                                                                 |
| **Knowledge Risk**        | LOW for Unity version; MEDIUM for WebGL callback timing and domain reload behavior                                                             |
| **References Consulted**  | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/architecture.md`, `docs/architecture/adr-0001-scene-lifecycle-context-routing.md` |
| **Post-Cutoff APIs Used** | None                                                                                                                                           |
| **Verification Required** | EditMode event ordering tests; PlayMode subscription cleanup tests; WebGL stale callback smoke test                                            |


## ADR Dependencies


| Field             | Value                                                                                                      |
| ----------------- | ---------------------------------------------------------------------------------------------------------- |
| **Depends On**    | ADR-0001                                                                                                   |
| **Enables**       | ADR-0004, ADR-0005, ADR-0006, all P1 combat feature ADRs                                                   |
| **Blocks**        | Any implementation that publishes battle events across input, turn, parry, damage, action, UI, or feedback |
| **Ordering Note** | Must be implemented after context routing so all event payloads can embed `SceneEventContext`.             |


## Context

### Problem Statement

The MVP combat loop depends on multiple systems reacting to the same facts: input phase changes, parry attempts and results, damage application, counter windows, enemy telegraphs, UI updates, and feedback requests. Direct cross-system calls would make ownership unclear and create cycles. A general-purpose mutable event bus without DTO discipline would also be unsafe, because stale events, duplicate damage, and schema drift would be hard to detect.

### Constraints

- Gameplay logic should stay pure C# and testable outside Unity scenes.
- WebGL and WeChat callbacks can arrive late relative to scene transitions.
- Some events are facts (`DamageApplied`), some are requests (`HitStopRequest`), and some are rejections (`ParryAttemptRejected`).
- Event consumers must not mutate payload objects shared by other systems.

### Requirements

- `TR-scene-002`: stale events must be rejected during scene transition.
- `TR-turn-002`: turn manager publishes explicit `BattleInputPhaseChanged`.
- `TR-dmg-004`: damage system emits `DamageApplied`, `DamageRejected`, and `CharacterDefeated`.
- `TR-parry-003`: parry emits resolved, cancelled, overlap, and timeline rejection events.
- `TR-fx-001`, `TR-ui-001`: presentation consumes events without owning gameplay facts.

## Decision

Use a lightweight, synchronous, in-process battle event bus for domain events within one Unity client. Events are immutable DTO structs/classes implementing `IBattleEvent`. All battle events carry `SceneEventContext`, `EventId`, `SchemaVersion`, and `OccurredAtCombatClockMs`. Events that can cause state mutation must include an idempotency key or source ID.

The bus is a delivery boundary, not an ownership boundary. The system that owns a fact publishes the authoritative event; consumers may update local read models or request follow-up work, but cannot write another system's owned state directly.

### Architecture Diagram

```text
Owner System
  -> creates immutable event DTO
  -> IBattleEventBus.Publish(evt)
       -> context filter
       -> schema/version guard
       -> synchronous handlers in deterministic subscription order
            -> consumers update read models or submit explicit requests
```

### Key Interfaces

```csharp
public interface IBattleEvent
{
    string EventId { get; }
    int SchemaVersion { get; }
    SceneEventContext Context { get; }
    long OccurredAtCombatClockMs { get; }
}

public interface IBattleEventBus
{
    void Publish<TEvent>(TEvent evt) where TEvent : IBattleEvent;
    IDisposable Subscribe<TEvent>(
        BattleEventHandler<TEvent> handler,
        BattleEventSubscriptionOptions options = default)
        where TEvent : IBattleEvent;
}

public readonly struct BattleEventSubscriptionOptions
{
    public readonly int Priority;
    public readonly bool DropStaleContext;
}

public delegate void BattleEventHandler<in TEvent>(TEvent evt)
    where TEvent : IBattleEvent;
```

Required DTO rules:

- DTOs are immutable after publish.
- DTOs use stable gameplay IDs, not Unity instance IDs.
- DTOs do not expose mutable domain objects.
- DTOs include `SchemaVersion`; MVP starts at `1`.
- Rejection and cancellation events are first-class events, not log strings.
- Request events must be named as requests, for example `HitStopRequest`, and the owning system must still decide whether to accept.

Deterministic delivery rules:

- The bus runs on the gameplay thread and provides typed publish/subscribe, context filtering, and enqueue APIs.
- ADR-0004 is the authority for combat tick-local drain order. The event bus must not own an independent gameplay drain loop.
- Events published during handler execution are enqueued, not immediately re-entered.
- The turn manager / combat tick runner drains queued events by the ADR-0004 priority policy, then enqueue order, within the current `CombatClockMs` tick.
- Publishers must not depend on UI/feedback handlers having completed to determine gameplay facts.
- Recursive publish depth and event count per tick must be guarded to detect loops.
- The event trace records enqueue order, drain order, and source event IDs.
- HybridCLR/AOT builds must preserve generic event bus call sites used by `Publish<T>` and `Subscribe<T>`.

## Alternatives Considered

### Alternative 1: Direct service calls between systems

- **Description**: Turn calls parry, parry calls damage, damage calls UI, and so on.
- **Pros**: Simple to follow for tiny prototypes.
- **Cons**: Creates dependency cycles and hides event ordering.
- **Rejection Reason**: Violates presentation separation and makes stale context rejection inconsistent.

### Alternative 2: UnityEvent / C# event fields per system

- **Description**: Each service exposes events directly.
- **Pros**: Familiar in Unity/C# code.
- **Cons**: Subscription cleanup and ordering are fragmented; testing full traces is harder.
- **Rejection Reason**: Does not provide a central context/version guard or event trace.

### Alternative 3: Lightweight typed battle event bus

- **Description**: A pure C# typed event bus with immutable DTOs, context filtering, and deterministic handler order.
- **Pros**: Testable, explicit, and aligned with cross-system GDD contracts.
- **Cons**: Adds boilerplate and requires discipline around event naming.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Cross-system communication becomes traceable and testable.
- UI and feedback can observe facts without owning them.
- Stale context rejection is centralized.
- Event schemas can evolve without silently breaking consumers.

### Negative

- Developers must define DTOs instead of passing domain objects.
- Misuse of event priority can create hidden ordering assumptions.
- More ceremony is required for small feature experiments.

### Risks

- **Risk**: Event bus becomes a dumping ground for mutable shared state.
  - **Mitigation**: Ban mutable payloads and Unity object references in DTOs.
- **Risk**: Recursive publish creates hard-to-debug chains.
  - **Mitigation**: Use a tick-local queue, recursion guards, and event trace assertions.
- **Risk**: Presentation logic starts depending on publish order.
  - **Mitigation**: Presentation handlers may not publish gameplay facts.
- **Risk**: HybridCLR/AOT generic stripping breaks `Publish<T>` / `Subscribe<T>` for event DTOs.
  - **Mitigation**: Maintain AOT generic reference coverage or link preservation for all event DTOs used in hot-update code; avoid dynamic code generation such as `Expression.Emit` or `DynamicMethod`.

## GDD Requirements Addressed


| GDD System   | Requirement                                                                     | How This ADR Addresses It                                         |
| ------------ | ------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `回合管理器.md`   | Publish `BattleInputPhaseChanged` and coordinate battle phases                  | Defines typed battle events and deterministic delivery            |
| `实时弹反系统.md`  | Emit `ParryResolved`, `ParryCancelled`, `OverlapRejected`, and rejection events | Makes cancellation/rejection first-class immutable DTOs           |
| `伤害与生命系统.md` | Emit `DamageApplied`, `DamageRejected`, `CharacterDefeated`                     | Defines authoritative owner events and idempotency expectations   |
| `战斗反馈系统.md`  | Consume events and request hit stop without changing gameplay facts             | Separates fact events from request events                         |
| `战斗 UI.md`   | Render from events and snapshots                                                | Provides event subscription with stale context filtering          |
| `场景管理.md`    | Reject late events after scene unload                                           | Requires `SceneEventContext` and drop-stale subscription behavior |


## Performance Implications

- **CPU**: O(number of subscribers for event type); expected low for MVP.
- **Memory**: Allocations depend on DTO implementation. Critical combat DTOs should prefer readonly structs or pooled immutable objects only when measured.
- **Load Time**: No meaningful impact.
- **Network**: Not applicable.

## Migration Plan

1. Define `IBattleEvent`, `IBattleEventBus`, and subscription options in pure C# gameplay/core layer.
2. Define DTOs for turn, parry, damage, action, UI, and feedback events.
3. Add context-filtered subscription helper.
4. Add event trace capture for tests and debug overlay.
5. Replace direct cross-presentation calls with event consumers.

## Validation Criteria

- `BattleInputPhaseChanged` reaches input and UI in deterministic order.
- Stale `ParryResolved` is dropped after scene version changes.
- Duplicate `DamageRequest` produces at most one `DamageApplied`.
- Feedback can request hit stop but cannot directly pause the combat clock.
- Event trace test verifies PerfectParry → CounterEntryOpened → Counter input flow.

## Related Decisions

- `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`
- `docs/architecture/architecture.md`
- `design/gdd/回合管理器.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/伤害与生命系统.md`
- `design/gdd/战斗反馈系统.md`
- `design/gdd/战斗 UI.md`


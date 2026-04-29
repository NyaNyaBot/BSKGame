# ADR-0001: 场景生命周期与场景上下文路由

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | Core / Scene / Runtime Lifecycle |
| **Knowledge Risk** | LOW for Unity version; MEDIUM for WebGL memory and scene cleanup behavior |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/architecture.md`, `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Unity PlayMode scene unload cleanup test; WebGL build smoke test for stale event rejection and memory release |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | None |
| **Enables** | ADR-0002, ADR-0004, ADR-0006, P1 combat feature ADRs |
| **Blocks** | Any battle scene implementation, combat event routing, and scene unload cleanup stories |
| **Ordering Note** | This ADR is the first context identity decision. Other ADRs must reuse its context fields instead of inventing parallel scene or battle version IDs. |

## Context

### Problem Statement

Combat, input, UI, feedback, and scene loading can all produce asynchronous or delayed events. On WebGL and WeChat Mini Game builds, scene unload, backgrounding, low frame rate, and delayed touch/input callbacks can cause events from an old scene or old battle to arrive after a new scene is already active. Without a shared context identity and lifecycle contract, those stale events can mutate the wrong battle, show old UI, replay old feedback, or apply damage after a scene transition.

### Constraints

- Unity scene objects may be destroyed while C# services, event subscriptions, or delayed callbacks still exist.
- WebGL builds have limited memory and no native threading; cleanup must be explicit and deterministic.
- Hot-update logic must not rely on Unity instance IDs as durable gameplay identity.
- MVP battle systems must support scene retry, battle cleanup, and late event rejection.

### Requirements

- `TR-scene-001`: scene management owns `SceneContext`, `sceneContextId`, and `sceneVersion`.
- `TR-scene-002`: scene switching freezes battle input and rejects late events.
- `TR-scene-003`: battle runtime state is created and destroyed within the battle scene lifecycle.
- `TR-turn-005`: turn manager reads battle outcome but does not own scene cleanup.
- `TR-ui-001` and `TR-fx-001`: UI and feedback must not keep stale rule state after scene unload.

## Decision

Create a `SceneContext` as the authoritative runtime scope for every playable scene and a nested `BattleContext` for each active battle. All cross-system battle events must carry context identity. Consumers must reject events whose context no longer matches the active scene/battle version before mutating local state or requesting gameplay changes.

### Architecture Diagram

```text
Unity Scene Load
  -> Scene Management creates SceneContext
       SceneContextId: stable per scene entry
       SceneVersion: increments on unload/retry/re-enter
  -> Battle scene creates BattleContext
       BattleContextId: stable per battle attempt
       BattleVersion: increments on dispose/restart
  -> Systems bind to SceneEventContext
  -> Scene unload freezes input, disposes battle, clears subscriptions, increments versions
  -> Late events are rejected by context mismatch
```

### Key Interfaces

```csharp
public readonly struct SceneContext
{
    public readonly string SceneContextId;
    public readonly int SceneVersion;
    public readonly SceneKind SceneKind;
    public readonly bool IsTransitioning;
}

public readonly struct BattleContext
{
    public readonly string BattleContextId;
    public readonly int BattleVersion;
    public readonly string SceneContextId;
    public readonly int SceneVersion;
}

public readonly struct SceneEventContext
{
    public readonly string SceneContextId;
    public readonly int SceneVersion;
    public readonly string BattleContextId;
    public readonly int BattleVersion;
}

public interface ISceneContextService
{
    SceneContext CurrentScene { get; }
    bool IsCurrent(SceneEventContext context);
    BattleContext CreateBattleContext(StartBattleRequest request);
    void DisposeBattle(string battleContextId, BattleDisposeReason reason);
    void BeginSceneTransition(SceneTransitionReason reason);
}
```

Rules:

- `SceneContextId` changes for every scene entry, including retry.
- `SceneVersion` increments before teardown side effects complete.
- `BattleVersion` increments when battle runtime state is disposed or restarted.
- Systems must not compare Unity `GameObject` references to decide whether an event is current.
- Scene management freezes combat input before disposing battle runtime state.

## Alternatives Considered

### Alternative 1: Use Unity scene name and active scene handle only

- **Description**: Systems check Unity's active scene name or handle before acting.
- **Pros**: Simple and close to Unity API.
- **Cons**: Does not distinguish retries of the same scene, delayed events from previous battle attempts, or multiple battle contexts in the same scene.
- **Rejection Reason**: Fails `sceneVersion` and late event rejection requirements.

### Alternative 2: Let each system own its own lifecycle flag

- **Description**: Input, UI, feedback, turn, and damage systems each track whether they believe a scene is active.
- **Pros**: Local implementation is easy.
- **Cons**: Creates divergent lifecycle truth and stale-subscription bugs.
- **Rejection Reason**: Violates the one-owner rule for mutable lifecycle facts.

### Alternative 3: Central `SceneContext` and nested `BattleContext`

- **Description**: Scene management owns context identity and versioning; all battle events carry those IDs.
- **Pros**: Deterministic stale event rejection, testable without Unity scene objects, compatible with WebGL delayed callbacks.
- **Cons**: Every event DTO becomes slightly larger and every consumer must perform context checks.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Stale input, damage, UI, and feedback events can be rejected uniformly.
- Scene retry and battle restart are explicit rather than inferred from object lifetime.
- Pure C# tests can simulate scene unload by incrementing versions.

### Negative

- All battle event DTOs must include context fields.
- Consumers must consistently call `IsCurrent` before changing local state.
- Debug logs need to include context IDs to be useful.

### Risks

- **Risk**: A consumer forgets the context check.
  - **Mitigation**: Provide shared event subscription helpers that drop stale events before invoking handlers.
- **Risk**: Scene transition order differs between Unity Editor and WebGL.
  - **Mitigation**: Require PlayMode and WebGL smoke tests for scene unload and retry.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `场景管理.md` | Own `SceneContext`, `sceneContextId`, `sceneVersion` | Defines `SceneContext` and authoritative ownership in scene management |
| `场景管理.md` | Freeze input and reject late events during transitions | Requires input freeze before teardown and `IsCurrent` checks |
| `场景管理.md` | Create/destroy battle runtime state in battle scene lifecycle | Defines nested `BattleContext` and `DisposeBattle` |
| `输入系统.md` | Scene/context fields on input DTOs | Requires all battle events and input events to carry `SceneEventContext` |
| `实时弹反系统.md` | Reject stale parry/counter events after scene change | Uses context mismatch rejection before parry resolution or counter handoff |
| `战斗 UI.md` | UI renders from current events and snapshots only | Gives HUD presenters a context boundary for subscriptions |

## Performance Implications

- **CPU**: Constant-time ID/version checks per event.
- **Memory**: Adds several primitive/string fields to event DTOs; acceptable for MVP event volume.
- **Load Time**: No meaningful impact.
- **Network**: Not applicable.

## Migration Plan

1. Introduce pure C# `SceneContext`, `BattleContext`, and `SceneEventContext`.
2. Add scene context service in Unity runtime adapter.
3. Update battle events and input DTOs to include `SceneEventContext`.
4. Add helper subscription wrappers that reject stale contexts.
5. Add scene unload/retry tests before implementing battle systems.

## Validation Criteria

- A stale `ParryAttempt` from a previous scene version is rejected.
- A stale `DamageRequest` from a disposed battle version cannot mutate HP.
- Scene unload clears input areas, UI subscriptions, feedback scope, active attack segment, and counter window.
- WebGL smoke test confirms scene retry does not replay old feedback or apply old damage.

## Related Decisions

- `docs/architecture/architecture.md`
- `design/gdd/场景管理.md`
- `design/gdd/输入系统.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/战斗 UI.md`

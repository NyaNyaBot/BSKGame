# ADR-0003: 玩法逻辑、Unity 适配器与 HybridCLR 的运行时程序集分层

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility


| Field                     | Value                                                                                                                                 |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| **Engine**                | Unity 2022.3.17f1                                                                                                                     |
| **Domain**                | Scripting / HybridCLR / Assembly Layering                                                                                             |
| **Knowledge Risk**        | LOW for Unity version; HIGH for HybridCLR + WebGL/AOT platform boundary                                                               |
| **References Consulted**  | `docs/engine-reference/unity/VERSION.md`, `AGENTS.md`, `.agents/skills/bsk-project-map/SKILL.md`, `docs/architecture/architecture.md` |
| **Post-Cutoff APIs Used** | None                                                                                                                                  |
| **Verification Required** | HybridCLR hot-update load smoke test; gameplay EditMode tests running without Unity scene objects; WebGL export smoke test            |


## ADR Dependencies


| Field             | Value                                                                                                 |
| ----------------- | ----------------------------------------------------------------------------------------------------- |
| **Depends On**    | None                                                                                                  |
| **Enables**       | ADR-0002, ADR-0004, ADR-0005, ADR-0006, all gameplay implementation stories                           |
| **Blocks**        | Any implementation that places core battle rules directly in Unity MonoBehaviours or AOT launch code  |
| **Ordering Note** | This ADR defines where code may live. Other ADRs may define interfaces but must follow this layering. |


## Context

### Problem Statement

BSKGame uses Unity 2022.3, GameFramework/Starforce style runtime, HybridCLR hot update, and WeChat WebGL export. If combat logic is implemented directly in Unity scene objects, UI prefabs, or launch/AOT code, the core rules become hard to test, hard to hot update safely, and fragile under WebGL/AOT constraints. The MVP combat systems need deterministic pure logic for timing, parry, damage, action, and event ordering while still integrating with Unity input, UI, VFX, audio, and scene lifecycle.

### Constraints

- Existing repository structure separates pure C# `game.core` / `game.gameplay` from Unity client scripts.
- HybridCLR introduces AOT metadata and hot-update assembly constraints.
- WeChat WebGL has no native threads and tighter memory/package constraints.
- Unity objects, prefabs, and scene references are unsuitable as durable gameplay IDs.

### Requirements

- `TR-concept-001`: support turn-based RPG combat with embedded real-time parry.
- `TR-concept-002`: presentation amplifies results but cannot become rule authority.
- `TR-parry-002`: parry uses `CombatClockMs`, not Unity frame time.
- `TR-dmg-001`: damage system is the only HP write path.
- `TR-input-001`: touch/PointerDown collection is platform-facing, but rule resolution is not.

## Decision

Adopt a three-layer runtime assembly model:

1. **Pure gameplay/domain layer**: deterministic rules, DTOs, state machines, clocks, event contracts, damage, parry, action, turn, and AI logic. In this repository this means the external pure C# `game.gameplay.dll` / `game.core.dll` style assemblies. This layer must not reference `UnityEngine`.
2. **Unity hot-update adapter layer**: MonoBehaviours, UI presenters, input adapters, feedback players, scene bridges, and service composition. In this repository this means Unity-side hot-update assemblies such as `BSK.Game.Client`, `BSK.Game.Gameplay.asmdef`, `Assembly-CSharp`, and related `client/Assets/Game/Scripts` code. This layer translates Unity/platform signals into pure gameplay DTOs and renders gameplay events.
3. **Launch/AOT layer**: bootstrapping, HybridCLR loading, AOT metadata, resource update flow, and platform SDK initialization. This layer must not contain combat rule logic.

Core gameplay services should be injectable through interfaces so EditMode tests can run without Unity scenes and PlayMode/WebGL tests can verify adapters.

### Architecture Diagram

```text
Launch / AOT
  - ProcedureLaunch, resource update, HybridCLR load
  - no combat rules
        |
        v
Unity Hot-Update Adapter Layer
  - MonoBehaviour lifecycle
  - Touch / UI / VFX / audio / scene adapters
  - translates to DTOs and consumes events
        |
        v
Pure Gameplay Domain Layer
  - turn, parry, damage, action, AI
  - DTOs, event bus interfaces, clocks
  - no UnityEngine references
```

### Key Interfaces

```csharp
public interface IGameplayServiceRegistry
{
    T Resolve<T>() where T : class;
}

public interface IUnityGameplayAdapter
{
    void BindScene(SceneContext sceneContext);
    void UnbindScene(SceneContext sceneContext);
}

public interface IPlatformInputAdapter
{
    void Enable(SceneEventContext context);
    void Disable(SceneEventContext context);
}
```

Layering rules:

- Pure gameplay assemblies may depend on `game.core` but not `UnityEngine`.
- Unity adapters may depend on pure gameplay interfaces and DTOs.
- Launch/AOT code may initialize and load assemblies but must not own battle state.
- DTOs crossing the boundary use primitives, strings, enums, and project pure math types.
- Unity instance IDs, `GameObject`, `Transform`, `MonoBehaviour`, `Vector2`, `AudioClip`, and `Material` must not appear in domain event DTOs.
- Generic services, event DTOs, and reflection-visible types used across HybridCLR boundaries must be covered by AOT generic references or link preservation.
- WebGL/AOT code must not rely on runtime code generation paths such as `Expression.Emit`, `DynamicMethod`, or unpreserved reflection constructors.

## Alternatives Considered

### Alternative 1: Implement battle rules in MonoBehaviours

- **Description**: Put parry, damage, turn, and input logic on Unity scene components.
- **Pros**: Fast prototyping and easy Inspector debugging.
- **Cons**: Hard to test deterministically, tightly coupled to scene lifetime, fragile under HybridCLR/AOT boundaries.
- **Rejection Reason**: Violates pure logic and hot-update requirements.

### Alternative 2: Put everything in hot-update Unity client assembly

- **Description**: Keep rule logic and Unity adapters in the same hot-update assembly.
- **Pros**: Simple dependency management.
- **Cons**: Encourages Unity API leakage into domain logic and weakens EditMode testability.
- **Rejection Reason**: Does not enforce architectural separation.

### Alternative 3: Pure gameplay domain with Unity adapter boundary

- **Description**: Keep deterministic rules in pure C# and isolate Unity/platform behavior in adapters.
- **Pros**: Testable, portable across Editor/WebGL, clear HybridCLR boundary.
- **Cons**: Requires DTO mapping and service composition code.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Combat logic can be tested without Unity scenes or prefabs.
- WebGL and HybridCLR platform quirks are isolated in adapters.
- Future server simulation, replay, or tooling can reuse pure gameplay logic.
- Presentation cannot accidentally own combat facts.

### Negative

- More boundary DTOs and mapping code are required.
- Unity developers must avoid convenient direct references in rule code.
- Debugging sometimes crosses adapter/domain boundaries.

### Risks

- **Risk**: Domain code gradually imports Unity types for convenience.
  - **Mitigation**: Add assembly definition constraints and lint/code review checks.
- **Risk**: Adapter mapping diverges from DTO contracts.
  - **Mitigation**: Add integration tests for input→parry and parry→feedback flows.
- **Risk**: HybridCLR AOT metadata issues appear late.
  - **Mitigation**: Add early WebGL/WeChat smoke build after service skeletons exist, maintain AOT generic references / link preservation for event bus and DTO types, and test hot-update assembly load before combat implementation expands.

## GDD Requirements Addressed


| GDD System        | Requirement                                                           | How This ADR Addresses It                                                  |
| ----------------- | --------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| `game-concept.md` | Turn-based RPG with embedded real-time parry                          | Places combat rules in a reusable pure domain layer                        |
| `实时弹反系统.md`       | Parry uses `CombatClockMs` and pure DTOs, not Unity frame time        | Prevents Unity frame APIs from becoming rule authority                     |
| `伤害与生命系统.md`      | Damage system is the only HP write path                               | Keeps HP authority in pure gameplay service                                |
| `战斗反馈系统.md`       | Feedback never modifies battle facts                                  | Restricts feedback to Unity adapter/presentation layer                     |
| `战斗 UI.md`        | UI renders state and registers input areas but owns no gameplay facts | Forces UI to consume events and produce DTO requests                       |
| `输入系统.md`         | Platform input converts to structured `ParryAttempt`                  | Places touch adapters in Unity layer and timestamp DTOs in domain contract |


## Performance Implications

- **CPU**: Boundary mapping has small overhead; domain tests can optimize hot paths before Unity integration.
- **Memory**: DTO allocations must be monitored, especially on WebGL. Critical event DTOs should avoid unnecessary heap churn.
- **Load Time**: HybridCLR assembly loading remains launch responsibility; domain assembly count should stay minimal.
- **Network**: Not applicable.

## Migration Plan

1. Confirm assembly definition boundaries for pure gameplay, Unity client, and launch/AOT.
2. Place domain interfaces and DTOs in pure C# assemblies.
3. Implement Unity adapters for scene, input, UI, and feedback.
4. Add tests that compile/run domain logic without Unity scene objects.
5. Add smoke build that verifies HybridCLR load and WebGL export still work.

## Validation Criteria

- Domain project compiles without `UnityEngine` references.
- Parry timing tests run in EditMode/pure NUnit with injected clock.
- Damage idempotency tests run without a scene.
- Unity adapter integration test converts PointerDown into `ParryAttempt`.
- WebGL build loads hot-update assemblies and enters battle scene without missing metadata errors.

## Related Decisions

- `docs/architecture/architecture.md`
- `docs/architecture/adr-0001-scene-lifecycle-context-routing.md`
- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/伤害与生命系统.md`
- `design/gdd/输入系统.md`
- `design/gdd/战斗反馈系统.md`


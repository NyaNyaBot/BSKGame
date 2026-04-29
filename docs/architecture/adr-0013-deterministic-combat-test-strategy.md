# ADR-0013: 确定性战斗逻辑测试策略

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | Testing / Determinism / WebGL Evidence |
| **Knowledge Risk** | LOW for Unity Test Framework basics; HIGH for WeChat device timing evidence |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`, `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`, `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | NUnit/EditMode pure domain tests; PlayMode adapter tests; WebGL/WeChat device evidence checklist |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | ADR-0002, ADR-0003, ADR-0004, ADR-0006, ADR-0007 |
| **Enables** | Story test evidence, smoke checks, pre-production gate validation |
| **Blocks** | QA handoff for combat MVP stories |
| **Ordering Note** | Test strategy follows core contracts and must be referenced by implementation stories. |

## Context

### Problem Statement

The MVP's main risk is not only whether systems compile, but whether timing-sensitive combat stays deterministic across low FPS, hit stop, scene transitions, duplicate input, HybridCLR/WebGL boundaries, and device timestamp variance. Current documents mention tests, but no ADR defines which evidence is required before stories can be considered complete.

### Constraints

- Pure gameplay logic should run without Unity scenes.
- Unity adapters still need PlayMode coverage for UI/input lifecycle.
- WebGL/WeChat timing behavior cannot be fully proven in EditMode.
- Performance budgets and minimum coverage are not yet fully configured.

### Requirements

- All core logic must be deterministic under fixed inputs and seeds.
- Parry boundary tests must use `CombatClockMs`.
- Input timestamp and safe area behavior need device evidence.
- Scene unload and stale event rejection need integration tests.
- HybridCLR/WebGL smoke checks must happen early enough to catch AOT issues.

## Decision

Use a three-layer test evidence strategy:

1. **Pure domain tests**: NUnit/EditMode tests for pure C# gameplay services, fake clocks, deterministic RNG, DTO validation, parry timing, damage, turn ordering, action validation, and enemy pattern selection.
2. **Unity adapter tests**: PlayMode tests for scene lifecycle, input area registration, UI lifecycle, feedback request routing, hit stop integration, and stale subscription cleanup.
3. **WebGL/WeChat evidence**: manual or automated device evidence for touch timestamp source, fallback ratio, bridge delay, input-to-result latency, safe area hit targets, feedback FPS/GC behavior, and HybridCLR hot-update load.

Automated tests are required for deterministic rules. Device evidence is required for platform timing and performance claims.

### Architecture Diagram

```text
Pure Gameplay Tests
  - fake clock
  - fixed seed
  - DTO/event trace assertions

Unity PlayMode Tests
  - adapters
  - UI/input lifecycle
  - scene unload cleanup

WebGL/WeChat Evidence
  - timestamp source
  - latency/fallback
  - safe area
  - feedback performance
```

### Key Interfaces

```csharp
public interface ICombatTestClock : ICombatClockController
{
    void SetNow(long nowMs);
}

public readonly struct CombatReplayInput
{
    public readonly uint Seed;
    public readonly IReadOnlyList<IBattleEvent> InitialEvents;
    public readonly IReadOnlyList<TimedInputSample> Inputs;
}

public readonly struct CombatReplayResult
{
    public readonly IReadOnlyList<IBattleEvent> EventTrace;
    public readonly BattleResult FinalResult;
}
```

Rules:

- Domain tests cannot depend on Unity scene objects.
- Any timing-sensitive story needs boundary tests at early edge, perfect edge, late edge, and no-input failure.
- Any story touching event ordering must assert event trace order.
- Any story touching WebGL input must include device evidence or explicitly mark the gap.
- `ReceivedFrameFallback` cases must be tested, not treated as impossible.
- Smoke tests must include HybridCLR hot-update load before large combat implementation expands.

## Alternatives Considered

### Alternative 1: Rely mostly on manual playtesting

- **Description**: Validate parry feel through manual sessions only.
- **Pros**: Good for subjective feel.
- **Cons**: Cannot prove deterministic boundaries, idempotency, or stale event rejection.
- **Rejection Reason**: Insufficient for timing-critical architecture.

### Alternative 2: Only pure unit tests

- **Description**: Cover all gameplay logic with EditMode/pure tests.
- **Pros**: Fast, deterministic, CI-friendly.
- **Cons**: Cannot verify Unity adapter, safe area, WebGL timestamp, or HybridCLR behavior.
- **Rejection Reason**: Platform risk remains untested.

### Alternative 3: Layered automated tests plus device evidence

- **Description**: Use pure, PlayMode, and WebGL/WeChat evidence according to risk.
- **Pros**: Matches architecture layers and separates deterministic logic from platform proof.
- **Cons**: Requires evidence discipline and test fixtures.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Timing and ownership rules become enforceable.
- Platform evidence gaps are visible before QA handoff.
- Stories can reference consistent test expectations.
- Deterministic replay supports debugging parry/turn bugs.

### Negative

- More up-front test scaffolding is required.
- WebGL/WeChat evidence may slow iteration.
- Performance/test budgets still need final numeric targets.

### Risks

- **Risk**: Device evidence is skipped because automated tests pass.
  - **Mitigation**: Story acceptance criteria must distinguish automated coverage from platform evidence.
- **Risk**: Tests overfit one implementation.
  - **Mitigation**: Assert contracts and event traces, not private implementation details.
- **Risk**: HybridCLR AOT issues appear late.
  - **Mitigation**: Add early WebGL/hot-update smoke tests for DTO/event generic usage.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `回合管理器.md` | Deterministic battle flow and event order | Requires event trace and fake clock tests |
| `实时弹反系统.md` | Boundary timing and device evidence | Requires parry boundary and WebGL timestamp evidence |
| `输入系统.md` | Touch timestamp diagnostics and fallback | Requires fallback and bridge delay evidence |
| `伤害与生命系统.md` | Damage idempotency and HP clamp | Requires pure service tests |
| `战斗反馈系统.md` | WebGL degradation without gameplay changes | Requires performance/evidence checks |
| `战斗 UI.md` | Safe area and hit area behavior | Requires PlayMode and device validation |

## Performance Implications

- **CPU**: Test-only overhead; release builds should disable verbose traces.
- **Memory**: Event traces must use bounded buffers in runtime diagnostics.
- **Load Time**: CI and WebGL smoke builds increase validation time.
- **Network**: Not applicable.

## Migration Plan

1. Create test helper fixtures for fake clock, deterministic RNG, character factory, and event trace assertions.
2. Add pure tests for clock, parry, damage, actions, enemy selection, and event ordering.
3. Add PlayMode tests for scene/input/UI/feedback lifecycle.
4. Define WebGL/WeChat evidence template and required metrics.
5. Add HybridCLR/WebGL smoke test to milestone gate checklist.

## Validation Criteria

- Pure test suite can run without Unity scene objects.
- Parry boundary tests pass at simulated 15, 20, 30, and 60 FPS.
- Event trace order matches ADR-0004.
- Duplicate input/damage/action idempotency tests pass.
- WebGL evidence records timestamp source, fallback ratio, bridge delay, safe area, and feedback performance.
- HybridCLR hot-update smoke test confirms event DTO/generic call preservation.

## Related Decisions

- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/adr-0006-mobile-touch-timestamp-hit-area-strategy.md`
- `docs/architecture/adr-0007-parry-timeline-counter-handoff.md`
- `design/gdd/回合管理器.md`
- `design/gdd/实时弹反系统.md`
- `design/gdd/输入系统.md`

# ADR-0008: 敌方攻击模式数据与调度所有权

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility


| Field                     | Value                                                                                                                                                                                                                                          |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Engine**                | Unity 2022.3.17f1                                                                                                                                                                                                                              |
| **Domain**                | Gameplay / AI / Combat Data                                                                                                                                                                                                                    |
| **Knowledge Risk**        | LOW                                                                                                                                                                                                                                            |
| **References Consulted**  | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`, `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`, `docs/architecture/adr-0007-parry-timeline-counter-handoff.md` |
| **Post-Cutoff APIs Used** | None                                                                                                                                                                                                                                           |
| **Verification Required** | Deterministic pattern selection tests; seed completeness tests; overlap/counter response integration tests                                                                                                                                     |


## ADR Dependencies


| Field             | Value                                                                                                            |
| ----------------- | ---------------------------------------------------------------------------------------------------------------- |
| **Depends On**    | ADR-0002, ADR-0004, ADR-0005, ADR-0007, ADR-0011                                                                 |
| **Enables**       | Enemy attack implementation, parry integration, combat encounter data authoring                                  |
| **Blocks**        | Enemy AI and attack pattern stories                                                                              |
| **Ordering Note** | Requires parry timeline and character snapshot contracts so attack plans can be validated without Unity objects. |


## Context

### Problem Statement

Enemy attacks drive parry timing, damage requests, feedback cues, and turn progression. The GDD requires scripted, readable, deterministic enemy patterns for MVP. Existing ADRs say the turn manager schedules attack segments and the parry system owns final window derivation, but no ADR defines what the enemy system owns or how pattern choice remains deterministic.

### Constraints

- MVP avoids GOAP, behavior trees, and learning AI.
- Enemy logic must run in pure C# without Unity scene object references.
- Pattern selection must be deterministic for tests and replay.
- Enemy content facts must not become timeline sequence authority.
- Rejected overlap must never become an unavoidable hit.

### Requirements

- `TR-enemy-001`: choose scripted attack patterns by weight, cooldown, HP phase, round, and target legality.
- `TR-enemy-002`: own content facts such as base damage, windup timing, cue IDs, and combo interrupt rules.
- `TR-enemy-003`: respond to parry, cancel, overlap, and counter events without converting rejected attacks to guaranteed hits.
- `TR-turn-003`: turn manager owns active segment scheduling and timeline identity.

## Decision

Create an `IEnemyAttackPlanner` and `IAttackPatternCatalog` in the pure gameplay layer. Enemy attack patterns own content facts and selection policy. The turn manager owns scheduling facts: `attackSegmentId`, `timelineSequenceId`, active segment lifecycle, and phase transitions.

Pattern selection uses an injected deterministic RNG or fixed test seed. It must not use Unity `Random`, wall-clock time, frame count, ScriptableObject instance identity, or scene object state as rule input. Unity assets may author pattern data, but runtime domain DTOs must be converted to stable IDs and primitive values before entering gameplay logic.

### Architecture Diagram

```text
TurnManager -> EnemyAttackPlanner.Query
                  |
                  v
          PatternCatalog + CharacterSnapshot
                  |
          deterministic selection
                  |
                  v
           EnemyActionPlan content facts
                  |
                  v
TurnManager assigns attackSegmentId + timelineSequenceId
                  |
                  v
ParryResolver accepts AttackSegmentTimelineSeed
```

### Key Interfaces

```csharp
public interface IEnemyAttackPlanner
{
    EnemyActionPlan SelectAction(EnemyActionQuery query);
    EnemyPatternUpdateResult NotifyEvent(IBattleEvent evt);
}

public readonly struct EnemyActionQuery
{
    public readonly SceneEventContext Context;
    public readonly string EnemyInstanceId;
    public readonly int RoundIndex;
    public readonly CharacterSnapshot Enemy;
    public readonly IReadOnlyList<CharacterSnapshot> CandidateTargets;
    public readonly uint DeterministicSeed;
}

public readonly struct AttackSegmentDefinition
{
    public readonly string SegmentLocalId;
    public readonly int ComboIndex;
    public readonly int BaseDamage;
    public readonly string ProfileId;
    public readonly bool IsParryable;
    public readonly long WindupDurationMs;
    public readonly long VisualImpactDelayMs;
    public readonly long DamageCommitDelayMs;
    public readonly long RecoveryDurationMs;
    public readonly ComboInterruptRule ComboInterruptRule;
    public readonly string TelegraphCueId;
}
```

Rules:

- Enemy patterns output content facts only.
- Turn manager assigns runtime segment IDs and timeline sequence IDs.
- Pattern choice reads only query data, cooldown state, deterministic seed, and pattern definitions.
- `OverlapRejected` requires reschedule or cancel; it must not mutate `isParryable=false`.
- PerfectParry response follows `ComboInterruptRule`.
- Attack plan DTOs must not contain `GameObject`, `Transform`, `AnimationClip`, `ScriptableObject`, or Unity instance IDs.

## Alternatives Considered

### Alternative 1: Implement MVP enemies as direct turn-manager scripts

- **Description**: Turn manager hardcodes enemy attacks and sequence choices.
- **Pros**: Fast for one prototype.
- **Cons**: Mixes scheduling authority with enemy content and makes expansion difficult.
- **Rejection Reason**: Violates ownership separation in turn and enemy GDDs.

### Alternative 2: Use GOAP or behavior trees immediately

- **Description**: Use a richer AI framework for enemy decisions.
- **Pros**: Scales to complex AI.
- **Cons**: Overkill for MVP, increases debugging surface, and delays parry validation.
- **Rejection Reason**: MVP needs readable scripted patterns, not complex planning.

### Alternative 3: Pure scripted planner with deterministic selection

- **Description**: Pattern catalog and planner own content/choice; turn manager owns scheduling.
- **Pros**: Testable, deterministic, readable, and aligned with current scope.
- **Cons**: Less expressive than full AI frameworks.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- Enemy behavior remains predictable enough for parry mastery.
- Attack content can be tested without Unity scenes.
- Turn, parry, and damage ownership stay separated.
- Pattern data can later be loaded from Unity assets without leaking asset references into domain logic.

### Negative

- Designers need explicit pattern definitions and IDs.
- Weighted randomness requires deterministic seeding discipline.
- Complex boss behavior may require future extension ADRs.

### Risks

- **Risk**: Runtime pattern selection uses Unity randomness or frame time.
  - **Mitigation**: Inject deterministic RNG and assert fixed-seed replay.
- **Risk**: Pattern DTOs leak Unity asset references.
  - **Mitigation**: Convert asset-authored data into pure DTOs before gameplay.
- **Risk**: Overlap rejection creates unfair unavoidable hits.
  - **Mitigation**: Tests must assert rejected overlap can only reschedule or cancel.

## GDD Requirements Addressed


| GDD System       | Requirement                                       | How This ADR Addresses It                            |
| ---------------- | ------------------------------------------------- | ---------------------------------------------------- |
| `敌人 AI 与攻击模式.md` | Scripted weighted pattern selection               | Defines pure planner, query, and deterministic seed  |
| `敌人 AI 与攻击模式.md` | Own attack content facts                          | Defines pattern and segment content ownership        |
| `敌人 AI 与攻击模式.md` | Respond to parry/overlap/counter without cheating | Defines event response and no-guaranteed-hit rule    |
| `回合管理器.md`       | Turn manager schedules enemy attack segments      | Keeps runtime segment identity with turn manager     |
| `实时弹反系统.md`      | Parry derives windows from seed facts             | Provides seed content without final window ownership |


## Performance Implications

- **CPU**: O(pattern count) per enemy action selection; MVP pattern count is small.
- **Memory**: Pattern catalog can be immutable and shared; runtime cooldown state is battle-scoped.
- **Load Time**: Minimal; asset-to-DTO conversion may occur at encounter load.
- **Network**: Not applicable.

## Migration Plan

1. Define pattern catalog DTOs and planner interfaces in pure gameplay.
2. Add deterministic RNG/test seed utilities.
3. Convert MVP enemy pattern data into pure definitions.
4. Integrate turn manager action query and seed submission.
5. Add overlap, cancel, and PerfectParry response tests.

## Validation Criteria

- Fixed seed and snapshots produce identical pattern choices over repeated runs.
- Pattern score returns zero for invalid target, cooldown, or round/phase mismatch.
- Submitted seed includes all parry-required content facts.
- `OverlapRejected` cannot produce a guaranteed hit.
- PerfectParry applies the configured combo interrupt rule.

## Related Decisions

- `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md`
- `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md`
- `docs/architecture/adr-0007-parry-timeline-counter-handoff.md`
- `design/gdd/敌人 AI 与攻击模式.md`
- `design/gdd/回合管理器.md`
- `design/gdd/实时弹反系统.md`


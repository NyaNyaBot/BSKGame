# ADR-0011: 角色数据模型 Schema 与快照所有权

## Status

Accepted

## Date

2026-04-29

## Engine Compatibility

| Field | Value |
|---|---|
| **Engine** | Unity 2022.3.17f1 |
| **Domain** | Core / Data Model / Combat State |
| **Knowledge Risk** | LOW |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`, `docs/architecture/adr-0005-damage-hp-ownership.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Pure C# schema validation tests; snapshot immutability tests; HP mutation boundary tests |

## ADR Dependencies

| Field | Value |
|---|---|
| **Depends On** | ADR-0003, ADR-0005 |
| **Enables** | Turn, damage, parry, enemy, action, UI implementation stories |
| **Blocks** | Character repository and combat participant stories |
| **Ordering Note** | Must preserve ADR-0005: damage service remains the only external HP write path. |

## Context

### Problem Statement

Character data is the foundation for turn order, target legality, damage, parry, enemy selection, action validation, and UI display. The GDD defines static definitions, runtime instances, immutable snapshots, and stable IDs, but no ADR currently governs schema ownership or mutation boundaries.

### Constraints

- Domain character data must not depend on Unity scene objects.
- Stable gameplay IDs are required across events and tests.
- HP is stored on runtime instances but externally mutated only through damage service.
- Snapshots must prevent consumers from observing partially mutated state.
- MVP uses one player character but must not hardcode singleton assumptions.

### Requirements

- `TR-char-001`: stable gameplay IDs for cross-system references.
- `TR-char-002`: separate static definitions, runtime instances, and immutable snapshots.
- `TR-char-003`: expose independent legality flags.
- `TR-char-004`: defeated characters remain until explicit battle/scene cleanup.
- `TR-dmg-001`: damage/HP system is the only HP write entry point.

## Decision

Create a pure C# `ICharacterRepository` that owns character definitions, battle-scoped runtime instances, snapshot generation, and controlled state mutation hooks. Static definitions are immutable loaded data. Runtime instances are battle-scoped mutable records. `CharacterSnapshot` is an immutable DTO generated at explicit query or event points.

The repository exposes no public arbitrary HP setter. HP mutation is available only through an internal/controlled boundary used by `IDamageService`. Other systems consume snapshots or request state changes through their owning systems.

### Architecture Diagram

```text
CharacterDefinition catalog
        |
Scene/Battle start
        v
CharacterRepository creates RuntimeInstances
        |
        +-> CharacterSnapshot -> Turn / Parry / Enemy / Action / UI
        |
        +-> controlled HP mutation <- DamageService only
        |
Scene/Battle dispose -> explicit cleanup/removal
```

### Key Interfaces

```csharp
public interface ICharacterRepository
{
    CharacterRuntimeInstanceId CreateInstance(CharacterSpawnRequest request);
    CharacterSnapshot GetSnapshot(string instanceId);
    IReadOnlyList<CharacterSnapshot> GetBattleParticipants(string battleContextId);
    bool TryGetDefinition(string characterId, out CharacterDefinition definition);
}

internal interface ICharacterHpMutationSink
{
    CharacterMutationResult ApplyHpDeltaFromDamage(DamageMutationCommand command);
    CharacterMutationResult SetDefeatedFromDamage(string instanceId, string sourceEventId);
}
```

Rules:

- `CharacterId` identifies static definitions.
- `InstanceId` identifies battle/runtime instances.
- Events use runtime `InstanceId` when referring to combat participants.
- Snapshots are immutable and include a `Version`.
- `CurrentHp`, `RuntimeState`, and legality flags update version when externally visible.
- `Defeated` is not `Removed`.
- Runtime collections represent teams as lists even in single-player MVP.
- Unity instance IDs and GameObject references are not character identity.

## Alternatives Considered

### Alternative 1: Store all character state directly on Unity components

- **Description**: MonoBehaviours hold HP, flags, and identity.
- **Pros**: Easy Inspector visibility.
- **Cons**: Hard to test, unsafe across scene unload, and violates pure domain layering.
- **Rejection Reason**: Conflicts with ADR-0003.

### Alternative 2: Let damage service fully own character state

- **Description**: Damage service stores HP and all runtime character fields.
- **Pros**: HP ownership is simple.
- **Cons**: Mixes damage consequence rules with general character identity/snapshot storage.
- **Rejection Reason**: Damage owns HP mutation policy, not all character data.

### Alternative 3: Repository owns schema/snapshots with controlled damage mutation

- **Description**: Character repository owns storage; damage service is the only external HP mutation caller.
- **Pros**: Keeps data model centralized while preserving HP ownership.
- **Cons**: Requires internal mutation boundary discipline.
- **Rejection Reason**: Chosen.

## Consequences

### Positive

- All combat systems share stable character facts.
- Snapshots make deterministic tests easier.
- Damage remains the only HP write entry point.
- Future three-character party support does not require interface rewrite.

### Negative

- Requires DTO mapping and version tracking.
- Some mutation APIs must be internal or otherwise guarded.
- Debugging state requires repository inspection tools.

### Risks

- **Risk**: Controlled HP hook becomes a second public write API.
  - **Mitigation**: Keep it internal to gameplay assembly or expose only to `IDamageService` composition; code review forbids direct feature use.
- **Risk**: Consumers cache snapshots too long.
  - **Mitigation**: Include snapshot version and require fresh queries at decision points.
- **Risk**: Single-player MVP leads to singleton assumptions.
  - **Mitigation**: All interfaces return collections for teams/participants.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|---|---|---|
| `角色数据模型.md` | Stable gameplay IDs | Defines `CharacterId` and `InstanceId` roles |
| `角色数据模型.md` | Static definitions, runtime instances, snapshots | Defines repository-owned schema layers |
| `角色数据模型.md` | Independent legality flags | Requires snapshots to expose flags |
| `角色数据模型.md` | Defeated remains until cleanup | Defines state semantics |
| `伤害与生命系统.md` | Damage is only HP write path | Restricts HP mutation to damage service boundary |

## Performance Implications

- **CPU**: Snapshot creation is small; avoid per-frame UI polling.
- **Memory**: Runtime instances and snapshots are battle-scoped; keep historical stubs bounded.
- **Load Time**: Character definitions can be loaded at encounter setup.
- **Network**: Not applicable.

## Migration Plan

1. Define character definition, runtime instance, and snapshot DTOs.
2. Implement repository and validation for duplicate IDs and invalid stats.
3. Add controlled mutation boundary for damage service.
4. Update turn, parry, enemy, action, and UI to consume snapshots.
5. Add schema, snapshot, and mutation boundary tests.

## Validation Criteria

- Duplicate `CharacterId` fails validation.
- Invalid `MaxHp <= 0` cannot create a battle participant.
- Snapshot is immutable and versioned.
- Damage service can mutate HP exactly through the controlled boundary.
- Non-damage systems cannot write HP directly.
- Defeated instance remains queryable until explicit cleanup.

## Related Decisions

- `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md`
- `docs/architecture/adr-0005-damage-hp-ownership.md`
- `design/gdd/角色数据模型.md`
- `design/gdd/伤害与生命系统.md`

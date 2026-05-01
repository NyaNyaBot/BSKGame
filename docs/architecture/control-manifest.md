# Control Manifest

> **Engine**: Unity 2022.3.17f1  
> **Last Updated**: 2026-05-01  
> **Manifest Version**: 2026-05-01  
> **ADRs Covered**: ADR-0001, ADR-0002, ADR-0003, ADR-0004, ADR-0005, ADR-0006, ADR-0007, ADR-0008, ADR-0009, ADR-0010, ADR-0011, ADR-0012, ADR-0013  
> **Status**: Active — regenerate when any ADR status changes to Accepted/Superseded or ADR text materially changes.

`Manifest Version` equals `Last Updated`. Story files may embed this date; `/story-readiness` may compare against it for stale-rule detection.

**Director gate:** TD-MANIFEST skipped — Lean mode (`production/review-mode.txt`).

**Loaded inputs report:** 13 Accepted ADRs; engine Unity 2022.3.17f1; `technical-preferences.md`. `docs/engine-reference/unity/deprecated-apis.md` and `current-best-practices.md` are absent — Global Forbidden APIs lists that gap only (no invented API bans).

---

## Foundation Layer Rules

*Applies to: scene management, event architecture, save/load, engine initialisation, cross-cutting runtime scope*

### Required Patterns

- Maintain authoritative `SceneContext` and nested `BattleContext`; every cross-system battle event carries `SceneEventContext` (`sceneContextId`, `sceneVersion`, `battleContextId`, `battleVersion`). — source: ADR-0001  
- `SceneContextId` changes on every scene entry including retry; `SceneVersion` increments before teardown side effects complete; `BattleVersion` increments on battle dispose/restart. — source: ADR-0001  
- Do not use Unity `GameObject` / scene object identity to decide whether an event is current; use context IDs/versions. — source: ADR-0001  
- Scene management must freeze combat input before disposing battle runtime state. — source: ADR-0001  
- Use lightweight synchronous in-process battle event bus; events are immutable DTOs implementing `IBattleEvent` with `SceneEventContext`, `EventId`, `SchemaVersion`, `OccurredAtCombatClockMs`; state-mutating events carry idempotency key or source ID. — source: ADR-0002  
- Owner system publishes authoritative fact events; consumers update read models or submit explicit requests — they must not write another system’s owned state directly. — source: ADR-0002  
- DTOs immutable after publish; stable gameplay IDs only; no mutable domain objects in DTOs; `SchemaVersion` on DTOs (MVP starts at `1`); rejection/cancellation as first-class events, not logs; requests named as requests (e.g. `HitStopRequest`) and owning system still decides acceptance. — source: ADR-0002  
- Bus: typed publish/subscribe with context filtering and enqueue APIs; **ADR-0004 owns combat tick-local drain order** — event bus must **not** own an independent gameplay drain loop. — source: ADR-0002, ADR-0004  
- Events published during handler execution are enqueued, not immediately re-entered; turn manager / combat tick runner drains queued events by ADR-0004 priority then enqueue order within current `CombatClockMs` tick. — source: ADR-0002, ADR-0004  
- Publishers must not depend on UI/feedback handlers completing to determine gameplay facts. — source: ADR-0002  
- Guard recursive publish depth and per-tick event count; event trace records enqueue order, drain order, source event IDs. — source: ADR-0002  
- HybridCLR/AOT: preserve generic call sites for `Publish<T>` / `Subscribe<T>` used by event DTOs in hot-update code. — source: ADR-0002  
- Three-layer assembly model: (1) pure gameplay/domain — no `UnityEngine`; (2) Unity hot-update adapters — MonoBehaviour/UI/input/feedback bridges; (3) Launch/AOT — HybridCLR load, **no combat rule logic**. — source: ADR-0003  
- DTOs crossing gameplay/Unity boundary: primitives, strings, enums, project pure math types only. — source: ADR-0003  
- Generic services, event DTOs, reflection-visible types across HybridCLR boundaries: AOT generic references or link preservation. — source: ADR-0003  
- Battle input subsystem: `TouchTimestampAdapter` maps platform touch/PointerDown to `battleTimestampMs` and diagnostics; `InputHitAreaRegistry` owns registered areas, priority, occlusion, state versions, duplicate filtering, dispatch to domain events. — source: ADR-0006  
- Parry/counter judgment must originate from touch/PointerDown sampling with diagnostic metadata; UGUI `Button.onClick` not the judgment source for parry/counter timing. — source: ADR-0006  
- Hit area priorities: Counter `300`; Modal/pause `250`; Standard UI action `200`; Parry `100`; Background `0`. — source: ADR-0006  
- When `CounterEntryOpened` is active: counter area higher priority than parry; parry area must not use `BlocksUnderlying=true` while counter active; stale `stateVersion` ignored. — source: ADR-0006  
- Timestamp mapping: `bridgeDelayMs = max(0, receivedRealtimeMs - normalizedRawTimestampMs - calibratedBridgeOffsetMs)`; `battleTimestampMs = max(0, combatClock.NowMs - bridgeDelayMs)`; normalize units/epoch before `bridgeDelayMs`; reject/clamp non-monotonic / negative-after-norm / above max plausible bridge delay. — source: ADR-0006  
- `latency_compensation_ms` is **not** applied by input system (parry/system tuning only; no double-compensation with bridge delay). — source: ADR-0006  
- Registry hit testing: keep sorted by priority; MVP area count small. — source: ADR-0006 Performance  

### Forbidden Approaches

- **Never** use only Unity active scene name/handle for stale-event rejection — fails retry and multi-battle identity. — source: ADR-0001 Alternatives  
- **Never** let each system own its own independent “scene active” lifecycle flag — divergent truth and stale subscriptions. — source: ADR-0001 Alternatives  
- **Never** use direct cross-system service calls as the primary combat integration (turn→parry→damage→UI chains) — cycles, ordering, stale context inconsistency. — source: ADR-0002 Alternatives  
- **Never** use fragmented per-system C# events as the primary bus without central context/version guard and trace — subscription cleanup and ordering fragmented. — source: ADR-0002 Alternatives  
- **Never** put mutable payloads or Unity object references in battle event DTOs — bus becomes shared mutable state. — source: ADR-0002 Consequences / ADR-0003  
- **Never** let event bus own a separate gameplay drain loop from ADR-0004 ordering — ordering authority split. — source: ADR-0002, ADR-0004  
- **Never** implement core battle rules in MonoBehaviours / scene components as rule authority. — source: ADR-0003 Alternatives  
- **Never** collapse pure gameplay and Unity adapters in one assembly if that causes `UnityEngine` leakage into domain — weakens EditMode testability. — source: ADR-0003 Alternatives  
- **Never** use UGUI `Button.onClick` as the combat timing judgment source for parry/counter. — source: ADR-0006 Alternatives  
- **Never** let each UI panel own isolated touch hit logic for combat — priority/occlusion inconsistent. — source: ADR-0006 Alternatives  
- **Never** use `Expression.Emit`, `DynamicMethod`, or unpreserved reflection constructors on WebGL/AOT paths per ADR-0003 mitigation — source: ADR-0002, ADR-0003  

### Performance Guardrails

- Scene/event: context check constant-time; DTO context fields acceptable for MVP volume. — source: ADR-0001  
- Event bus: O(subscribers per type); prefer readonly structs or pooled immutable DTOs only when measured. — source: ADR-0002  
- Input registry: O(active areas); avoid unbounded input logs in release. — source: ADR-0006  

---

## Core Layer Rules

*Applies to: combat clock, turn flow, damage/HP, character snapshots, deterministic ordering with gameplay facts*

### Required Patterns

- Turn manager owns **only** mutable `CombatClockMs` via `ICombatClockController`; others get read-only `ICombatClock` or event timestamps; input/parry/damage/UI/feedback must not advance or rewind the clock. — source: ADR-0004  
- Within same `CombatClockMs` tick, process in order: (1) disposal/context invalidation (2) pause/resume/hit stop (3) input phase changes (4) input attempts mapped (5) parry validation/resolution (6) damage/HP (7) death/victory/defeat (8) counter window open/close (9) UI/feedback read models (10) debug/trace. — source: ADR-0004  
- Tick-local events appended to ADR-0002 queue and drained by above policy; handlers must not re-enter gameplay mutation outside queue; detect excessive recursive production. — source: ADR-0004, ADR-0002  
- `Advance` rejects negative delta; clamp very large delta to configured max step. — source: ADR-0004  
- Hit stop pauses `CombatClockMs`; presentation may use unscaled Unity time; hit stop accepted only by turn manager; parry result never recalculated after hit stop starts. — source: ADR-0004  
- `OccurredAtCombatClockMs` is logical event time, not Unity processing time; tests may inject fake clock. — source: ADR-0004  
- Tick-local drain max count in debug/test to detect cycles. — source: ADR-0004  
- All HP mutation through `IDamageService.ApplyDamage(DamageRequest)`; publish exactly one terminal outcome per `DamageRequestId`: `DamageApplied` or `DamageRejected`; `CharacterDefeated` when transition applies. — source: ADR-0005  
- `DamageRequestId` globally unique per battle; duplicate ID returns original result, never double HP. — source: ADR-0005  
- Final damage clamped `>= 0`; HP clamped `[0, MaxHp]`; defeat emitted once per defeat transition; multipliers as **integer basis points** in `DamageRequest` (`PerfectParry` 0, `NormalParry` 5000, `FailedParry` 10000); float `damageMultiplier` from GDD converted at integration boundary. — source: ADR-0005  
- Pure C# `IParryResolver`: owns `FrozenAttackSegmentTimeline`, resolution, counter authorization events; at most one terminal result per `attackSegmentId + timelineSequenceId`. — source: ADR-0007  
- Timeline immutable after acceptance; validation failure emits `AttackSegmentTimelineRejected` (not a failed parry); valid segment exactly one of `ParryResolved` / `ParryCancelled` / `AttackSegmentTimelineRejected` terminal; Perfect emits `damageMultiplier=0` and `CounterEntryOpened`; Normal 5000 bps no counter; Failed 10000 bps no counter; counter closure owned by turn manager. — source: ADR-0007  
- `CounterEntryOpened` only after `ParryResolved(PerfectParry)`; counter input valid from **next** queue phase after `CounterEntryOpened` drained; same-tick touches before authorization stay parry/action, not retroactively counter. — source: ADR-0007  
- `ICharacterRepository` owns definitions, battle runtime instances, snapshots, controlled HP mutation hook used only by `IDamageService`; no public arbitrary HP setter; snapshots immutable with `Version`. — source: ADR-0011  
- `CharacterId` static definition; `InstanceId` runtime participant; events use `InstanceId`; `Defeated` ≠ `Removed`; teams as lists (no singleton hardcoding). — source: ADR-0011  
- `IBattleActionService` owns authorization, action idempotency, target legality from snapshots, conversion to `DamageRequest`; never mutates HP, turn phase, or UI directly. — source: ADR-0012  
- MVP catalog exactly `BasicAttackAction` (only `BattleInputPhase.PlayerCommand`) and `CounterAction` (only with current `CounterEntryOpened` / turn in `CounterWindow`); `BattleActionRequestId` idempotency; valid action emits result events and at most one `DamageRequest`; duplicate action returns original result. — source: ADR-0012  
- `CounterAction` must reference source `CounterEntryOpened` / `resolutionId`; animation completion cannot roll back committed damage. — source: ADR-0012  

### Forbidden Approaches

- **Never** read Unity `Time.time` / `deltaTime` as authority for parry/combat timing everywhere. — source: ADR-0004 Alternatives  
- **Never** give each subsystem its own independent logical timer for combat time. — source: ADR-0004 Alternatives  
- **Never** let features apply HP directly (enemy/parry/counter/UI) — double damage and inconsistent death order. — source: ADR-0005 Alternatives  
- **Never** let character repository own all damage formulas as the sole damage authority — mixes storage with combat consequence rules. — source: ADR-0005 Alternatives  
- **Never** hand-author final parry window fields in enemy pattern data as rule authority — invalid relationships and content owns rules. — source: ADR-0007 Alternatives  
- **Never** drive parry windows from Unity animation events / frame progress as rule authority — unsafe under low FPS/WebGL. — source: ADR-0007 Alternatives  
- **Never** let UI call `IDamageService` directly with self-built `DamageRequest` — UI becomes authority, counter bypass risk. — source: ADR-0012 Alternatives  
- **Never** put all player actions inside turn manager — turn absorbs action effects, scope creep. — source: ADR-0012 Alternatives  
- **Never** expose public HP setters on character model for feature convenience — second write path. — source: ADR-0011 Consequences  
- **Never** use Unity instance IDs / `GameObject` as durable gameplay identity for characters. — source: ADR-0011  

### Performance Guardrails

- Damage: constant-time validation; battle-scoped idempotency cache cleared on battle dispose. — source: ADR-0005  
- Parry resolver: constant-time validation per active segment; one frozen timeline + small idempotency record per segment. — source: ADR-0007  
- Combat clock: deterministic trace optional in release; bounded ring buffer if enabled. — source: ADR-0004  
- Character snapshots: avoid per-frame UI polling; battle-scoped instances. — source: ADR-0011  

---

## Feature Layer Rules

*Applies to: enemy attack planning, scripted AI, action catalog extensions*

### Required Patterns

- `IEnemyAttackPlanner` + `IAttackPatternCatalog` in pure gameplay; patterns own content facts + deterministic selection; turn manager owns scheduling: `attackSegmentId`, `timelineSequenceId`, segment lifecycle, phase transitions. — source: ADR-0008  
- Pattern selection: injected deterministic RNG or fixed test seed — **must not** use Unity `Random`, wall-clock, frame count, ScriptableObject instance identity, or scene objects as rule input; Unity assets may author data but runtime DTOs = stable IDs + primitives before gameplay. — source: ADR-0008  
- Patterns output content facts only; `OverlapRejected` → reschedule or cancel only; must not flip `isParryable=false` to force hit; PerfectParry applies `ComboInterruptRule`. — source: ADR-0008  
- Attack plan DTOs: no `GameObject`, `Transform`, `AnimationClip`, `ScriptableObject`, Unity instance IDs. — source: ADR-0008  
- Future skills: extend action catalog via `IBattleActionService` — **not** bypass service. — source: ADR-0012 Decision  

### Forbidden Approaches

- **Never** hardcode all enemy behavior inside turn manager scripts as long-term approach — mixes scheduling with enemy content. — source: ADR-0008 Alternatives  
- **Never** adopt GOAP/BT for MVP enemy — scope and debugging surface. — source: ADR-0008 Alternatives  
- **Never** use non-deterministic Unity randomness or frame time in pattern selection. — source: ADR-0008 Consequences  

### Performance Guardrails

- Enemy selection O(pattern count) per query; MVP small; immutable shared catalog, battle-scoped cooldown state. — source: ADR-0008  

---

## Presentation Layer Rules

*Applies to: UIFrame HUD, feedback, VFX/audio, hit stop requests*

### Required Patterns

- Battle feedback service in **Unity hot-update adapter** layer; consumes immutable battle events; builds `FeedbackRequest` / `HitStopRequest`; quality tiers cap particles, shake, flash, vibration, concurrent audio; degradation must not change parry grade, damage, HP, turn order, event ordering, or clock/window facts. — source: ADR-0009  
- Feedback: drop stale `SceneEventContext`; idempotent key feedback by `SourceEventId`; hit stop request only — turn manager accepts and pauses `CombatClockMs`; hit stop duration clamped by tier + ADR-0004; presentation may use unscaled time while clock paused; bounded logs release, detail dev/test. — source: ADR-0009, ADR-0004  
- Critical readability: each tier retains at least one visual or audio signal for key outcomes. — source: ADR-0009  
- Battle HUD: UIFrame Window + Panels; UI owns layout, view-model binding, panel lifecycle, **registration/unregistration lifecycle** of hit areas; input system owns `InputHitAreaRegistry`, hit test, priority, occlusion, duplicates, timestamps. — source: ADR-0010, ADR-0006  
- UI reads immutable view models / event DTOs only; unregister all hit areas and subscriptions on unload/dispose; counter registers priority `300`, parry `100`; counter active → parry must not block counter; debug overlays (`CombatClockMs`, window bounds, IDs) **development only**; UI cannot infer parry success from animation/button state. — source: ADR-0010  
- Formal HUD: no exact ms parry countdown, QTE rings, or “press now” prompts. — source: ADR-0010, ADR-0007 GDD linkage  

### Forbidden Approaches

- **Never** let each combat system trigger its own VFX/audio directly as primary integration — couples presentation to facts, inconsistent degradation. — source: ADR-0009 Alternatives  
- **Never** always play full-intensity feedback regardless of device — WebGL unsafe. — source: ADR-0009 Alternatives  
- **Never** use monolithic single-window HUD if it tangles disposal and hit lifecycle against UIFrame composition norms. — source: ADR-0010 Alternatives (discouraged)  
- **Never** let UI panels own combat hit testing / timestamp authority — conflicts ADR-0006. — source: ADR-0010 Alternatives  

### Performance Guardrails

- Feedback: profile adapters; pool VFX/audio requests; cap concurrent cues; avoid GC spikes. — source: ADR-0009  
- HUD: event-driven updates, no per-frame polling of mutable gameplay objects; battle-scoped view models/subscriptions. — source: ADR-0010  
- Preload HUD prefabs with battle scene where needed. — source: ADR-0010  

---

## Global Rules (All Layers)

### Naming Conventions

| Element | Convention | Example |
|--------|------------|---------|
| Classes | PascalCase | `PlayerController` |
| Public fields/properties | PascalCase | `MoveSpeed` |
| Private fields | _camelCase | `_moveSpeed` |
| Methods | PascalCase | `TakeDamage()` |
| Signals/Events | PascalCase + `EventArgs` suffix | `ParrySuccessEventArgs` |
| Files | PascalCase matching class | `PlayerController.cs` |
| Scenes/Prefabs | PascalCase | `BattleScene.unity` |
| Constants | PascalCase or UPPER_SNAKE_CASE | — |

*Source: `technical-preferences.md`*

### Performance Budgets

| Target | Value |
|--------|--------|
| Framerate | 30 FPS minimum on target WeChat WebGL; 60 FPS stretch high-end |
| Frame budget | 33.3 ms @ 30 FPS; 16.7 ms stretch @ 60 FPS |
| Draw calls | ≤ 70 at PerfectParry peak; ≤ 50 normal combat |
| Memory ceiling | 256 MB runtime working set target MVP WebGL; no unbounded combat event/feedback logs |

*Source: `technical-preferences.md`*

### Testing (from technical preferences + ADR-0013)

- Framework: NUnit (Unity Test Framework).  
- Targets: ≥ 80% line/branch on pure gameplay services once implemented; **100%** contract coverage for timing, HP mutation, idempotency, stale-context acceptance.  
- Required evidence areas: balance formulas, combat clock, parry boundaries, HP/damage idempotency, input hit priority, scene unload cleanup, HybridCLR/WebGL smoke.  
- Layered strategy: pure domain tests (fake clock, fixed seed, DTO/trace); PlayMode adapter/UI/input/feedback lifecycle; WebGL/WeChat manual or automated device evidence for timestamps, fallback, bridge delay, latency, safe area, feedback FPS/GC; HybridCLR hot-update smoke before large combat expansion.  
- Domain tests must not depend on Unity scene objects; timing stories: boundaries early/perfect/late/none; event-order stories: assert trace matches ADR-0004; WebGL input stories: device evidence or explicit gap flag; test `ReceivedFrameFallback`; smoke includes HybridCLR load.  

*Source: `technical-preferences.md`, ADR-0013*

### Approved Libraries / Addons

- None configured in `technical-preferences.md` (placeholder).  

### Forbidden APIs (Unity 2022.3.17f1)

- `docs/engine-reference/unity/deprecated-apis.md` **missing** — no project-maintained deprecated API list to merge. Until added: follow each ADR **Engine Compatibility** / **Post-Cutoff APIs Used** (currently None across ADRs) and verify against Unity docs when implementing.  

### Forbidden Patterns (technical preferences)

- Placeholder: “None configured yet” in `technical-preferences.md` — add explicit bans there as new decisions land.  

### Cross-Cutting Constraints

- Presentation must not own gameplay facts (`TR-concept-002`, ADR-0003/0009/0010).  
- Stale `SceneEventContext` rejection before any state mutation or gameplay change (ADR-0001, ADR-0002).  
- HybridCLR: maintain AOT generic preservation for event bus generics and cross-boundary DTO/service types (ADR-0002, ADR-0003).  
- WebGL input: document timestamp source, fallback ratio, bridge delay, input-to-result latency per ADR-0006 / ADR-0013 evidence contracts.  

---

## Suggested Next Steps

1. Run `/ccgs-create-epics layer: foundation` then `/ccgs-create-epics layer: core`, then `/ccgs-create-stories [epic-slug]` for each — programmers embed **Manifest Version `2026-05-01`** and TR-ID + ADR references per story.  
2. Execute Pre-Production Sprint 0: HybridCLR smoke, WebGL/WeChat timestamp + safe-area evidence, low-FPS parry replay, feedback degradation without fact changes — closes prior gate CONCERNS.  
3. Add `docs/engine-reference/unity/deprecated-apis.md` (and optional `current-best-practices.md`) then regenerate this manifest so Global Forbidden APIs is populated from repo truth.

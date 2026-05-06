# Active Session

- Task: UX Spec — Pause Menu（暂停菜单）
- Status: Revised — **`ux-review` APPROVED**（2026-05-06）；宿主暂停策略已写入 Spec
- File: design/ux/pause-menu.md
- Queue: 实现移交 `/team-ui`；Open #2/#3（探索共用 Pause / 音频 Duck）非门禁阻断
- Next: 按 Acceptance 绑宿主 `onHide`/`onShow` smoke

## Previous — Main Menu

- File: design/ux/main-menu.md — APPROVED（2026-05-06）

## Previous Session (archived note)

- Task: MVP 主架构蓝图
- File: docs/architecture/architecture.md
- Next (historical): 见下方 Session Extract

## Session Extract — /architecture-review 2026-04-29
- Verdict: FAIL
- Requirements: 38 total — 0 covered, 0 partial, 38 gaps
- New TR-IDs registered: None
- GDD revision flags: `design/gdd/实时弹反系统.md`
- Top ADR gaps: 场景生命周期与场景上下文路由; 战斗事件总线与 DTO 版本策略; 玩法逻辑与 Unity 适配器程序集分层
- Report: docs/architecture/architecture-review-2026-04-29.md

## Session Extract — architecture repair 2026-04-29
- Restored: `design/gdd/实时弹反系统.md` from review log and approved dependent contracts
- Fixed: dangling turn-manager TR reference in `docs/architecture/architecture.md`
- Remaining blocker: P0 ADR coverage still missing

## Session Extract — P0 ADR authoring 2026-04-29
- Created: ADR-0001 through ADR-0006 as Proposed P0 architecture baselines
- ADRs: scene lifecycle/context routing; battle event bus/DTO versioning; gameplay/Unity/HybridCLR layering; combat clock/deterministic ordering; damage/HP ownership; mobile touch timestamp/hit area strategy
- Engine specialist result: no BLOCKING Unity/HybridCLR issue; high-risk notes incorporated
- Updated: `docs/architecture/architecture.md` and technical preferences ADR log
- Remaining blocker: ADRs are Proposed; run independent `/architecture-review` before accepting or advancing gate

## Session Extract — /architecture-review 2026-04-29
- Verdict: FAIL
- Requirements: 38 total — 26 covered, 6 partial, 6 gaps
- New TR-IDs registered: None
- GDD revision flags: None
- Top ADR gaps: 弹反时间轴派生与 counter handoff; 敌方攻击模式数据与调度所有权; 战斗反馈质量分级与 WebGL 降级策略
- Report: docs/architecture/architecture-review-2026-04-29.md

## Session Extract — missing ADR completion 2026-04-29
- Created: ADR-0007 through ADR-0013 as Proposed follow-up architecture baselines
- ADRs: parry timeline/counter handoff; enemy attack pattern ownership; battle feedback quality/WebGL degradation; UIFrame battle HUD composition; character schema/snapshot ownership; action service boundary; deterministic combat test strategy
- Resolved draft conflict: ADR-0004 is now the combat tick/event drain order authority; ADR-0002 owns typed DTO/subscription/context filtering/enqueue mechanics only
- Updated: `docs/architecture/architecture.md` and technical preferences ADR log
- Remaining blocker: run independent `/ccgs-architecture-review` before accepting ADRs or advancing pre-production gate

## Session Extract — /architecture-review 2026-04-29
- Verdict: CONCERNS
- Requirements: 38 total — 38 covered, 0 partial, 0 gaps
- New TR-IDs registered: 38
- GDD revision flags: None
- Top ADR gaps: None
- Report: docs/architecture/architecture-review-2026-04-29.md

## Session Extract — /gate-check pre-production remediation 2026-04-29
- Verdict: CONCERNS
- Closed blockers: tests/CI baseline, accessibility requirements, UX interaction patterns, HUD UX spec, performance budgets, ADR Accepted status
- Remaining concerns: Unity deprecated/breaking/module reference docs; HybridCLR/WebGL device evidence
- Report: production/gate-checks/pre-production-gate-2026-04-29.md

## Session Extract — /dev-story 2026-05-01
- Story: production/epics/scene-lifecycle-context/story-001-scene-context-identity.md — 场景与战斗上下文身份与版本
- Files changed: client/Assets/Tests/EditMode/Scene/SceneContextIdentityTests.cs (added restart test)
- Test written: client/Assets/Tests/EditMode/Scene/SceneContextIdentityTests.cs (5 tests, 6/6 passed)
- Status: Complete (was already implemented; verified + supplemented restart test)
- Next: /dev-story story-001-battle-event-dto-and-context

## Session Extract — /dev-story 2026-05-01
- Story: production/epics/battle-event-bus-dto-versioning/story-001-battle-event-dto-and-context.md — 战斗事件 DTO 与 SceneEventContext
- Files changed: gameplay/gameplay/Events/IBattleEvent.cs, IBattleEventBus.cs, BattleEventBus.cs (created)
- Test written: client/Assets/Tests/EditMode/Events/BattleEventDtoTests.cs (8 tests, 14/14 passed)
- Blockers: None
- Next: /dev-story story-001-assembly-boundaries or story-002-dto-boundary-primitives

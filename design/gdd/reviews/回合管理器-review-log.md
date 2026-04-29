## Review — 2026-04-27 — Verdict: APPROVED
Scope signal: L
Specialists: lean main-session review against approved input, scene, character, and parry GDDs
Blocking items: 0 | Recommended: 4
Summary: Review found the core battle-flow design implementation-ready after focused fixes. The GDD now cleanly owns `CombatClockMs`, battle phases, input phase changes, attack segment seed scheduling, and counter window closure while leaving damage, AI selection, skill effects, and parry judgement to their owning systems.
Prior verdict resolved: First review.

### Findings Resolved During Review

1. `AttackSegmentTimelineRejected` ownership was clarified: realtime parry owns frozen timeline validation; turn manager only performs seed structural preflight and cancels rejected segments.
2. Counter event naming was aligned to the approved parry contract: `CounterEntryOpened` / `CounterEntryClosed`.
3. Enemy parry input availability was made explicit with `BattleInputPhaseChanged(inputPhase=Parry)`.
4. Acceptance criteria were expanded to cover parry input phase, rejected frozen timeline handling, and counter close recovery.

### Remaining Recommendations

1. 伤害与生命系统 GDD should explicitly accept `battle_result` reading HP/death state without letting turn manager mutate HP.
2. 敌人 AI 与攻击模式 GDD should adopt `AttackSegmentTimelineSeed` and `timelineSequenceId` lifecycle exactly, including `OverlapRejected` reschedule/cancel behavior.
3. 技能与行动系统 GDD should define `BasicAttackAction` and `CounterAction` as MVP test fixtures.
4. 战斗 UI GDD should treat exact counter/window timing as debug-only unless UX later approves player-facing timing display.

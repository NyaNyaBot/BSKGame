# Story 003: 弹反结果与异常事件发布

> **Epic**: parry-system
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/实时弹反系统.md`
**Requirement**: `TR-parry-003`

**ADR Governing Implementation**: ADR-0007, ADR-0002
**ADR Decision Summary**: 弹反系统发布 `ParryResolved`、`ParryCancelled`、`ParryAttemptRejected`、`OverlapRejected`、`AttackSegmentTimelineRejected` 作为不可变 DTO；所有事件携带 `SceneEventContext` 和 `OccurredAtCombatClockMs`；事件通过 `IBattleEventBus` 入队。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM
**Engine Notes**: 事件总线 HybridCLR 泛型保留。

**Control Manifest Rules (this layer)**:

- **Required**: 每种事件场景必须覆盖（Resolution Event Matrix 全覆盖）；DTO 不可变。
- **Forbidden**: 取消或目标无效时发布 `ParryResolved(FailedParry)`——必须走 `ParryCancelled`。
- **Guardrail**: 集成测试验证事件携带正确 context 和 OccurredAtCombatClockMs。

---

## Acceptance Criteria

- [ ] 合法结算产生 `ParryResolved`，载荷含 grade、damageMultiplier、echoIntent、timingOffsetMs、context。
- [ ] 攻击取消/目标无效/场景过期产生 `ParryCancelled`（不产生 FailedParry）。
- [ ] 过早/重复/非弹反阶段输入产生 `ParryAttemptRejected`（含 reason）。
- [ ] 第二个可弹反段与 active 段重叠产生 `OverlapRejected`。
- [ ] seed 验证失败产生 `AttackSegmentTimelineRejected`。
- [ ] 所有事件携带合法 `SceneEventContext`。

---

## Out of Scope

- **Story 001–002**: timeline 派生和结算逻辑。
- **Story 004**: counter 生命周期。

---

## QA Test Cases

- **AC-1**: 事件矩阵覆盖
  - Given: GDD Resolution Event Matrix 中每种条件
  - When: 触发对应场景
  - Then: 产生对应事件类型

- **AC-2**: 取消不制造失败
  - Given: 段在结算前被取消
  - When: 取消发生
  - Then: 发布 ParryCancelled，不发布 ParryResolved(FailedParry)

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/parry/parry_events_integration_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-parry-resolution, `battle-event-bus-dto-versioning` story-001
- Unlocks: story-004-counter-lifecycle

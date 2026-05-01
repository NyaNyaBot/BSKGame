# Story 001: FrozenAttackSegmentTimeline 派生与验证

> **Epic**: parry-system
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/实时弹反系统.md`
**Requirement**: `TR-parry-001`

**ADR Governing Implementation**: ADR-0007
**ADR Decision Summary**: 纯 C# `IParryResolver` 从 `AttackSegmentTimelineSeed` 派生 `FrozenAttackSegmentTimeline`；timeline 不可变且必须满足时间戳排序不变量；违反不变量发布 `AttackSegmentTimelineRejected`。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（低 FPS / 触摸时序）
**Engine Notes**: 派生逻辑为纯 C#，无 Unity 依赖；时间戳使用 `CombatClockMs` 域。

**Control Manifest Rules (this layer)**:

- **Required**: timeline 不可变；`windupStartMs <= windowOpenMs <= perfectStartMs <= parryCenterMs <= perfectEndMs <= visualImpactMs <= normalEndMs <= damageCommitMs <= recoveryEndMs`。
- **Forbidden**: 手动编写冻结窗口字段作为规则权威；用 Unity 动画事件驱动弹反窗口。
- **Guardrail**: 单元测试覆盖全部 timeline invariant 校验路径。

---

## Acceptance Criteria

- [ ] 从合法 `AttackSegmentTimelineSeed` + `window_profile` 派生 `FrozenAttackSegmentTimeline`，所有 invariant 成立。
- [ ] seed 违反任一 invariant 时返回验证失败并准备发布 `AttackSegmentTimelineRejected`。
- [ ] 冻结后的 timeline 不可变——无 public setter，创建后字段不可修改。

---

## Implementation Notes

- `window_profile` 包含 `perfect_half_window_ms`、`normal_early_window_ms`、`normal_late_window_ms`、`center_before_impact_ms`。
- 与 `enemy-ai-attack-patterns`：seed 内容事实由攻击模式提供；本 story 只负责派生和校验。
- 与 `turn-manager-combat-clock`：`CombatClockMs` 提供时间基准，本 story 不推进时钟。

---

## Out of Scope

- **Story 002**: ParryAttempt 结算与 grade 判定。
- **Story 003–004**: 事件发布与 counter 生命周期。

---

## QA Test Cases

- **AC-1**: 合法 seed 派生
  - Given: seed 所有时间戳合法且 profile 参数在 safe range
  - When: 调用 `AcceptTimelineSeed(seed)`
  - Then: 返回成功且 `FrozenAttackSegmentTimeline` 所有字段满足排序不变量
  - Edge cases: 边界值（`perfect_half_window_ms = normal_early_window_ms`）

- **AC-2**: 非法 seed 拒绝
  - Given: seed 的 `center_before_impact_ms < normal_late_window_ms`
  - When: 调用 `AcceptTimelineSeed(seed)`
  - Then: 返回验证失败，含具体 validationError
  - Edge cases: `windupStartMs < 0`、`damageCommitMs < visualImpactMs`

- **AC-3**: 不可变性
  - Given: 已创建 `FrozenAttackSegmentTimeline`
  - When: 尝试修改任何字段
  - Then: 编译错误或运行时不可变保证

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/parry/frozen_timeline_derivation_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `battle-event-bus-dto-versioning` story-001（IBattleEvent 契约）
- Unlocks: story-002-parry-resolution

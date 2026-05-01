# Story 002: CounterAction 授权与 counter 消耗

> **Epic**: action-system
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/技能与行动系统.md`
**Requirement**: `TR-action-002`, `TR-action-003`

**ADR Governing Implementation**: ADR-0012, ADR-0007
**ADR Decision Summary**: `CounterAction` 只在 `CounterEntryOpened` 授权后执行；必须引用来源 `resolutionId`；执行后关闭 counter（通知回合管理器广播 `CounterEntryClosed`）。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW（纯 C# 逻辑）
**Engine Notes**: counter 优先级由输入系统/UI 管理；行动系统只负责授权校验和伤害提交。

**Control Manifest Rules (this layer)**:

- **Required**: counter 必须有 active `CounterEntryOpened`；`sourceCounterResolutionId` 匹配；执行后消耗 counter。
- **Forbidden**: 无 `CounterEntryOpened` 授权时自行创建 counter。
- **Guardrail**: 单元测试含 counter 合法/过期/已消耗三条路径。

---

## Acceptance Criteria

- [ ] 有 active `CounterEntryOpened` 且 `sourceCounterResolutionId` 匹配时，SubmitCounter 成功并生成 `DamageRequest`（powerMultiplier=1.5）。
- [ ] counter 已过期时，SubmitCounter 被拒绝。
- [ ] counter 已被消耗时，重复提交被拒绝。
- [ ] counter 目标在 commit 前死亡时，取消效果并记录 `TargetInvalid`。
- [ ] 执行成功后通知回合管理器关闭 counter。

---

## Out of Scope

- **Story 001**: BasicAttackAction。
- **Story 003**: 幂等和场景过期。

---

## QA Test Cases

- **AC-1**: 合法 counter
  - Given: CounterEntryOpened active, target alive
  - When: SubmitCounter
  - Then: DamageRequest with damage = round_half_up(20 * 1.5) = 30

- **AC-2**: 过期拒绝
  - Given: CombatClockMs > expiresAtMs
  - When: SubmitCounter
  - Then: rejected, reason = CounterExpired

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/action/counter_action_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-basic-attack, `parry-system` story-004
- Unlocks: story-003-action-idempotency

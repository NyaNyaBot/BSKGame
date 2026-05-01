# Story 002: HitStopRequest 构建与提交

> **Epic**: battle-feedback
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/战斗反馈系统.md`
**Requirement**: `TR-fx-002`

**ADR Governing Implementation**: ADR-0009, ADR-0004
**ADR Decision Summary**: 反馈系统构建 `HitStopRequest`（含 sourceResolutionId、durationMs、priority）；回合管理器裁决并通过 `ICombatClockController.BeginHitStop` 暂停 `CombatClockMs`；表现可用 unscaled Unity time。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM
**Engine Notes**: hit stop 暂停逻辑时间；弹反结果不在 hit stop 后重算。

**Control Manifest Rules (this layer)**:

- **Required**: hit stop 是请求不是直接控制；durationMs 按 profile + tier clamp。
- **Forbidden**: 反馈系统直接暂停 CombatClockMs。
- **Guardrail**: 集成测试含 Perfect hit stop 请求和回合管理器裁决。

---

## Acceptance Criteria

- [ ] `ParryResolved(PerfectParry)` 产生一次 `HitStopRequest(durationMs=90, priority=Critical)`。
- [ ] `ParryResolved(NormalParry)` 产生 `HitStopRequest(durationMs=35, priority=Normal)` 或按 profile 配置。
- [ ] 同一 `sourceResolutionId` 不重复提交 hit stop。
- [ ] hit stop request 提交给回合管理器，由其裁决是否暂停时钟。
- [ ] 质量 tier 不允许 hit stop 时，durationMs = 0（不提交或提交 0）。

---

## Out of Scope

- **Story 001**: profile 查表。
- **Story 003**: 性能降级。

---

## QA Test Cases

- **AC-1**: Perfect hit stop
  - Given: ParryResolved(PerfectParry), quality tier allows hit stop
  - When: 构建请求
  - Then: HitStopRequest(durationMs=90, priority=Critical)

- **AC-2**: 低质量 tier
  - Given: quality tier 禁止 hit stop
  - When: 构建请求
  - Then: durationMs = 0

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/feedback/hitstop_request_integration_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-feedback-profile, `turn-manager-combat-clock` story-003
- Unlocks: story-003-quality-degradation

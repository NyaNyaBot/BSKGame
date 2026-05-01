# Story 003: 性能降级与质量分层

> **Epic**: battle-feedback
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/战斗反馈系统.md`
**Requirement**: `TR-fx-003`

**ADR Governing Implementation**: ADR-0009
**ADR Decision Summary**: 质量 tier cap 粒子、shake、flash、vibration、concurrent audio；降级不改 parry grade、damage、HP、turn order、event ordering、clock/window facts。Critical readability 每 tier 至少一个视觉或音频信号。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM（WebGL 性能）
**Engine Notes**: 降级阈值基于 FPS 检测；粒子预算按 tier 配置。

**Control Manifest Rules (this layer)**:

- **Required**: 降级不改变战斗结果；每 tier 至少一个 signal。
- **Forbidden**: 全强度播放无视设备。
- **Guardrail**: 降级模式下战斗结果不变的回归测试。

---

## Acceptance Criteria

- [ ] FPS 低于阈值时进入 `Degraded` 模式，减少粒子和震屏。
- [ ] 降级模式下 `ParryResolved.grade` 和 `DamageApplied.finalDamage` 不变。
- [ ] 每个质量 tier（Low/Medium/High）至少保留一个 Critical 事件的视觉或音频信号。
- [ ] FPS 恢复后退出降级。
- [ ] 降级状态切换不产生 GC 尖峰。

---

## Out of Scope

- **Story 001–002**: profile 和 hit stop。

---

## QA Test Cases

- **AC-1**: 降级不改结果
  - Given: 降级模式 active
  - When: PerfectParry 反馈播放
  - Then: grade 仍为 PerfectParry, damageMultiplier 仍为 0.0

- **AC-2**: 最小信号
  - Given: 最低质量 tier
  - When: PerfectParry 反馈
  - Then: 至少一个视觉或音频 cue 存在

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/feedback/quality_degradation_integration_test.cs` + WebGL 设备证据（或 gap 标记）

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-feedback-profile, story-002-hitstop-request
- Unlocks: Presentation 层联调

# Story 002: 触区注册与优先级管理

> **Epic**: battle-ui
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/战斗 UI.md`
**Requirement**: `TR-ui-002`

**ADR Governing Implementation**: ADR-0010, ADR-0006
**ADR Decision Summary**: UI 创建/更新/注销 `InputHitArea`；输入系统拥有 `InputHitAreaRegistry` 和命中测试；Counter 300 > UI Button 200 > Parry 100；counter active 时 parry 不 block。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（触区 / 安全区）
**Engine Notes**: 触区不小于输入系统最小 dp；安全区内缩不得遮挡 counter。

**Control Manifest Rules (this layer)**:

- **Required**: UI 注册/注销触区生命周期；counter priority 300；counter active 时 parry `BlocksUnderlying=false`。
- **Forbidden**: UI panels 自行做战斗命中测试/时间戳权威。
- **Guardrail**: 集成测试含 counter 注册/注销、优先级验证、stale stateVersion 拒绝。

---

## Acceptance Criteria

- [ ] 进入弹反阶段时注册 Parry 触区（priority=100, rectDp >= 最小 dp）。
- [ ] `CounterEntryOpened` 时注册 Counter 触区（priority=300），parry 触区设 `BlocksUnderlying=false`。
- [ ] counter 关闭时注销 Counter 触区并恢复 parry 设置。
- [ ] 暂停时注销/禁用所有战斗触区。
- [ ] 场景卸载时注销全部触区。
- [ ] 触区更新时递增 `stateVersion`。

---

## Out of Scope

- **Story 001**: HUD 生命周期。
- **Story 003**: 状态渲染。

---

## QA Test Cases

- **AC-1**: Counter 优先级
  - Given: parry area active, CounterEntryOpened
  - When: counter area 注册
  - Then: counter priority 300, parry 100, parry BlocksUnderlying=false

- **AC-2**: 场景卸载清理
  - Given: 多个触区注册
  - When: 场景卸载
  - Then: registry 中该 battle 触区数 = 0

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/ui/hit_area_registration_integration_test.cs` + 设备安全区证据（或 gap 标记）

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-hud-lifecycle, `battle-input-touch-and-hit-areas` story-003
- Unlocks: story-003-state-rendering

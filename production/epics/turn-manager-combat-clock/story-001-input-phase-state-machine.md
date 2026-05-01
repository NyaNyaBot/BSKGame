# Story 001: 显式战斗输入阶段状态机

> **Epic**: turn-manager-combat-clock  
> **Status**: Ready  
> **Layer**: Core  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/回合管理器.md`  
**Requirement**: `TR-turn-001`  

**ADR Governing Implementation**: ADR-0004  
**ADR Decision Summary**: Turn manager 拥有输入阶段机；其他系统通过事件/只读接口观察；禁止多子系统私自推进阶段。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（低 FPS / 触摸时序）  
**Engine Notes**: 阶段迁移与 `CombatClockMs` 推进解耦但可协调（hit stop 等）。

**Control Manifest Rules (this layer)**:

- **Required**: 显式阶段枚举与合法迁移表；非法迁移拒绝或记录。  
- **Forbidden**: 用 Unity `Time.time` 作为阶段权威计时。  
- **Guardrail**: 状态机单元测试全覆盖迁移（MVP 规模）。

---

## Acceptance Criteria

- [ ] 定义 MVP 所需输入阶段（与 GDD 命名一致，如 PlayerCommand / EnemyAction 等子集）。  
- [ ] 合法迁移与拒绝路径有单元测试。  
- [ ] 对外只读查询当前阶段；变更仅通过 turn manager 授权 API。

---

## Implementation Notes

- 与 `battle-input-touch-and-hit-areas`：输入采样仅在允许阶段被接受（挂钩点可为下一 story）。  
- 与 ADR-0012：`BattleInputPhase.PlayerCommand` 与动作授权对齐（后续 epic）。

---

## Out of Scope

- **Story 002**: `BattleInputPhaseChanged` 事件广播。  
- **Story 003–004**: 时钟与 drain。

---

## QA Test Cases

- **AC-1**: 非法迁移拒绝  
  - Given: 当前阶段 A  
  - When: 请求非法迁到 C（跳过 B）  
  - Then: 拒绝并保持 A；可观测原因  
  - Edge cases: 重入同阶段 no-op

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/turn/input_phase_state_machine_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `battle-event-bus-dto-versioning` story-001（事件类型挂接预留）  
- Unlocks: story-002-battle-input-phase-changed

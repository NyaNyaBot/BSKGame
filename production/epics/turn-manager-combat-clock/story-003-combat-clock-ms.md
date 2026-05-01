# Story 003: CombatClockMs 推进与只读暴露

> **Epic**: turn-manager-combat-clock  
> **Status**: Ready  
> **Layer**: Core  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/回合管理器.md`  
**Requirement**: `TR-turn-003`  

**ADR Governing Implementation**: ADR-0004  
**ADR Decision Summary**: 可变 `CombatClockMs` 仅由 `ICombatClockController`（turn manager）推进；其他系统使用只读 `ICombatClock`；`Advance` 拒绝负 delta，过大 delta clamp；hit stop 暂停逻辑时钟。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH  
**Engine Notes**: `OccurredAtCombatClockMs` 为逻辑时间；测试注入 fake clock。

**Control Manifest Rules (this layer)**:

- **Required**: 非 controller 不得 `Advance`；hit stop 仅由 turn manager 接受。  
- **Forbidden**: 多子系统独立逻辑计时器。  
- **Guardrail**: 单测覆盖 clamp 与 pause/resume。

---

## Acceptance Criteria

- [ ] 实现 `ICombatClock` / `ICombatClockController` 最小集。  
- [ ] 负 `delta` 拒绝；超上限 clamp 到配置值并可有诊断。  
- [ ] hit stop 期间 `NowMs` 冻结（与 ADR-0004 一致）；恢复后继续。

---

## Implementation Notes

- 与 `battle-input-touch-and-hit-areas` story-001：`NowMs` 读数一致。  
- 与事件：`OccurredAtCombatClockMs` 字段来源统一。

---

## Out of Scope

- **Story 004**: 同 tick drain 全编排（与 bus 联调）。  
- 表现层 unscaled Unity 时间（仅文档化接口边界）。

---

## QA Test Cases

- **AC-1**: clamp 行为  
  - Given: `maxStep` 配置为 N  
  - When: `Advance(10*N)`  
  - Then: 实际前进 ≤ N 或按 ADR 精确语义；无负时间  
  - Edge cases: pause 中调用 Advance（拒绝或排队策略按 ADR）

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/turn/combat_clock_ms_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-battle-input-phase-changed（阶段与事件时间一致）  
- Unlocks: story-004-same-tick-drain-ordering

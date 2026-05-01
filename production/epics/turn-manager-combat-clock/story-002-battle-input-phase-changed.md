# Story 002: BattleInputPhaseChanged 事件发布

> **Epic**: turn-manager-combat-clock  
> **Status**: Complete  
> **Layer**: Core  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/回合管理器.md`  
**Requirement**: `TR-turn-002`  

**ADR Governing Implementation**: ADR-0002, ADR-0004  
**ADR Decision Summary**: 阶段变化发布显式 `BattleInputPhaseChanged`（或 GDD 确切命名）；事件携带 `SceneEventContext` 与 `OccurredAtCombatClockMs`；由总线入队规则约束。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM  
**Engine Notes**: 发布不得依赖 UI handler 完成才提交事实。

**Control Manifest Rules (this layer)**:

- **Required**: Owner（turn manager）发布权威事实；DTO 不可变。  
- **Forbidden**: UI 订阅回调反向写阶段状态。  
- **Guardrail**: 同 tick 多阶段变化（若可能）顺序符合 ADR-0004 段 3。

---

## Acceptance Criteria

- [ ] 每次合法阶段迁移发布对应事件，载荷含前后阶段、时钟值、context。  
- [ ] 订阅方在 EditMode 测试可收到确定性序列。  
- [ ] 与 `battle-event-bus-dto-versioning` 的入队策略兼容（handler 内发布不立即重入）。

---

## Implementation Notes

- 事件类型命名与 `tr-registry.yaml` 一致；若 GDD 用中文描述，代码用 PascalCase 英文事件名并在 story 注映射。  
- 与输入系统：监听阶段以启用/禁用采样（集成测试可选）。

---

## Out of Scope

- **Story 001**: 状态机本体。  
- **Story 003–004**: 时钟推进与 drain 编排。

---

## QA Test Cases

- **AC-1**: 迁移序列事件匹配  
  - Given: 脚本驱动 A→B→C  
  - When: drain 队列  
  - Then: 收到两条 `BattleInputPhaseChanged`，顺序与迁移一致  
  - Edge cases: 拒绝迁移时不发布

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/turn/battle_input_phase_changed_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-input-phase-state-machine  
- Unlocks: story-003-combat-clock-ms

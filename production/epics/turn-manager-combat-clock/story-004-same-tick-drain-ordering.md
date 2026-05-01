# Story 004: 同 tick 事件 drain 与 ADR-0004 顺序编排

> **Epic**: turn-manager-combat-clock  
> **Status**: Complete  
> **Layer**: Core  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/回合管理器.md`  
**Requirement**: `TR-turn-004`  

**ADR Governing Implementation**: ADR-0004, ADR-0002  
**ADR Decision Summary**: 同一 `CombatClockMs` tick 内处理顺序：（1）dispose/context（2）pause/hit stop（3）输入阶段变化（4）输入尝试映射（5）弹反（6）伤害（7）死亡（8）counter 窗口（9）UI/反馈（10）debug；与总线队列 drain 协作。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH  
**Engine Notes**: 编排器为单入口；handlers 不得在回调外隐式突变跨阶段状态。

**Control Manifest Rules (this layer)**:

- **Required**: 与 ADR-0002 队列 drain 单点协作；tick-local drain 最大计数（Debug/Test）。  
- **Forbidden**: handler 内绕过队列直接触发下一阶段 gameplay 副作用（除非 ADR 明确允许的同步子步）。  
- **Guardrail**: 低 FPS 仿真测试：单帧多事件仍顺序稳定。

---

## Acceptance Criteria

- [ ] 单一 `TickCombat`（或等价）入口在同一 `CombatClockMs` 内按 ADR-0004 顺序调用子系统/总线 drain。  
- [ ] 集成测试覆盖至少两段顺序依赖（例如：阶段变更事件先于伤害 drain）。  
- [ ] 与 `battle-event-bus-dto-versioning` story-002 联调通过（无双重 drain loop）。

---

## Implementation Notes

- MVP 可先桩（5）（6）子步为 no-op，但顺序占位必须存在，避免后续插入时重排风险。  
- Debug 模式记录每子步处理计数。

---

## Out of Scope

- **Story 001–003**: 阶段机、阶段事件、时钟。  
- 具体弹反/伤害逻辑（各自 epic）。

---

## QA Test Cases

- **AC-1**: 十段顺序探测  
  - Given: 每段注册探针计数器  
  - When: 执行一个 tick  
  - Then: 探针触发严格非降序符合 ADR 列表  
  - Edge cases: 子步抛异常时整 tick 中止策略（按项目错误策略文档化）

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/turn/same_tick_drain_ordering_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-003-combat-clock-ms；`battle-event-bus-dto-versioning` story-002  
- Unlocks: None（本 epic 内）

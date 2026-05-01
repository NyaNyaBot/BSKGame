# Story 002: 入队与同 tick 确定性 drain

> **Epic**: battle-event-bus-dto-versioning  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/回合管理器.md`  
**Requirement**: `TR-turn-002`、`TR-turn-004`（与总线协同部分）  

**ADR Governing Implementation**: ADR-0002, ADR-0004  
**ADR Decision Summary**: 处理器执行期间发布的事件入队、不立即重入；由 ADR-0004 权威在同一 `CombatClockMs` tick 内按优先级与入队次序 drain；总线不得拥有独立 gameplay drain 循环。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（低 FPS / 同 tick 多事件）  
**Engine Notes**: 与 `ICombatClockController` / turn manager 的挂钩点需单点文档化。

**Control Manifest Rules (this layer)**:

- **Required**: 递归发布深度与每 tick 事件数守卫；drain 顺序服从 ADR-0004 十段顺序。  
- **Forbidden**: 事件总线内独立 gameplay drain 与 ADR-0004 分叉。  
- **Guardrail**: Debug/Test 下 tick-local drain 最大次数防循环。

---

## Acceptance Criteria

- [ ] 处理器内 `Publish` 将事件入队至当前 tick 队列尾部（或 ADR 指定结构）。  
- [ ] Drain 调用方（turn/combat tick runner）按 ADR-0004 顺序调用总线 drain。  
- [ ] 可测：同 tick 内多事件顺序稳定、与手动推演一致。  
- [ ] 超递归深度或超每 tick 上限时触发可诊断失败（Debug）或安全截断（Release 策略按 ADR）。

---

## Implementation Notes

- 与 `turn-manager-combat-clock` story-004 联调：单一「drain 编排」入口。  
- 事件轨迹：记录 enqueue 序、drain 序、source event id（Debug）。

---

## Out of Scope

- **Story 001**: DTO 形状本身。  
- **Story 003**: HybridCLR 泛型保留。  
- 具体伤害/弹反业务事件载荷（由对应 Core epic 填充类型）。

---

## QA Test Cases

- **AC-1**: 同 tick 顺序确定性  
  - Given: 伪造时钟固定在同一 `CombatClockMs`  
  - When: handler A 内发布 B、C；再 drain  
  - Then: 处理顺序符合 ADR-0004 与入队规则  
  - Edge cases: 嵌套三层 publish；空队列 drain

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/battle_events/event_drain_ordering_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-battle-event-dto-and-context；`turn-manager-combat-clock` story-003（时钟 tick）建议同迭代联调  
- Unlocks: story-003-hybridclr-generic-bus-bridge

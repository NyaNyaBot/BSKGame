# Story 002: ParryAttempt 结构化输出与诊断

> **Epic**: battle-input-touch-and-hit-areas  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/输入系统.md`  
**Requirement**: `TR-input-002`  

**ADR Governing Implementation**: ADR-0006, ADR-0007（下游消费预备）  
**ADR Decision Summary**: 弹反/反击判定须源自 touch/PointerDown 采样及诊断元数据；输出结构化 `ParryAttempt` 供 `IParryResolver` 消费。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH  
**Engine Notes**: 不在本 story 实现完整 `IParryResolver`；仅保证输入侧契约稳定。

**Control Manifest Rules (this layer)**:

- **Required**: PointerDown 为战斗输入源；诊断字段可测试。  
- **Forbidden**: UGUI `Button.onClick` 作为判定时序源。  
- **Guardrail**: 结构化 payload 保持 DTO/值语义，无 Unity 引用。

---

## Acceptance Criteria

- [ ] 定义 `ParryAttempt`（或等价）包含 GDD/ADR 要求的触摸与时间字段、区域 id、`stateVersion` 等。  
- [ ] 从采样到 `ParryAttempt` 的管线可被 EditMode 测试驱动（无真实触摸亦可）。  
- [ ] 与 `InputHitAreaRegistry` 的 dispatch 接口衔接（可为接口桩）。

---

## Implementation Notes

- 与 `battle-event-bus-dto-versioning` 协调：若 `ParryAttempt` 先以请求对象进入 domain，避免循环依赖。  
- 诊断字段分级：Debug 全量 / Release 采样。

---

## Out of Scope

- **Story 001**: 时间戳映射公式。  
- **Story 003**: 区域优先级与遮挡。  
- 弹反窗口几何与 timeline（ADR-0007 另 epic）。

---

## QA Test Cases

- **AC-1**: 合成输入产生合法 `ParryAttempt`  
  - Given: 假 PointerDown + registry 命中  
  - When: 构建 attempt  
  - Then: 字段完整；时间戳与 story-001 一致  
  - Edge cases: 未命中任何区域；多区域重叠（留 story-003）

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/input/parry_attempt_pipeline_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-touch-timestamp-combat-clock  
- Unlocks: story-003-hit-area-registry

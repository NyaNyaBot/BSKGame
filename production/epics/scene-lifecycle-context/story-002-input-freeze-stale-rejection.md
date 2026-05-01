# Story 002: 场景切换期间输入冻结与迟到拒绝

> **Epic**: scene-lifecycle-context  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/场景管理.md`  
**Requirement**: `TR-scene-002`  

**ADR Governing Implementation**: ADR-0001, ADR-0002  
**ADR Decision Summary**: 场景管理在销毁战斗运行态前冻结战斗输入；事件总线路径上拒绝 context 不匹配或过期的回调/事件。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM（WebGL 异步回调）  
**Engine Notes**: 与事件入队/ drain 顺序协作，避免在 teardown 中重入 gameplay。

**Control Manifest Rules (this layer)**:

- **Required**: 场景切换前冻结战斗输入；禁止各系统各自维护独立的「场景激活」标志。  
- **Forbidden**: 跨系统直链调用链替代上下文守卫。  
- **Guardrail**: 与 ADR-0004 drain 协作时不得在总线内独立 gameplay drain。

---

## Acceptance Criteria

- [ ] 场景切换流程中，战斗输入在配置时点被冻结（与 GDD Acceptance 对齐）。  
- [ ] 携带过期 `sceneVersion` / `battleVersion` 的事件被拒绝或丢弃并可有诊断轨迹。  
- [ ] 与 `battle-event-bus-dto-versioning` 的 context 过滤行为一致、无双重语义。

---

## Implementation Notes

- 冻结应早于 dispose 副作用；与输入适配器/总线订阅清理顺序在 ADR-0001 指引下文档化。  
- 拒绝路径优先使用 ADR-0002 的一等拒绝/取消事件形态（若本 story 仅实现守卫，可发内部 trace）。

---

## Out of Scope

- **Story 001**: context 身份模型本身。  
- **Story 003**: 临时态对象生命周期与具体清理列表。

---

## QA Test Cases

- **AC-1**: 切换中迟到事件被拒绝  
  - Given: 模拟异步延迟回调携带旧 `sceneVersion`  
  - When: 总线或入口守卫处理  
  - Then: 不产生 gameplay 突变；可观测拒绝原因  
  - Edge cases: 同 tick 多事件；边界 version 相等 vs 小于

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/scene/scene_transition_input_freeze_test.cs` 或与 bus epic 联合的集成测试  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-scene-context-identity  
- Unlocks: story-003-battle-transient-lifecycle

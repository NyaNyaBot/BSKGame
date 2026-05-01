# Story 003: 战斗临时运行态创建与销毁

> **Epic**: scene-lifecycle-context  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/场景管理.md`  
**Requirement**: `TR-scene-003`  

**ADR Governing Implementation**: ADR-0001  
**ADR Decision Summary**: 在战斗场景生命周期内创建/销毁战斗临时运行态；卸载时清理订阅与临时数据，适配 WebGL 内存压力。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM（WebGL）  
**Engine Notes**: 避免泄漏：总线订阅、计时器、输入注册须在 battle dispose 路径释放。

**Control Manifest Rules (this layer)**:

- **Required**: dispose 前冻结输入；版本递增与 teardown 顺序符合 ADR-0001。  
- **Forbidden**: 用 `GameObject` 活跃性作为是否处理事件的唯一依据。  
- **Guardrail**: teardown 侧效应与事件 drain 不形成无限递归。

---

## Acceptance Criteria

- [ ] 进入战斗场景时创建文档化的「战斗临时态」根对象或服务集合。  
- [ ] 离开/重开战斗时上述对象全部释放，无残留订阅（可测）。  
- [ ] WebGL/Editor 行为一致或可解释差异记录在实现注释或 `tests/evidence/`。

---

## Implementation Notes

- 与 `battle-event-bus-dto-versioning` 的订阅注册点约定：由 battle scope 拥有注册生命周期。  
- 清单式 dispose（订阅、registry、clock 绑定）便于 Code Review。

---

## Out of Scope

- **Story 001–002**: context 与冻结/拒绝路径。  
- 具体玩法实体池化（属后续 Feature）。

---

## QA Test Cases

- **AC-1**: dispose 后无订阅泄漏  
  - Given: 进入战斗再退出两次  
  - When: 查询总线/注册表订阅数或弱引用探针  
  - Then: 恢复基线；无重复处理旧 battle 事件  
  - Edge cases: 异常中途退出；域重载（Editor）

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/scene/battle_transient_lifecycle_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-input-freeze-stale-rejection  
- Unlocks: None（本 epic 内）

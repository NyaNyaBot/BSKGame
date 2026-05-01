# Story 003: InputHitAreaRegistry 优先级、遮挡与去重

> **Epic**: battle-input-touch-and-hit-areas  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/输入系统.md`  
**Requirement**: `TR-input-004`  

**ADR Governing Implementation**: ADR-0006, ADR-0010（UI 协同）  
**ADR Decision Summary**: `InputHitAreaRegistry` 拥有注册区域、优先级、遮挡、`stateVersion`、重复过滤；Counter 300 > Modal 250 > Standard 200 > Parry 100 > Background 0；`CounterEntryOpened` 激活时反击区高于弹反区且弹反区不得 `BlocksUnderlying=true` 等。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW（MVP 区域数小）  
**Engine Notes**: 命中测试按优先级排序；O(活跃区域)。

**Control Manifest Rules (this layer)**:

- **Required**: 优先级常量与 ADR 一致；stale `stateVersion` 忽略。  
- **Forbidden**: 各 UI 面板私自实现战斗命中栈。  
- **Guardrail**: MVP 限制最大注册区域数并在 Debug 断言。

---

## Acceptance Criteria

- [ ] 注册/注销 API；命中返回单一最高优先有效区域或明确的多命中策略（与 GDD 一致）。  
- [ ] `CounterEntryOpened` 行为与 ADR-0006 冲突规则有自动化用例。  
- [ ] `BlocksUnderlying` 与 `stateVersion`  stale 路径有覆盖。

---

## Implementation Notes

- 与 UI 协作：`CounterEntryOpened` 状态可由 turn/parry epic 注入 registry 的策略对象，而非 UI 直接改 registry 内部集合（按架构偏好二选一并文档化）。  
- 输出去向：产生 domain 事件或 `ParryAttempt`（与 story-002 衔接）。

---

## Out of Scope

- **Story 001–002**: 时间戳与 ParryAttempt 形状。  
- 全屏 UI 布局与美术（Presentation）。

---

## QA Test Cases

- **AC-1**: 优先级正确  
  - Given: Parry(100) 与 Counter(300) 重叠且 counter 模式开  
  - When: hit test  
  - Then: 选中 Counter 区域  
  - Edge cases: 同优先同 z-order；注销后回退

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/input/input_hit_area_registry_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-parry-attempt-structure  
- Unlocks: None（本 epic 内）

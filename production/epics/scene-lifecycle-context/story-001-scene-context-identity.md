# Story 001: 场景与战斗上下文身份与版本

> **Epic**: scene-lifecycle-context  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/场景管理.md`  
**Requirement**: `TR-scene-001`  
*(条目以 `docs/architecture/tr-registry.yaml` 为准；评审时重新读取。)*

**ADR Governing Implementation**: ADR-0001: 场景生命周期与场景上下文路由  
**ADR Decision Summary**: 使用 `SceneContext` / `BattleContext` / `SceneEventContext` 与版本递增作为跨系统事件的权威身份，禁止仅用 Unity 场景名/句柄判断当前性。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW（Unity）；MEDIUM（WebGL 内存与清理）  
**Engine Notes**: 以 ID/版本做常量级上下文校验；重试与多战斗身份需一致模型。

**Control Manifest Rules (this layer)**:

- **Required**: 维护权威 `SceneContext` 与嵌套 `BattleContext`；跨系统战斗事件携带 `SceneEventContext`（`sceneContextId`、`sceneVersion`、`battleContextId`、`battleVersion`）。  
- **Forbidden**: 仅用 Unity active scene 名/句柄拒绝 stale 事件。  
- **Guardrail**: 上下文检查常数时间；DTO 携带上下文字段在 MVP 体量可接受。

---

## Acceptance Criteria

- [ ] 存在可查询的 `SceneContextId`，每次进入场景（含重试）会变更或按 ADR 约定递增身份。  
- [ ] `sceneVersion` 在 teardown 副作用完成前按 ADR 递增。  
- [ ] `BattleContext` 与 `battleVersion` 在战斗 dispose/restart 时递增。  
- [ ] 事件或回调可携带 `SceneEventContext` 供下游过滤。

---

## Implementation Notes

- 实现 `ISceneContextService`（或等价）作为单一真相源；订阅者在入队/处理前比对 context。  
- 与 ADR-0002 事件 DTO 上的 `SceneEventContext` 字段对齐，避免两套字段语义分叉。

---

## Out of Scope

- **Story 002**: 切换期间输入冻结与迟到拒绝策略。  
- **Story 003**: 战斗临时态创建/销毁与资源清理细节。

---

## QA Test Cases

- **AC-1**: 场景进入后 context id/version 可观测且与 ADR 一致  
  - Given: 干净启动后进入战斗场景  
  - When: 记录 `sceneContextId` / `sceneVersion` / `battleVersion`  
  - Then: 与 GDD/ADR 描述一致；重试后 id 或 version 按约定变化  
  - Edge cases: 快速连续进入/退出；同场景重载

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/scene/scene_context_identity_test.cs`（或 `client/Assets/Tests/` 下等价路径）— 实现后须存在并通过  

**Status**: [x] `client/Assets/Tests/EditMode/Scene/SceneContextIdentityTests.cs` — 5 test functions, 6/6 passed (2026-05-01)

---

## Dependencies

- Depends on: None  
- Unlocks: story-002-input-freeze-stale-rejection

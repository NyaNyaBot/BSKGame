# Story 001: 程序集边界与 UnityEngine 隔离

> **Epic**: gameplay-unity-hybridclr-layering  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/game-concept.md`  
**Requirement**: `TR-concept-001`、`TR-concept-002`  

**ADR Governing Implementation**: ADR-0003  
**ADR Decision Summary**: 三层程序集：纯玩法无 `UnityEngine`；Unity 热更适配器；Launch/AOT 仅引导与 HybridCLR 加载、无战斗规则逻辑。

**Engine**: Unity 2022.3.17f1 + HybridCLR | **Risk**: HIGH  
**Engine Notes**: asmdef 引用方向单向；禁止 domain 程序集引用 Unity 层。

**Control Manifest Rules (this layer)**:

- **Required**: 纯 gameplay/domain 无 `UnityEngine`；核心规则不在仅 AOT 的 Launch 层实现。  
- **Forbidden**: 为省事把 domain 与 MonoBehaviour 混在同一程序集导致 `UnityEngine` 泄漏。  
- **Guardrail**: EditMode 测试可在 domain 程序集运行。

---

## Acceptance Criteria

- [ ] `game.gameplay`（或等价 domain 程序集）编译图不引用 `UnityEngine`。  
- [ ] 热更适配器程序集明确引用 domain + Unity。  
- [ ] Launch/AOT 程序集不包含战斗规则类（grep/架构测试守护）。

---

## Implementation Notes

- 若现有结构部分违规，本 story 负责引入 asmdef 拆分或移动文件的最小集，不扩大 Feature 范围。  
- 与仓库 `CopyDLL.bat` / 现有 HybridCLR 流程兼容。

---

## Out of Scope

- **Story 002**: DTO 跨边界字段类型约束测试。  
- **Story 003**: 完整 smoke 与证据文档。

---

## QA Test Cases

- **AC-1**: 编译图检查  
  - Given: CI 或本地脚本扫描 asmdef 引用  
  - When: 构建 domain  
  - Then: 无 `UnityEngine` 引用失败则构建失败  
  - Edge cases: 条件编译 `UNITY_EDITOR` 误放入 domain

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: 脚本或 `tests/integration/architecture/no_unityengine_in_gameplay_test.cs`（按项目惯例）  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None  
- Unlocks: story-002-dto-boundary-primitives

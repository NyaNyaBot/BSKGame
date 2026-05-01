# Story 002: 跨边界 DTO 仅基元与稳定 ID

> **Epic**: gameplay-unity-hybridclr-layering  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/game-concept.md`  
**Requirement**: `TR-concept-002`  

**ADR Governing Implementation**: ADR-0003  
**ADR Decision Summary**: 跨越 gameplay/Unity 边界的 DTO 仅允许基元、字符串、枚举、项目内纯数学类型；稳定 ID，无 Unity 对象引用。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: 与 ADR-0002「DTO 无 Unity 引用」一致，本 story 强调「跨程序集边界」静态约束。

**Control Manifest Rules (this layer)**:

- **Required**: DTO 跨边界字段类型白名单执行（分析器/单元测试/代码评审清单三选一或组合）。  
- **Forbidden**: 将 `UnityEngine.Object` 或场景引用塞入热更↔domain DTO。  
- **Guardrail**: 新增字段必须经过白名单检查。

---

## Acceptance Criteria

- [ ] 文档化「允许类型」列表，并在仓库中有可执行检查（测试或 Roslyn 分析器其一）。  
- [ ] MVP 跨边界 DTO（若已存在）通过检查或在本 story 内重构为合规形状。  
- [ ] 违规时构建失败或在 PR 检查中失败。

---

## Implementation Notes

- 优先复用现有类型与命名空间约定（`Game.Client` / `Game.Gameplay` 等）。  
- 与 `battle-event-bus-dto-versioning` story-001 对齐字段策略。

---

## Out of Scope

- **Story 001**: asmdef 大图景。  
- 网络同步 DTO（若未来有）另行 epic。

---

## QA Test Cases

- **AC-1**: 白名单拒绝非法字段类型  
  - Given: 含 `Texture2D` 引用的假 DTO（测试专用）  
  - When: 运行检查  
  - Then: 失败并指出类型  
  - Edge cases: `UnityEngine.Mathf` vs 项目纯数学 Vector 替身

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/architecture/dto_boundary_types_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-assembly-boundaries  
- Unlocks: story-003-hybridclr-smoke-checklist

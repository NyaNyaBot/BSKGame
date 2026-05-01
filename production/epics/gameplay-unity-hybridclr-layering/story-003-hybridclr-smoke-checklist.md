# Story 003: HybridCLR / AOT smoke 与保留清单

> **Epic**: gameplay-unity-hybridclr-layering  
> **Status**: Deferred — 延后到发布前 WebGL 冒烟验证；当前热更层内泛型由 HybridCLR 解释器处理，无需手动保留  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/game-concept.md`  
**Requirement**: `TR-concept-001`  

**ADR Governing Implementation**: ADR-0003  
**ADR Decision Summary**: 跨 HybridCLR 边界的泛型服务、事件 DTO、反射可见类型需 link/AOT 补充策略；提供可重复的最小 smoke。

**Engine**: Unity 2022.3.17f1 + HybridCLR | **Risk**: HIGH  
**Engine Notes**: 与项目已选 WebGL/HybridCLR 工作流一致；证据可 Editor 为主若 CI 未就绪。

**Control Manifest Rules (this layer)**:

- **Required**: 泛型与反射可见类型有维护中的保留策略（link.xml / HybridCLR Generate/补充列表）。  
- **Forbidden**: WebGL/AOT 路径上的未保留反射构造。  
- **Guardrail**: smoke 仅覆盖启动 + 加载热更 + 调用 1 个 domain API，控制时间成本。

---

## Acceptance Criteria

- [ ] `tests/evidence/` 或等价路径存在一份 **HybridCLR smoke checklist**（日期、引擎版本、步骤、结果）。  
- [ ] 清单包含：打开工程、生成/补充 AOT、运行指定菜单或测试、期望日志关键字。  
- [ ] 与 `battle-event-bus-dto-versioning` story-003 可交叉引用同一 smoke 段落（避免重复维护时注明主副本）。

---

## Implementation Notes

- 不强制本 story 完成全量 WebGL CI；以可落地证据为准。  
- 若仓库已有 `tests/evidence/unity-editor-smoke-*.md`，扩展一节而非另起炉灶。

---

## Out of Scope

- **Story 001–002**: 边界与 DTO 白名单。  
- 微信导出全链路（属 `bsk-wx-minigame` 后续）。

---

## QA Test Cases

- **AC-1**: Checklist 可执行  
  - Given: 新机器按清单操作  
  - When: 执行到「调用 domain API」步  
  - Then: 无 MissingMethod/AOT 典型错误；结果记录在证据文件  
  - Edge cases: 域重载后重复一步

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/evidence/hybridclr-layering-smoke-2026-05-01.md`（文件名可随日期调整）  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-dto-boundary-primitives  
- Unlocks: None（本 epic 内）

# Story 003: HybridCLR 泛型 Publish/Subscribe 调用点保留

> **Epic**: battle-event-bus-dto-versioning  
> **Status**: Deferred — 延后到发布前 WebGL 冒烟验证；Publish<T>/Subscribe<T> 在热更层内部，HybridCLR 解释器直接处理  
> **Layer**: Foundation  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/game-concept.md`（热更边界）  
**Requirement**: `TR-concept-001`（架构可热更）  

**ADR Governing Implementation**: ADR-0002, ADR-0003  
**ADR Decision Summary**: 热更代码中的 `Publish<T>` / `Subscribe<T>` 调用点需保留泛型以兼容 HybridCLR；跨边界类型需 AOT 泛型补充或 link 保留。

**Engine**: Unity 2022.3.17f1 + HybridCLR | **Risk**: HIGH（HybridCLR + WebGL）  
**Engine Notes**: 禁止在 AOT 路径使用未保留的反射构造；遵循项目 HybridCLR 配置。

**Control Manifest Rules (this layer)**:

- **Required**: 保留热更侧泛型总线 API；跨边界 DTO 仅基元/字符串/枚举/项目纯数学类型。  
- **Forbidden**: `Expression.Emit` / `DynamicMethod` / 未保留反射构造（WebGL/AOT）。  
- **Guardrail**: 最小化热更程序集中事件 DTO 类型集合以降低 AOT 表面积。

---

## Acceptance Criteria

- [ ] 热更程序集中存在稳定的 `Publish<TEvent>` / `Subscribe<TEvent>` 入口且被实际引用（非死代码消除）。  
- [ ] 文档或 checklist 列出需在 `link.xml`/HybridCLR 补充的泛型组合（按当前 DTO 集合）。  
- [ ] 在目标平台配置下可执行最小 smoke（见 `tests/evidence/` 或与 `gameplay-unity-hybridclr-layering` 共用证据）。

---

## Implementation Notes

- 与 `gameplay-unity-hybridclr-layering` story-003 对齐证据路径，避免重复造轮子。  
- DTO 程序集划分满足 ADR-0003：热更层可引用 DTO，AOT 启动层不承载战斗规则。

---

## Out of Scope

- 完整 CI WebGL 构建流水线（若未就绪则 story 证据可为 Editor + 清单）。  
- 所有业务事件类型的实现（随 Feature 迭代补充 link）。

---

## QA Test Cases

- **AC-1**: 泛型入口不被剥离  
  - Given: IL2CPP/HybridCLR 预检配置  
  - When: 分析 strip 日志或运行最小订阅  
  - Then: `Publish`/`Subscribe` 对 MVP 事件类型可用  
  - Edge cases: 新增事件类型时 checklist 更新责任方

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/hybridclr/event_bus_generic_smoke_test.cs` 和/或 `tests/evidence/hybridclr-event-bus-smoke.md`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-enqueue-deterministic-drain；`gameplay-unity-hybridclr-layering` story-001–002  
- Unlocks: None（本 epic 内）

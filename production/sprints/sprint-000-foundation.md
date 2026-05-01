# Sprint 000: Foundation 基础设施

> **Status**: Ready
> **Duration**: 1 周（Pre-Production Sprint 0）
> **Goal**: 实现 Foundation 层全部 Story + Core 层场景上下文验证，建立战斗系统的基础契约
> **Milestone**: Pre-Production — 验证核心基础设施可编码

---

## Sprint Scope

### Foundation 层 Stories（必须完成）

| # | Epic | Story | Type | Path | Priority |
|---|---|---|---|---|---|
| 1 | scene-lifecycle-context | story-001 场景上下文身份 | Logic | `production/epics/scene-lifecycle-context/story-001-scene-context-identity.md` | P0 |
| 2 | scene-lifecycle-context | story-002 输入冻结与迟到事件拒绝 | Integration | `production/epics/scene-lifecycle-context/story-002-input-freeze-stale-rejection.md` | P0 |
| 3 | scene-lifecycle-context | story-003 战斗临时生命周期 | Integration | `production/epics/scene-lifecycle-context/story-003-battle-transient-lifecycle.md` | P0 |
| 4 | battle-event-bus-dto-versioning | story-001 战斗事件 DTO 与上下文 | Logic | `production/epics/battle-event-bus-dto-versioning/story-001-battle-event-dto-and-context.md` | P0 |
| 5 | battle-event-bus-dto-versioning | story-002 入队与确定性 drain | Logic | `production/epics/battle-event-bus-dto-versioning/story-002-enqueue-deterministic-drain.md` | P0 |
| 6 | battle-event-bus-dto-versioning | story-003 HybridCLR 泛型总线桥接 | Integration | `production/epics/battle-event-bus-dto-versioning/story-003-hybridclr-generic-bus-bridge.md` | P1 |
| 7 | gameplay-unity-hybridclr-layering | story-001 程序集边界 | Logic | `production/epics/gameplay-unity-hybridclr-layering/story-001-assembly-boundaries.md` | P0 |
| 8 | gameplay-unity-hybridclr-layering | story-002 DTO 边界基元 | Logic | `production/epics/gameplay-unity-hybridclr-layering/story-002-dto-boundary-primitives.md` | P0 |
| 9 | gameplay-unity-hybridclr-layering | story-003 HybridCLR 冒烟清单 | Integration | `production/epics/gameplay-unity-hybridclr-layering/story-003-hybridclr-smoke-checklist.md` | P1 |
| 10 | battle-input-touch-and-hit-areas | story-001 触摸时间戳与战斗时钟 | Logic | `production/epics/battle-input-touch-and-hit-areas/story-001-touch-timestamp-combat-clock.md` | P0 |
| 11 | battle-input-touch-and-hit-areas | story-002 弹反尝试结构 | Logic | `production/epics/battle-input-touch-and-hit-areas/story-002-parry-attempt-structure.md` | P0 |
| 12 | battle-input-touch-and-hit-areas | story-003 触区注册表 | Integration | `production/epics/battle-input-touch-and-hit-areas/story-003-hit-area-registry.md` | P0 |

### 平台验证 Stories（附加目标）

| # | Task | Type | Priority |
|---|---|---|---|
| 13 | HybridCLR 热更加载冒烟测试 | Evidence | P1 |
| 14 | WebGL 构建冒烟验证 | Evidence | P1 |

---

## 实现顺序（依赖安全）

```
阶段 1: scene-lifecycle-context story-001 + battle-event-bus story-001
         （场景上下文 + 事件 DTO 契约，零依赖）
    ↓
阶段 2: gameplay-unity-hybridclr story-001 + story-002
         （程序集边界 + DTO 基元，依赖事件 DTO 定义）
    ↓
阶段 3: battle-event-bus story-002 + scene-lifecycle story-002
         （入队/drain + 输入冻结，依赖事件总线 + 场景上下文）
    ↓
阶段 4: battle-input story-001 + story-002
         （时间戳适配 + ParryAttempt，依赖战斗时钟接口）
    ↓
阶段 5: battle-input story-003 + scene-lifecycle story-003
         （触区注册表 + 战斗临时生命周期，集成测试）
    ↓
阶段 6: hybridclr story-003 + event-bus story-003
         （HybridCLR 冒烟 + 泛型桥接，平台验证）
```

---

## Definition of Done

- [ ] Foundation 层 12 个 Story 全部 Complete（或 P1 标记为 gap）
- [ ] 所有 Logic Story 有对应 `tests/unit/` 测试文件且通过
- [ ] 所有 Integration Story 有对应 `tests/integration/` 测试文件且通过
- [ ] Unity Editor 编译无错误
- [ ] EditMode 测试全部通过
- [ ] HybridCLR 泛型保留已验证（或标记 gap）

---

## Risks

| Risk | Severity | Mitigation |
|---|---|---|
| HybridCLR 泛型 `Publish<T>/Subscribe<T>` 保留失败 | HIGH | story-003 专门验证；失败则回退到非泛型 bus |
| WebGL 触摸时间戳精度不足 | HIGH | story-001 含 fallback 诊断；真机证据延后 |
| 程序集边界导致编译循环引用 | MEDIUM | story-001 先验证三层程序集编译 |

---

## Next Sprint

Sprint 001: Core 层（character-schema-snapshot + turn-manager-combat-clock + damage-hp-ownership）

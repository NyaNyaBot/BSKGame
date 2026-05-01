# Epic: 玩法逻辑、Unity 适配器与 HybridCLR 分层

> **Layer**: Foundation  
> **GDD**: `design/gdd/game-concept.md`  
> **Architecture Module**: Platform layering — Pure gameplay / Unity hot-update adapter / Launch-AOT  
> **Status**: Ready  
> **Stories**: see table below

## Overview

固化三层运行时程序集边界：纯玩法无 `UnityEngine`；Unity 热更层做适配与表现；Launch/AOT 仅引导与 HybridCLR 加载；DTO 跨边界仅用基元与稳定 ID；为 HybridCLR/AOT 维护泛型与反射可见类型的 link/AOT 补充策略。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0003: 玩法逻辑、Unity 适配器与 HybridCLR 的运行时程序集分层 | 三层模型与禁止项 | LOW（Unity）；HIGH（HybridCLR + WebGL） |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-concept-001 | 支持“回合制 RPG + 敌方攻击中嵌入实时弹反”的核心循环（架构可热更/可测） | ADR-0003 |
| TR-concept-002 | 表现层放大快感但不得成为规则事实来源（分层 enforcement） | ADR-0003, ADR-0009 |

## Definition of Done

- `game.gameplay` / `game.core`（或等价程序集）编译无 `UnityEngine` 引用  
- 热更与 AOT 边界有最小 smoke（见 story-003 与 `tests/evidence/`）  

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 程序集边界与 UnityEngine 隔离 | Integration | Ready | ADR-0003 |
| 002 | 跨边界 DTO 仅基元与稳定 ID | Logic | Ready | ADR-0003 |
| 003 | HybridCLR / AOT smoke 与保留清单 | Integration | Ready | ADR-0003 |

## Next Step

完成本 epic 后再大规模展开 Feature 层 combat 逻辑。

# Epic: 战斗事件总线与 DTO 版本策略

> **Layer**: Foundation  
> **GDD**: `design/gdd/回合管理器.md`（主锚点：阶段与事件广播；契约亦服务 `场景管理` / `伤害` / `弹反` / `反馈` / `战斗 UI`）  
> **Architecture Module**: Battle Event Bus（`IBattleEvent` / `IBattleEventBus`）  
> **Status**: Ready  
> **Stories**: see table below

## Overview

实现轻量同步进程内战斗事件总线：不可变 DTO、上下文过滤、入队/同 tick 确定性 drain 与 ADR-0004 排序策略对接；禁止事件总线独立拥有 gameplay drain loop；为 HybridCLR 保留泛型 `Publish`/`Subscribe` 调用点。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0002: 战斗事件总线与 DTO 版本策略 | 不可变事件、幂等、入队、与 drain 协作 | LOW；MEDIUM（WebGL 回调/域重载） |
| ADR-0004: 战斗时钟与确定性战斗排序 | 同 tick drain 优先级权威 | LOW；HIGH（低 FPS / 触摸时序） |

## GDD Requirements（本 epic 直接交付的 TR 子集）

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-turn-002 | 发布显式 `BattleInputPhaseChanged` 事件 | ADR-0002, ADR-0004 |
| TR-scene-002 | 场景切换期间冻结战斗输入并拒绝迟到事件（事件路径） | ADR-0001, ADR-0002 |
| TR-dmg-004 | 发出 `DamageApplied` / `DamageRejected` / `CharacterDefeated`（事件形态） | ADR-0002, ADR-0005 |
| TR-parry-003 | 发出 `ParryResolved` 等（事件形态） | ADR-0002, ADR-0007 |
| TR-fx-001 | 反馈只消费事件（契约侧） | ADR-0002, ADR-0009 |
| TR-ui-001 | UI 从事件与快照渲染（契约侧） | ADR-0002, ADR-0010 |

> 伤害数值权威、弹反判定等 **业务语义** 分别在 `damage-hp-ownership` 与后续 Feature epic 实现；本 epic 负责 **事件总线形态与排序挂钩**。

## Definition of Done

- 事件 DTO 与总线行为满足 ADR-0002 / ADR-0004 的约束与测试清单（含递归/同 tick drain 守卫）  
- EditMode 可验证事件顺序与 stale context 丢弃  

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 战斗事件 DTO 与 SceneEventContext | Logic | Ready | ADR-0002 |
| 002 | 入队与同 tick 确定性 drain | Integration | Ready | ADR-0002, ADR-0004 |
| 003 | HybridCLR 泛型 Publish/Subscribe 调用点保留 | Integration | Ready | ADR-0002, ADR-0003 |

## Next Step

实现 story-001 → story-003；与 `turn-manager-combat-clock` epic 联调 drain 顺序。

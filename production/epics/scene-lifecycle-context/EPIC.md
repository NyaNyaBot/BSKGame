# Epic: 场景生命周期与上下文路由

> **Layer**: Foundation  
> **GDD**: `design/gdd/场景管理.md`  
> **Architecture Module**: Scene Management (`SceneContext` / `BattleContext` / `SceneEventContext`)  
> **Status**: Ready  
> **Stories**: see table below

## Overview

实现权威场景与战斗上下文身份、版本递增与切换流程，使跨系统战斗事件可统一拒绝迟到/stale 回调；场景切换前冻结战斗输入，并在卸载时清理战斗临时态，满足 WebGL/微信小游戏下异步与延迟回调场景。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0001: 场景生命周期与场景上下文路由 | `SceneContext` / `BattleContext` / `SceneEventContext` 与 `ISceneContextService` 契约 | LOW（Unity）；MEDIUM（WebGL 内存与清理） |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-scene-001 | 拥有 `SceneContext`、`sceneContextId` 和 `sceneVersion` | ADR-0001 |
| TR-scene-002 | 场景切换期间冻结战斗输入并拒绝迟到事件 | ADR-0001, ADR-0002 |
| TR-scene-003 | 在战斗场景生命周期内创建和销毁战斗临时运行态 | ADR-0001 |

## Definition of Done

- 本 epic 下全部 story 经 `/story-done` 关闭且验收满足 GDD Acceptance Criteria 中与本模块相关的条目  
- Logic / Integration story 在 `tests/unit/` 或 `client/Assets/Tests/` 有对应自动化证据  
- WebGL 场景重试/stale 拒绝证据按 `tests/evidence/` 模板补齐（可与输入 epic 联合取证）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 场景与战斗上下文身份与版本 | Integration | Ready | ADR-0001 |
| 002 | 场景切换期间输入冻结与迟到拒绝 | Integration | Ready | ADR-0001, ADR-0002 |
| 003 | 战斗临时运行态创建与销毁 | Logic | Ready | ADR-0001 |

## Next Step

拆分子任务已完成；实现时按 `story-001` → `story-003` 顺序推进。

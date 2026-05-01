# Epic: 移动触摸时间戳与输入触区

> **Layer**: Foundation  
> **GDD**: `design/gdd/输入系统.md`  
> **Architecture Module**: Input System（`TouchTimestampAdapter` / `InputHitAreaRegistry`）  
> **Status**: Ready  
> **Stories**: see table below

## Overview

实现集中式触摸时间戳映射与 `CombatClockMs` 对齐、桥接延迟诊断、`ParryAttempt` 结构化输出，以及触区注册、优先级、遮挡与去重；弹反/counter 判定不得依赖 UGUI `onClick` 合成时序。

## Governing ADRs


| ADR                      | Decision Summary            | Engine Risk             |
| ------------------------ | --------------------------- | ----------------------- |
| ADR-0006: 移动触摸时间戳与输入触区策略 | 时间戳适配器 + 注册表 + 优先级          | LOW；HIGH（微信 WebGL 触摸时序） |
| ADR-0001                 | 事件与输入携带 `SceneEventContext` | MEDIUM（WebGL）           |
| ADR-0004                 | 时间戳映射消费 `CombatClockMs`     | HIGH（低 FPS）             |


## GDD Requirements


| TR-ID        | Requirement                           | ADR Coverage       |
| ------------ | ------------------------------------- | ------------------ |
| TR-input-001 | Touch / PointerDown 为战斗输入源            | ADR-0006           |
| TR-input-002 | 结构化 `ParryAttempt` 与诊断                | ADR-0006, ADR-0007 |
| TR-input-003 | 平台时间戳映射到 `CombatClockMs` 并标记 fallback | ADR-0004, ADR-0006 |
| TR-input-004 | 触区注册、优先级、遮挡、去重                        | ADR-0006, ADR-0010 |


## Definition of Done

- GDD「Core Rules」1–10 中与 MVP 输入路径相关的自动化用例通过  
- `tests/evidence/` 中记录微信/WebGL 触摸证据清单（可占位路径，随原型补齐）

## Stories


| #   | Story                          | Type        | Status | ADR                |
| --- | ------------------------------ | ----------- | ------ | ------------------ |
| 001 | 触摸时间戳映射到 CombatClockMs         | Logic       | Ready  | ADR-0006, ADR-0004 |
| 002 | ParryAttempt 结构化输出与诊断          | Integration | Ready  | ADR-0006           |
| 003 | InputHitAreaRegistry 优先级、遮挡与去重 | Logic       | Ready  | ADR-0006           |


## Next Step

与 `scene-lifecycle-context`、`turn-manager-combat-clock` 联调输入阶段切换。
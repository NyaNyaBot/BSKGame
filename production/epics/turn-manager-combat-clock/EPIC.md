# Epic: 回合管理器与战斗时钟

> **Layer**: Core  
> **GDD**: `design/gdd/回合管理器.md`  
> **Architecture Module**: Turn Manager + `CombatClockMs` + drain 协作  
> **Status**: Ready  
> **Stories**: see table below

## Overview

实现显式输入阶段机、与 `CombatClockMs` 对齐的 tick 推进、与事件总线同 tick drain 的确定性排序（ADR-0004），以及 `BattleInputPhaseChanged` 等阶段广播；禁止 gameplay 在事件总线内独立 drain。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0004: 战斗时钟与确定性战斗排序 | `CombatClockMs` + drain 优先级 | HIGH（低 FPS / 触摸） |
| ADR-0002: 战斗事件总线 | 同 tick drain 协作 | MEDIUM |
| ADR-0006 | 输入时间戳映射消费战斗时钟 | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-turn-001 | 显式输入阶段机 | ADR-0004 |
| TR-turn-002 | `BattleInputPhaseChanged` 事件 | ADR-0002, ADR-0004 |
| TR-turn-003 | `CombatClockMs` 推进与 tick 对齐 | ADR-0004 |
| TR-turn-004 | 与事件总线同 tick drain 的确定性排序 | ADR-0002, ADR-0004 |

## Definition of Done

- 四段 story 全部关闭且满足 GDD Acceptance Criteria  
- 含低 FPS / 同 tick 多事件的排序回归用例  

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 显式战斗输入阶段状态机 | Logic | Ready | ADR-0004 |
| 002 | BattleInputPhaseChanged 事件发布 | Integration | Ready | ADR-0002, ADR-0004 |
| 003 | CombatClockMs 推进与只读暴露 | Logic | Ready | ADR-0004 |
| 004 | 同 tick 事件 drain 与 ADR-0004 顺序编排 | Integration | Ready | ADR-0004, ADR-0002 |

## Next Step

与 `battle-event-bus-dto-versioning`、`battle-input-touch-and-hit-areas` 联调。

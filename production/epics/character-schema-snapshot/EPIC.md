# Epic: 角色数据模型与战斗 UI 快照

> **Layer**: Core  
> **GDD**: `design/gdd/角色数据模型.md`  
> **Architecture Module**: Character Schema + `CharacterBattleSnapshot`  
> **Status**: Ready  
> **Stories**: see table below

## Overview

实现权威角色 schema（HP/MP/护盾/状态/弹反窗口等）、`CharacterBattleSnapshot` 不可变快照与版本递增、以及 UI 只读投影规则；禁止 UI 直接写 gameplay 权威状态。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0011: 角色数据模型与战斗 UI 快照 | Schema + 快照 + UI 投影 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-char-001 | 权威角色 schema | ADR-0011 |
| TR-char-002 | `CharacterBattleSnapshot` 不可变 + 版本 | ADR-0011 |
| TR-char-003 | UI 只读投影 | ADR-0011, ADR-0010 |

## Definition of Done

- 快照与 schema 满足 GDD Acceptance Criteria 对应条目  
- EditMode 可验证版本递增与不可变投影  

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 权威角色 Schema | Logic | Ready | ADR-0011 |
| 002 | CharacterBattleSnapshot 不可变与版本 | Logic | Ready | ADR-0011 |
| 003 | UI 只读投影与快照消费 | UI | Ready | ADR-0011, ADR-0010 |

## Next Step

与 `damage-hp-ownership`、`battle-event-bus-dto-versioning` 联调事件载荷。

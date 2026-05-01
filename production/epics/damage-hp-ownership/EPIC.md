# Epic: 伤害与生命系统（权威归属）

> **Layer**: Core  
> **GDD**: `design/gdd/伤害与生命系统.md`  
> **Architecture Module**: Damage / HP / Shield / `DamagePipeline`  
> **Status**: Ready  
> **Stories**: see table below

## Overview

实现权威伤害管线（预检、护盾吸收、HP 扣减、死亡）、`DamageApplied` / `DamageRejected` / `CharacterDefeated` 事件形态，以及 UI 只读绑定；禁止 UI 直接改 HP。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0005: 伤害与生命系统的权威归属 | `DamagePipeline` + 事件 | LOW |
| ADR-0011 | 角色 schema 与快照消费 | LOW |
| ADR-0002 | 事件 DTO 与总线 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-dmg-001 | 权威 `DamagePipeline` | ADR-0005 |
| TR-dmg-002 | 护盾吸收与 HP 扣减顺序 | ADR-0005 |
| TR-dmg-003 | 死亡与 `CharacterDefeated` | ADR-0005 |
| TR-dmg-004 | `DamageApplied` / `DamageRejected` 事件 | ADR-0002, ADR-0005 |

## Definition of Done

- 伤害管线与事件满足 GDD Acceptance Criteria  
- EditMode 覆盖护盾边界与拒绝路径  

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 权威 DamagePipeline 与 DamageRequestId | Logic | Ready | ADR-0005 |
| 002 | 护盾吸收与 HP 扣减顺序 | Logic | Ready | ADR-0005 |
| 003 | DamageApplied / DamageRejected / CharacterDefeated 事件 | Integration | Ready | ADR-0005, ADR-0002 |

## Next Step

与 `character-schema-snapshot`、`battle-event-bus-dto-versioning` 联调。

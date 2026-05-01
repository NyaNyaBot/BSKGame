# Epic: 战斗反馈系统

> **Layer**: Presentation
> **GDD**: `design/gdd/战斗反馈系统.md`
> **Architecture Module**: Battle Feedback — 反馈 profile、降级、hit stop 请求
> **Status**: Ready
> **Stories**: see table below

## Overview

实现战斗事件到视听反馈的映射管线：订阅弹反/伤害/行动/读招事件，查表选择反馈 profile 和强度，构建 `FeedbackRequest` 和 `HitStopRequest`，并在微信 WebGL 性能预算内按质量层降级。反馈系统不拥有任何玩法事实——不判定弹反、不修改 HP、不推进时钟。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0009: 战斗反馈质量分级与 WebGL 降级策略 | 反馈在 Unity 热更层；质量 tier cap 粒子/shake/flash/vibration/audio；降级不改战斗结果；hit stop 由回合管理器裁决 | MEDIUM |
| ADR-0004: 战斗时钟与确定性战斗排序 | hit stop 暂停 CombatClockMs；表现可用 unscaled time | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-fx-001 | 只消费事件，绝不修改战斗事实 | ADR-0009 |
| TR-fx-002 | 发起 hit stop 请求，由回合管理器控制 CombatClockMs | ADR-0009, ADR-0004 |
| TR-fx-003 | VFX 和震动可按性能等级降级，但不得改变结果 | ADR-0009 |

## Definition of Done

- 三段 story 全部关闭且满足 GDD Acceptance Criteria
- 含 Perfect/Normal/Failed 差异反馈 profile 的 PlayMode 测试
- 含降级模式下战斗结果不变的回归用例
- 含 WebGL 设备性能证据（或显式 gap 标记）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 反馈事件消费与 profile 查表 | Logic | Ready | ADR-0009 |
| 002 | HitStopRequest 构建与提交 | Integration | Ready | ADR-0009, ADR-0004 |
| 003 | 性能降级与质量分层 | Integration | Ready | ADR-0009 |

## Next Step

与 `parry-system`（ParryResolved 事件）、`damage-hp-ownership`（DamageApplied 事件）、`turn-manager-combat-clock`（hit stop 裁决）联调。

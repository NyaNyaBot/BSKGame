# Epic: 实时弹反系统

> **Layer**: Feature
> **GDD**: `design/gdd/实时弹反系统.md`
> **Architecture Module**: Parry — 冻结弹反时间轴、弹反结果结算、counter 入口开启
> **Status**: Ready
> **Stories**: see table below

## Overview

实现核心弹反判定机制：从 `AttackSegmentTimelineSeed` 派生并冻结弹反时间轴，在 `CombatClockMs` 时间域内结算 `ParryAttempt` 为 `PerfectParry / NormalParry / FailedParry`，发布弹反结果事件和 counter 入口事件，并保持每个攻击段最多一次终结结果的幂等性。弹反系统只判定，不写 HP、不推进回合、不播放表现。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0007: 弹反时间轴派生与 Counter Handoff | 纯 C# IParryResolver 拥有冻结时间轴和结算；Perfect 发 CounterEntryOpened；counter 关闭由回合管理器拥有 | HIGH（低 FPS / 触摸时序） |
| ADR-0004: 战斗时钟与确定性战斗排序 | 所有弹反判定使用 CombatClockMs；hit stop 暂停逻辑时间 | HIGH |
| ADR-0006: 移动触摸时间戳与输入触区策略 | battleTimestampMs 映射；counter 优先级 300 > parry 100 | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-parry-001 | 从 AttackSegmentTimelineSeed 派生并冻结弹反窗口 | ADR-0007 |
| TR-parry-002 | 用 CombatClockMs 而不是 Unity 帧时间结算 ParryAttempt | ADR-0004, ADR-0007 |
| TR-parry-003 | 发出 ParryResolved、ParryCancelled、OverlapRejected 和时间轴拒绝事件 | ADR-0007 |
| TR-parry-004 | 只由 PerfectParry 打开 counter，且不直接写 HP | ADR-0007 |

## Definition of Done

- 四段 story 全部关闭且满足 GDD Acceptance Criteria
- 含窗口边界（early/perfect/late/none）的确定性测试用例
- 含低 FPS / hit stop / counter handoff 的集成回归用例

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | FrozenAttackSegmentTimeline 派生与验证 | Logic | Ready | ADR-0007 |
| 002 | ParryAttempt 结算与 grade 判定 | Logic | Ready | ADR-0007, ADR-0004 |
| 003 | 弹反结果与异常事件发布 | Integration | Ready | ADR-0007 |
| 004 | Counter 入口开启与关闭生命周期 | Integration | Ready | ADR-0007, ADR-0006 |

## Next Step

与 `battle-input-touch-and-hit-areas`（输入采样）、`turn-manager-combat-clock`（攻击段 seed 与阶段）、`damage-hp-ownership`（damageMultiplier 消费）联调。

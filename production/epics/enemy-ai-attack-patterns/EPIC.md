# Epic: 敌人 AI 与攻击模式

> **Layer**: Feature
> **GDD**: `design/gdd/敌人 AI 与攻击模式.md`
> **Architecture Module**: Enemy AI / Attack Patterns — 攻击模式选择、内容事实、连击中断
> **Status**: Ready
> **Stories**: see table below

## Overview

实现 MVP 脚本化敌人 AI：按权重、冷却、HP 阶段和目标合法性选择攻击模式，将攻击段内容事实提交给回合管理器生成 `AttackSegmentTimelineSeed`，并响应弹反、counter、重叠和取消事件。AI 不拥有调度权威、不修改 HP、不驱动弹反窗口，使用确定性 RNG 保证可回放测试。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0008: 敌方攻击模式数据与调度所有权 | 模式拥有内容事实；回合管理器拥有调度身份；确定性 RNG；禁止 GOAP/BT for MVP | LOW |
| ADR-0007: 弹反时间轴派生与 Counter Handoff | AI 提交 seed，弹反系统派生冻结时间轴；PerfectParry 按 comboInterruptRule 影响后续段 | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-enemy-001 | 按权重、冷却、HP 阶段和目标合法性选择脚本化攻击模式 | ADR-0008 |
| TR-enemy-002 | 拥有基础伤害、读招时序、连击规则等攻击内容事实 | ADR-0008 |
| TR-enemy-003 | 响应弹反、取消、重叠和 counter 事件，不得把被拒绝攻击转为必中 | ADR-0008, ADR-0007 |

## Definition of Done

- 三段 story 全部关闭且满足 GDD Acceptance Criteria
- 含固定种子确定性 pattern 选择的回放测试
- 含 OverlapRejected 重排/取消、PerfectParry comboInterruptRule 的集成用例

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 攻击模式选择器（权重/冷却/HP 阶段） | Logic | Ready | ADR-0008 |
| 002 | AttackSegment 内容事实与 seed 提交 | Logic | Ready | ADR-0008 |
| 003 | 弹反/counter/重叠事件响应 | Integration | Ready | ADR-0008, ADR-0007 |

## Next Step

与 `turn-manager-combat-clock`（调度身份与阶段）、`parry-system`（seed → 冻结时间轴 → 结果事件）联调。

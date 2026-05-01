# Epic: 技能与行动系统

> **Layer**: Feature
> **GDD**: `design/gdd/技能与行动系统.md`
> **Architecture Module**: Action System — BasicAttackAction、CounterAction、行动幂等
> **Status**: Ready
> **Stories**: see table below

## Overview

实现 MVP 两条行动路径：`BasicAttackAction`（PlayerCommand 阶段普通攻击）和 `CounterAction`（PerfectParry 后的反击窗口）。行动系统校验输入阶段、角色/目标合法性和 counter 授权，生成 `DamageRequest` 提交给伤害与生命系统，并保持行动请求幂等。行动系统不直接写 HP、不推进回合、不决定弹反结果。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0012: 技能与行动服务边界 | IBattleActionService 拥有授权/幂等/目标合法/DamageRequest 转换；MVP 只 BasicAttack + CounterAction | LOW |
| ADR-0005: 伤害与 HP 所有权 | 行动通过 DamageRequest 提交伤害；integer basis points multiplier | LOW |
| ADR-0007: 弹反时间轴派生与 Counter Handoff | CounterAction 必须引用 CounterEntryOpened 来源 | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-action-001 | MVP 只支持 BasicAttackAction 和 CounterAction | ADR-0012 |
| TR-action-002 | 通过 DamageRequest 提交伤害，并保持行动请求幂等 | ADR-0012, ADR-0005 |
| TR-action-003 | 只有收到 CounterEntryOpened 授权后才能执行 counter | ADR-0012, ADR-0007 |

## Definition of Done

- 三段 story 全部关闭且满足 GDD Acceptance Criteria
- 含行动幂等、counter 授权校验、阶段匹配拒绝的单元测试
- 含 PlayerCommand → BasicAttack → DamageRequest 和 PerfectParry → CounterEntryOpened → CounterAction → DamageRequest 的端到端集成用例

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | BasicAttackAction 授权与 DamageRequest 提交 | Logic | Ready | ADR-0012, ADR-0005 |
| 002 | CounterAction 授权与 counter 消耗 | Logic | Ready | ADR-0012, ADR-0007 |
| 003 | 行动幂等与场景/阶段过期拒绝 | Integration | Ready | ADR-0012 |

## Next Step

与 `turn-manager-combat-clock`（输入阶段授权）、`parry-system`（CounterEntryOpened）、`damage-hp-ownership`（DamageRequest 消费）联调。

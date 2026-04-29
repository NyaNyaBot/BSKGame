# 敌人 AI 与攻击模式

> **Status**: Approved
> **Author**: Sulequan + Agents
> **Last Updated**: 2026-04-27
> **Implements Pillar**: 精准即快感；层层递进；小而精致

## Summary

敌人 AI 与攻击模式系统负责决定敌人在敌方阶段使用哪个攻击、攻击目标是谁、攻击段如何排布，以及每个攻击段提供给回合管理器和实时弹反系统的内容事实。它不拥有弹反判定、不修改 HP、不推进回合时钟；它提供可读招、可测试、可逐层递进的攻击脚本，让玩家通过观察前摇和节奏掌握敌人。

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `回合管理器`, `角色数据模型`, `伤害与生命系统`, `实时弹反系统`

## Overview

MVP 使用轻量脚本化 AI，而不是复杂规划器。每个敌人拥有 1-3 个 `AttackPattern`，每个 pattern 由一个或多个 `AttackSegment` 组成。AI 在敌方阶段根据当前 HP、回合数、上次使用攻击和目标合法性选择 pattern，然后把段内容事实提交给回合管理器生成 `AttackSegmentTimelineSeed`。所有可弹反攻击必须遵守实时弹反系统的窗口与重叠规则；MVP 禁止两个可弹反段同时 active。

## Player Fantasy

玩家应该觉得敌人不是随机掷骰，而是在用越来越复杂的节奏考验自己。第一层敌人前摇清楚、节奏稳定；后续敌人会更快、更会连击，但仍然公平可读。Boss 可以施加压力，但不能作弊：每一段攻击都必须给出独立读招和明确窗口，让玩家从“挨打学习”成长到“读懂节奏”。

## Detailed Design

### Core Rules

1. **MVP AI 是脚本选择器。** 不做 GOAP、行为树或学习型 AI；按权重、冷却、HP 阶段和回合数选择攻击模式。
2. **攻击模式拥有内容事实。** `baseDamage`、`windupStartMs`、`visualImpactMs`、`damageCommitMs`、`recoveryEndMs`、`profileId`、`comboInterruptRule` 由攻击模式定义。
3. **回合管理器拥有调度事实。** `sceneContextId`、`battleContextId`、`attackSegmentId`、`timelineSequenceId` 由回合管理器分配或确认。
4. **实时弹反系统拥有窗口派生与判定。** AI 不直接写 `windowOpenMs`、`perfectStartMs`、`parryCenterMs` 等冻结窗口字段。
5. **MVP 攻击默认可弹反。** `isParryable=false` 只允许测试或 Boss 特殊段；MVP 内容不依赖不可弹反攻击制造难度。
6. **多段连击必须独立可读。** 每个 segment 都有自己的前摇、命中点、伤害提交点和弹反窗口配置。
7. **重叠可弹反段必须重排或取消。** 收到 `OverlapRejected` 后，攻击模式同 tick 选择 `RescheduleWithNewTimelineSequence` 或 `CancelSegment`，不得转为必中。
8. **Perfect 必须影响攻击模式。** 收到 `ParryResolved(PerfectParry)` 后按 `comboInterruptRule` 中断、插入 counter 或继续。
9. **目标选择只使用角色快照。** AI 不读取 Unity 对象状态，不攻击 `CanBeTargeted=false` 或 `CurrentHp<=0` 的目标。
10. **MVP 目标通常是唯一玩家角色。** 接口仍使用列表，为 Vertical Slice 三人队伍保留空间。

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | 非敌方阶段或敌人不可行动 | 回合管理器进入 `EnemyPlanning` | 不提交攻击 |
| `SelectingPattern` | 敌方阶段开始且敌人可行动 | 选中 pattern 或无可用 pattern | 按权重/冷却/阶段选择 |
| `SubmittingSegment` | pattern 有下一段 | seed 被接受、拒绝或取消 | 提交内容事实给回合管理器 |
| `WaitingForResolution` | 段 active | `ParryResolved` / `ParryCancelled` / damage complete | 等待段结果 |
| `CounterInterrupted` | Perfect + interrupt/counter rule | counter closed 或 pattern cancel | 暂停/取消后续段 |
| `PatternComplete` | 所有段完成或取消 | 回合管理器进入轮末 | 清理 cooldown 和记录 |

### Interactions with Other Systems

| System | Direction | Interface / Responsibility | Status |
|---|---|---|---|
| 回合管理器 | Upstream / Peer | 请求 enemy action、分配 attack id/timeline id、管理 active 段 | Approved |
| 角色数据模型 | Upstream | 提供敌人/玩家快照和目标合法性 | Approved |
| 伤害与生命系统 | Downstream | 消费 segment 的 `baseDamage` 生成伤害请求 | Approved |
| 实时弹反系统 | Peer | 消费 seed，回传 parry/cancel/overlap/counter 事件 | Approved |
| 战斗反馈系统 | Downstream | 消费 windup、attack、cancel、hit、parry 事件播放表现 | Approved |
| 战斗 UI | Downstream | 可显示调试攻击名、目标、阶段，不显示精确窗口 | Approved |

### Data Contracts

`AttackPatternDefinition`:

| Field | Type | Required | Description |
|---|---|---:|---|
| `attackPatternId` | string/int | Yes | 攻击模式 ID |
| `enemyId` | string/int | Yes | 所属敌人定义 |
| `weight` | int | Yes | 选择权重 |
| `cooldownTurns` | int | Yes | 使用后冷却 |
| `minRound` | int | No | 最早出现轮次 |
| `hpPhase` | enum | No | `Any / AboveHalf / BelowHalf / BossPhase2` |
| `segments` | list | Yes | 攻击段列表 |

`AttackSegmentDefinition`:

| Field | Type | Required | Description |
|---|---|---:|---|
| `segmentLocalId` | string/int | Yes | pattern 内段 ID |
| `comboIndex` | int | Yes | 从 0 开始 |
| `baseDamage` | int | Yes | 基础伤害 |
| `profileId` | enum/string | Yes | `Tutorial / Standard / Fast` |
| `isParryable` | bool | Yes | 是否弹反 |
| `windupDurationMs` | long | Yes | 相对当前段开始 |
| `visualImpactDelayMs` | long | Yes | 表现命中点 |
| `damageCommitDelayMs` | long | Yes | 伤害提交点 |
| `recoveryDurationMs` | long | Yes | 恢复段 |
| `comboInterruptRule` | enum | Yes | `InterruptOnPerfect / CounterInsertOnPerfect / ContinueAfterPerfect` |
| `telegraphCueId` | string | No | 反馈/音频 cue |

## Formulas

### Pattern Score

The `pattern_score` formula is defined as:

`pattern_score = weight if cooldown_remaining == 0 && round_index >= min_round && hp_phase_matches && has_valid_target else 0`

**Variables:**
| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| 权重 | `weight` | int | `0-100` | 配置权重 |
| 冷却剩余 | `cooldown_remaining` | int | `0-99` | 回合数 |
| 当前轮次 | `round_index` | int | `1-99` | 回合管理器 |
| 最小轮次 | `min_round` | int | `1-99` | 配置 |
| HP 阶段匹配 | `hp_phase_matches` | bool | `true/false` | 敌人当前 HP 阶段 |
| 有合法目标 | `has_valid_target` | bool | `true/false` | 目标快照合法 |

**Output Range:** `0-100`。  
**Example:** 权重 60、无冷却、轮次满足、目标合法，输出 60。

### Segment Absolute Timing

The `segment_absolute_timing` formula is defined as:

`absolute_time_ms = segment_start_ms + segment_relative_delay_ms`

**Variables:**
| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| 段开始时间 | `segment_start_ms` | long | `>=0` | `CombatClockMs` |
| 相对延迟 | `segment_relative_delay_ms` | long | `0-10000` | windup/impact/damage/recovery delay |

**Output Range:** `>= segment_start_ms`。  
**Example:** 段开始 1000ms，damage delay 900ms，则 `damageCommitMs=1900`。

## Edge Cases

- **If 没有合法目标**: pattern score 为 0，AI 不提交攻击，回合管理器记录 `EnemyActionUnavailable`。
- **If 所有 pattern 分数为 0**: MVP 跳过敌方行动并记录配置风险。
- **If 重排后仍与 active 段重叠**: 取消该段，不允许必中替代。
- **If `ContinueAfterPerfect` 后续段会遮蔽 counter**: 必须延后或取消后续段。
- **If `isParryable=false`**: 必须提供 `nonParryableReason`，MVP 正式内容默认不使用。
- **If Boss 进入新阶段**: pattern 选择可切换 hpPhase，但阶段切换叙事/演出不由本系统最终定义。
- **If 攻击目标在 windup 中死亡或离场**: 取消当前段并通知回合管理器/弹反系统。

## Dependencies

| System | Direction | Type | Interface / Reason |
|---|---|---|---|
| 回合管理器 | Upstream / Peer | Hard | 敌方阶段、时钟、seed 调度 |
| 角色数据模型 | Upstream | Hard | 目标选择和敌人状态 |
| 伤害与生命系统 | Downstream | Hard | `baseDamage` 进入伤害请求 |
| 实时弹反系统 | Peer | Hard | 弹反、counter、overlap 事件 |
| 战斗反馈系统 | Downstream | Hard for validation | 读招表现 |
| 战斗 UI | Downstream | Soft | 调试和状态显示 |

## Tuning Knobs

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|---|---:|---:|---|---|
| `pattern_weight.BasicStrike` | `60` | `0-100` | 更常见基础攻击 | 更少出现 |
| `pattern_weight.HeavyStrike` | `30` | `0-100` | 更多慢重击 | 节奏更稳定 |
| `pattern_weight.FastCombo` | `10` | `0-100` | 更多高压连击 | MVP 更简单 |
| `max_segments_per_pattern` | `3` | `1-6` | 更复杂连击 | 更易读 |
| `reschedule_delay_ms` | `300` | `100-800` | 重排更安全 | 节奏更紧 |

## Visual/Audio Requirements

攻击模式必须为每个正式攻击提供可读 telegraph cue。视觉/音频表现由反馈系统实现，但本系统必须提供 cue id、攻击类型和段事件。

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `EnemyPatternSelected` | debug-only 可显示攻击名 | 无 | Low |
| `EnemyWindUpStarted` | 武器/身体前摇、方向线索 | 低频蓄压或节拍 | Critical |
| `AttackSegmentCancelled` | 高亮信号淡出 | cue fade out | High |
| `AttackPatternComplete` | 敌人恢复待机 | 轻回落 | Medium |

## UI Requirements

正式 UI 不显示攻击名称或精确倒计时。开发构建可显示 pattern id、segment id、target id、profileId 和 `comboInterruptRule`。

## Acceptance Criteria

- **GIVEN** 敌方阶段开始且有合法目标，**WHEN** 至少一个 pattern score > 0，**THEN** AI 选择一个可用 pattern 并提交第一段 seed。
- **GIVEN** pattern 处于冷却，**WHEN** 计算分数，**THEN** 分数为 0。
- **GIVEN** 提交 `AttackSegmentTimelineSeed`，**WHEN** 回合管理器接收，**THEN** seed 包含 parry GDD 要求的所有内容事实。
- **GIVEN** 收到 `ParryResolved(PerfectParry)` 且规则为 `InterruptOnPerfect`，**WHEN** pattern 有后续段，**THEN** 后续段取消。
- **GIVEN** 收到 `OverlapRejected`，**WHEN** 当前段可重排，**THEN** 用新 `timelineSequenceId` 重排；否则取消，不得必中。
- **GIVEN** 目标在 windup 中变为不可受击，**WHEN** 段尚未结算，**THEN** 攻击取消并不制造失败弹反。
- **GIVEN** 连续 100 次 AI 选择测试，**WHEN** 固定随机种子和角色快照，**THEN** pattern 选择 deterministic。

## Open Questions

| Question | Owner | Target Resolution | Resolution |
|---|---|---|---|
| Boss 阶段切换是否需要独立 Boss GDD？ | Game Designer | Boss 设计前 | Open |
| MVP 三种敌人的正式 pattern 名称和数值是多少？ | Game Designer | 原型数据表 | Open |

### Test Evidence Matrix

| Evidence Type | Covers |
|---|---|
| EditMode automated | pattern score、target legality、seed completeness、overlap handling |
| Integration automated | 与回合/弹反/伤害系统事件顺序 |
| Manual playtest | 读招公平性、节奏递进、连击可理解性 |

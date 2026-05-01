# Epic: 战斗 UI

> **Layer**: Presentation
> **GDD**: `design/gdd/战斗 UI.md`
> **Architecture Module**: Battle UI — HUD view model、触区面板、debug overlay
> **Status**: Ready
> **Stories**: see table below

## Overview

实现基于 UIFrame Window + Panel 架构的战斗 HUD：通过事件和快照驱动 HP、阶段、行动可用和 counter 状态显示，通过输入系统注册/管理弹反和 counter 触区（counter 优先级 300 > parry 100），并在正式 HUD 中不显示精确弹反倒计时。场景卸载时清理所有触区注册和事件订阅。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0010: UIFrame 战斗 HUD 组成方式 | Window + Panels 组合；UI 注册/注销触区生命周期；事件驱动不轮询 | MEDIUM |
| ADR-0006: 移动触摸时间戳与输入触区策略 | Counter 300 > UI Button 200 > Parry 100；counter active 时 parry 不 block | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-ui-001 | 从事件和快照渲染状态，不直接读取可变玩法对象 | ADR-0010 |
| TR-ui-002 | 通过输入系统注册触区，counter 优先级高于 parry | ADR-0010, ADR-0006 |
| TR-ui-003 | 不显示精确弹反倒计时；时机可读性来自敌人与反馈 cue | ADR-0010 |

## Definition of Done

- 三段 story 全部关闭且满足 GDD Acceptance Criteria
- 含 HUD 生命周期、事件绑定、触区注册/注销的 PlayMode 测试
- 含安全区和触区尺寸的 WebGL 设备证据（或显式 gap 标记）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | BattleHudWindow 生命周期与事件订阅 | UI | Ready | ADR-0010 |
| 002 | 触区注册与优先级管理 | Integration | Ready | ADR-0010, ADR-0006 |
| 003 | 战斗状态渲染与无倒计时约束 | UI | Ready | ADR-0010 |

## Next Step

与 `battle-input-touch-and-hit-areas`（触区注册）、`turn-manager-combat-clock`（阶段事件）、`parry-system`（counter 入口）、`battle-feedback`（反馈不遮蔽协调）联调。

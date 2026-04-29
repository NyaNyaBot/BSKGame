# 战斗 UI

> **Status**: Approved
> **Author**: Sulequan + Agents
> **Last Updated**: 2026-04-27
> **Implements Pillar**: 精准即快感；角色即动力；小而精致

## Summary

战斗 UI 负责在战斗中展示角色生命、当前阶段、行动按钮、反击入口、暂停/胜负界面和必要的调试信息。它基于项目现有 UIFrame 的 Window + Panel 架构实现：战斗主 HUD 是战斗场景 Window 或常驻战斗 Screen 下的组合 UI，行动区、角色状态、敌人状态和 counter 入口可拆为 Panel。UI 不拥有判定、伤害、回合或反馈事实，只订阅事件并注册输入触区。

> **Quick reference** — Layer: `UI` · Priority: `MVP` · Key deps: `回合管理器`, `实时弹反系统`, `伤害与生命系统`, `技能与行动系统`, `输入系统`, `战斗反馈系统`

## Overview

MVP 战斗 UI 的目标是清楚、克制、可触摸。它必须告诉玩家当前是否能行动、角色 HP 是否危险、Perfect 后反击是否可用、战斗是否胜负，但不能用显眼 QTE 圈或精确倒计时代替敌人读招。弹反操作区域由 UI 注册到输入系统，但弹反结果由实时弹反系统判定；UI 只显示状态和可用性。

## Player Fantasy

玩家应该感觉界面在帮自己专注战斗，而不是把动作游戏变成按 UI 提示。HUD 提供安全感：我知道角色还剩多少血、现在能不能行动、完美弹反后哪里可以反击。真正的时机判断仍来自敌人动作和反馈。

## Detailed Design

### Core Rules

1. **UI 不拥有玩法事实。** HP、阶段、行动可用、counter 可用、弹反结果均来自下游系统事件。
2. **MVP 不显示精确弹反窗口倒计时。** 正式 HUD 不显示毫秒计时、QTE 圈或“现在按”提示。
3. **触区必须通过输入系统注册。** UI 创建/更新 `InputHitArea`，输入系统负责命中、优先级、去重和时间戳。
4. **Counter 触区优先级高于弹反触区。** `CounterEntryOpened` 后，counter area 注册更高 priority，弹反触区释放 raycast。
5. **移动端热区要足够大且避开安全区。** 弹反/counter 触区不得小于输入系统最小 dp。
6. **UIFrame 分层必须清楚。** 战斗主界面为 Window/Screen；子模块用 Panel；暂停和胜负可用 Window/Overlay。
7. **UI 更新必须事件驱动。** HP、阶段、行动可用、counter 状态通过事件或快照刷新，不每帧轮询可变对象。
8. **调试信息和正式 UI 分离。** Debug overlay 可显示 `CombatClockMs`、phase、input phase、attack id；正式构建默认关闭。
9. **场景卸载必须清理触区和订阅。** 防止迟到 UI 输入污染下一场战斗。

### States and Transitions


| State              | Entry Condition      | Exit Condition   | Behavior               |
| ------------------ | -------------------- | ---------------- | ---------------------- |
| `Hidden`           | 非战斗场景或 UI 未初始化       | `BattleStarted`  | 不注册战斗触区                |
| `Entering`         | 战斗开始                 | HUD 淡入完成         | 注册基础 HUD，读取初始快照        |
| `PlayerCommand`    | 回合进入玩家指令             | 行动提交或暂停          | 显示基础行动按钮               |
| `EnemyAttack`      | 敌方读招/攻击阶段            | 攻击结算或取消          | 淡化行动按钮，弹反触区 active     |
| `CounterAvailable` | `CounterEntryOpened` | counter 消耗/过期/取消 | 显示并注册高优先级 counter area |
| `Suspended`        | 暂停/后台                | 恢复或退出            | 显示暂停 overlay，禁用战斗触区    |
| `BattleResult`     | 胜利/失败                | 重试/退出            | 显示结果和入口                |
| `Disposed`         | 场景卸载                 | 新战斗              | 清理 UI 订阅和输入区域          |


### Interactions with Other Systems


| System  | Direction | Interface / Responsibility               | Status   |
| ------- | --------- | ---------------------------------------- | -------- |
| 回合管理器   | Upstream  | 提供 phase、input phase、battle result、pause | Approved |
| 角色数据模型  | Upstream  | 提供角色 HP、状态、快照                            | Approved |
| 伤害与生命系统 | Upstream  | 提供 HP 变化、低血、死亡事件                         | Approved |
| 技能与行动系统 | Peer      | UI 提交 action/counter 输入，显示行动可用状态         | Approved |
| 实时弹反系统  | Upstream  | 提供窗口状态、最近结果、counter opened               | Approved |
| 输入系统    | Peer      | UI 注册/更新/注销 hit area                     | Approved |
| 战斗反馈系统  | Peer      | UI 与反馈协作，不遮蔽读招                           | Approved |
| 场景管理    | Upstream  | 战斗场景创建/卸载时打开/关闭 UI                       | Approved |


### Data Contracts

`BattleHudViewModel`:


| Field                             | Type        | Required | Description                                          |
| --------------------------------- | ----------- | -------- | ---------------------------------------------------- |
| `sceneContextId` / `sceneVersion` | id/int      | Yes      | 场景过滤                                                 |
| `battleContextId`                 | id          | Yes      | 战斗上下文                                                |
| `battlePhase`                     | enum        | Yes      | 当前回合阶段                                               |
| `inputPhase`                      | enum        | Yes      | `None / PlayerCommand / Parry / Counter / Suspended` |
| `playerHp` / `playerMaxHp`        | int         | Yes      | 玩家 HP                                                |
| `enemySummaries`                  | list        | Yes      | 敌人 HP/状态简表                                           |
| `canUseBasicAttack`               | bool        | Yes      | 基础行动可用                                               |
| `counterAvailable`                | bool        | Yes      | counter 是否可用                                         |
| `lastParryGrade`                  | enum/null   | No       | 最近弹反结果                                               |
| `debug`                           | object/null | No       | 开发构建信息                                               |


`BattleUiInputAreaConfig`:


| Field          | Type       | Required | Description                  |
| -------------- | ---------- | -------- | ---------------------------- |
| `areaId`       | string/int | Yes      | 输入区域                         |
| `inputKind`    | enum       | Yes      | `UiButton / Parry / Counter` |
| `rectDp`       | rect       | Yes      | 命中区域                         |
| `priority`     | int        | Yes      | Counter > UI Button > Parry  |
| `stateVersion` | int        | Yes      | UI 版本                        |


## Formulas

### HUD State

The `hud_state` formula is defined as:

`hud_state = BattleResult if battle_result != InProgress; else Suspended if input_phase == Suspended; else CounterAvailable if counter_available; else phase_mapped_state`

**Variables:**


| Variable   | Symbol               | Type | Range                     | Description     |
| ---------- | -------------------- | ---- | ------------------------- | --------------- |
| 战斗结果       | `battle_result`      | enum | InProgress/Victory/Defeat | 回合管理器           |
| 输入阶段       | `input_phase`        | enum | known phase               | 回合管理器           |
| counter 可用 | `counter_available`  | bool | true/false                | 弹反/回合事件         |
| 阶段映射       | `phase_mapped_state` | enum | HUD states                | battle phase 映射 |


**Output Range:** HUD state enum。  
**Example:** counter 可用时，即使 battle phase 是 CounterWindow，也显示 `CounterAvailable`。

### Hit Area Priority

The `hit_area_priority` formula is defined as:

`priority = 300 if inputKind == Counter; 200 if inputKind == UiButton; 100 if inputKind == Parry; else 0`

**Variables:**


| Variable | Symbol      | Type | Range                  | Description |
| -------- | ----------- | ---- | ---------------------- | ----------- |
| 输入类型     | `inputKind` | enum | Counter/UiButton/Parry | 区域类型        |


**Output Range:** `0-300`。  
**Example:** Counter 打开时优先命中 counter area，避免弹反触区吞输入。

## Edge Cases

- **If HUD 收到旧 sceneVersion 事件**: 丢弃，不刷新当前 UI。
- **If HP 事件到达但角色已不在当前 battle**: 丢弃并记录 debug warning。
- **If CounterEntryOpened 后弹反触区仍 blocksUnderlying**: 视为 UI 配置错误，必须修正。
- **If 安全区导致默认触区过小**: 自动内缩/重排，但不得小于输入系统最小热区。
- **If 暂停 overlay 打开**: 注销或禁用战斗触区，只保留菜单按钮。
- **If 低端设备 UI 动画掉帧**: 降低 HUD 动画，不影响输入区域注册。
- **If 多敌目标重叠**: MVP 可默认当前目标；多敌选择详细交互延后。

## Dependencies


| System  | Direction | Type | Interface / Reason |
| ------- | --------- | ---- | ------------------ |
| 回合管理器   | Upstream  | Hard | 阶段、暂停、胜负           |
| 实时弹反系统  | Upstream  | Hard | 弹反状态和 counter      |
| 伤害与生命系统 | Upstream  | Hard | HP/死亡              |
| 技能与行动系统 | Peer      | Hard | 行动提交               |
| 输入系统    | Peer      | Hard | hit area 注册        |
| 战斗反馈系统  | Peer      | Hard | 避免 UI 遮蔽反馈         |
| 场景管理    | Upstream  | Hard | UI 生命周期            |


## Tuning Knobs


| Parameter                  | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
| -------------------------- | ------------- | ---------- | ------------------ | ------------------ |
| `parry_hit_area_size_dp`   | `112`         | `96-140`   | 更易触达               | 更少遮挡               |
| `counter_hit_area_size_dp` | `120`         | `96-160`   | 更易反击               | 更克制                |
| `hud_fade_ms`              | `160`         | `0-300`    | 更柔和                | 更快                 |
| `result_screen_delay_ms`   | `500`         | `0-1000`   | 胜负更有停顿             | 更快进入重试             |
| `debug_overlay_enabled`    | `false`       | bool       | QA 信息更多            | 正式更干净              |


## Visual/Audio Requirements


| Event                | Visual Feedback  | Audio Feedback | Priority |
| -------------------- | ---------------- | -------------- | -------- |
| `BattleStarted`      | HUD 淡入           | 可选 UI cue      | High     |
| `PlayerCommand`      | 行动按钮亮起           | 轻提示            | High     |
| `EnemyAttack`        | 行动区淡化、弹反触区 ready | 无或轻 cue        | High     |
| `CounterEntryOpened` | Counter 入口闪现/高亮  | 上扬 cue request | Critical |
| `DamageApplied`      | HP 条扣减           | 由反馈/音频处理       | High     |
| `BattleEnded`        | 胜负窗口             | 胜负 cue request | Critical |


## UI Requirements

MVP UI 模块建议：


| Module                     | UIFrame Layer    | Responsibility |
| -------------------------- | ---------------- | -------------- |
| `BattleHudWindow` / Screen | Window / Screen  | 战斗 HUD 根节点     |
| `PlayerStatusPanel`        | Panel            | HP、低血、状态       |
| `EnemyStatusPanel`         | Panel            | 当前敌人简要 HP/状态   |
| `ActionCommandPanel`       | Panel            | BasicAttack 按钮 |
| `ParryTouchPanel`          | Panel            | 弹反触区注册与轻状态     |
| `CounterPromptPanel`       | Panel            | Counter 入口     |
| `BattleDebugPanel`         | Panel            | 开发构建信息         |
| `BattleResultWindow`       | Window / Overlay | 胜利/失败/重试       |


## Acceptance Criteria

- **GIVEN** `BattleStarted`，**WHEN** 战斗 UI 打开，**THEN** 初始化 HUD、订阅事件并注册必要输入区域。
- **GIVEN** 进入 `PlayerCommand`，**WHEN** 玩家可行动，**THEN** BasicAttack 按钮启用并可提交行动输入。
- **GIVEN** 进入敌方攻击阶段，**WHEN** `inputPhase=Parry`，**THEN** 弹反触区注册为 enabled，但正式 HUD 不显示精确倒计时。
- **GIVEN** `CounterEntryOpened`，**WHEN** counter 可用，**THEN** Counter 触区注册更高 priority，弹反触区不吞输入。
- **GIVEN** `DamageApplied`，**WHEN** HP 变化，**THEN** HUD 通过事件刷新 HP，不直接读取可变内部对象。
- **GIVEN** 暂停菜单打开，**WHEN** UI 进入 `Suspended`，**THEN** 战斗触区禁用，恢复后按最新 battle/input phase 重建。
- **GIVEN** 场景卸载，**WHEN** UI dispose，**THEN** 所有 hit area 注销，事件订阅清理。
- **GIVEN** 微信小游戏两种屏幕比例和安全区设备，**WHEN** 打开战斗 HUD，**THEN** 弹反和 counter 热区不小于最小 dp 且不被系统手势区遮挡。
- **GIVEN** HUD 关闭 debug overlay，**WHEN** 敌人读招发生，**THEN** 玩家仍可依赖敌人表现/音频而不是 UI 倒计时完成弹反判断。

## Open Questions


| Question             | Owner              | Target Resolution | Resolution                |
| -------------------- | ------------------ | ----------------- | ------------------------- |
| 多敌目标选择是点击敌人还是目标切换按钮？ | UX / Game Designer | 多敌原型              | Deferred — MVP 默认当前目标     |
| 伤害数字是否正式显示？          | UX / Feedback      | UI 原型后            | Deferred — MVP debug-only |
| 左/右手触区偏好是否需要存档？      | UX / 存档系统          | Alpha             | Open                      |


### Test Evidence Matrix


| Evidence Type         | Covers                                        |
| --------------------- | --------------------------------------------- |
| PlayMode automated    | HUD lifecycle、event binding、hit area registry |
| Integration automated | phase/action/counter/HP events 到 UI           |
| WebGL / 微信设备 evidence | 安全区、触区尺寸、输入优先级                                |
| Manual playtest       | UI 是否克制、可读、不遮挡敌人读招                            |

# Architecture Traceability Index

Last Updated: 2026-04-29  
Engine: Unity 2022.3.17f1

## Coverage Summary

- Total requirements: 38
- Covered: 38 (100%)
- Partial: 0
- Gaps: 0

## Full Matrix

| Requirement ID | GDD | System | Requirement | ADR Coverage | Status |
|---|---|---|---|---|---|
| TR-concept-001 | `game-concept.md` | 产品 | 支持“回合制 RPG + 敌方攻击中嵌入实时弹反”的核心循环 | ADR-0003 | Covered |
| TR-concept-002 | `game-concept.md` | 表现 | 表现层必须放大精准操作的快感，但不得成为规则事实来源 | ADR-0003, ADR-0009 | Covered |
| TR-char-001 | `角色数据模型.md` | 角色数据 | 所有跨系统角色引用使用稳定 gameplay id | ADR-0011 | Covered |
| TR-char-002 | `角色数据模型.md` | 角色数据 | 区分静态定义、运行时实例和不可变快照 | ADR-0011 | Covered |
| TR-char-003 | `角色数据模型.md` | 角色数据 | 提供 `CanAct`、`CanBeTargeted`、`CanReceiveDamage`、`IsControllable` 等独立查询标记 | ADR-0011, ADR-0005 | Covered |
| TR-char-004 | `角色数据模型.md` | 角色数据 | 角色被击败后保留实例，直到战斗或场景清理显式移除 | ADR-0011, ADR-0005 | Covered |
| TR-input-001 | `输入系统.md` | 输入 | 战斗输入来自 Touch / PointerDown，不使用 UGUI click 合成事件做判定源 | ADR-0006 | Covered |
| TR-input-002 | `输入系统.md` | 输入 | 输出结构化 `ParryAttempt`，并携带时间戳诊断信息 | ADR-0006, ADR-0007 | Covered |
| TR-input-003 | `输入系统.md` | 输入 | 将平台时间戳映射到 `CombatClockMs`，并标记 fallback 来源 | ADR-0004, ADR-0006 | Covered |
| TR-input-004 | `输入系统.md` | 输入 | 拥有触区注册、优先级、遮挡和重复输入过滤 | ADR-0006, ADR-0010 | Covered |
| TR-scene-001 | `场景管理.md` | 场景 | 拥有 `SceneContext`、`sceneContextId` 和 `sceneVersion` | ADR-0001 | Covered |
| TR-scene-002 | `场景管理.md` | 场景 | 场景切换期间冻结战斗输入并拒绝迟到事件 | ADR-0001, ADR-0002 | Covered |
| TR-scene-003 | `场景管理.md` | 场景 | 在战斗场景生命周期内创建和销毁战斗临时运行态 | ADR-0001 | Covered |
| TR-turn-001 | `回合管理器.md` | 回合管理 | 拥有 `CombatClockMs` 和战斗阶段状态 | ADR-0004 | Covered |
| TR-turn-002 | `回合管理器.md` | 回合管理 | 发布显式 `BattleInputPhaseChanged` 事件 | ADR-0002, ADR-0004 | Covered |
| TR-turn-003 | `回合管理器.md` | 回合管理 | 调度敌方攻击段并分配时间轴序列身份 | ADR-0004, ADR-0008 | Covered |
| TR-turn-004 | `回合管理器.md` | 回合管理 | 将 `CounterWindow` 作为一等战斗状态 | ADR-0004, ADR-0007 | Covered |
| TR-turn-005 | `回合管理器.md` | 回合管理 | 只读取角色生命状态判定胜负，绝不写 HP | ADR-0005 | Covered |
| TR-dmg-001 | `伤害与生命系统.md` | 伤害 / HP | 作为唯一 HP 写入口 | ADR-0005, ADR-0011 | Covered |
| TR-dmg-002 | `伤害与生命系统.md` | 伤害 / HP | 消费具备幂等键的 `DamageRequest` | ADR-0005 | Covered |
| TR-dmg-003 | `伤害与生命系统.md` | 伤害 / HP | 精确应用弹反 `damageMultiplier` | ADR-0005, ADR-0007 | Covered |
| TR-dmg-004 | `伤害与生命系统.md` | 伤害 / HP | 发出 `DamageApplied`、`DamageRejected` 和 `CharacterDefeated` | ADR-0002, ADR-0005 | Covered |
| TR-parry-001 | `实时弹反系统.md` | 弹反 | 从 `AttackSegmentTimelineSeed` 派生并冻结弹反窗口 | ADR-0007 | Covered |
| TR-parry-002 | `实时弹反系统.md` | 弹反 | 用 `CombatClockMs` 而不是 Unity 帧时间结算 `ParryAttempt` | ADR-0004, ADR-0006, ADR-0007 | Covered |
| TR-parry-003 | `实时弹反系统.md` | 弹反 | 发出 `ParryResolved`、`ParryCancelled`、`OverlapRejected` 和时间轴拒绝事件 | ADR-0002, ADR-0007 | Covered |
| TR-parry-004 | `实时弹反系统.md` | 弹反 | 只由 `PerfectParry` 打开 counter，且不直接写 HP | ADR-0005, ADR-0007 | Covered |
| TR-enemy-001 | `敌人 AI 与攻击模式.md` | 敌人 AI | 按权重、冷却、HP 阶段和目标合法性选择脚本化攻击模式 | ADR-0008 | Covered |
| TR-enemy-002 | `敌人 AI 与攻击模式.md` | 敌人 AI | 拥有基础伤害、读招时序、连击规则等攻击内容事实 | ADR-0008 | Covered |
| TR-enemy-003 | `敌人 AI 与攻击模式.md` | 敌人 AI | 响应弹反、取消、重叠和 counter 事件，不得把被拒绝攻击转为必中 | ADR-0007, ADR-0008 | Covered |
| TR-action-001 | `技能与行动系统.md` | 行动 | MVP 只支持 `BasicAttackAction` 和 `CounterAction` | ADR-0012 | Covered |
| TR-action-002 | `技能与行动系统.md` | 行动 | 通过 `DamageRequest` 提交伤害，并保持行动请求幂等 | ADR-0005, ADR-0012 | Covered |
| TR-action-003 | `技能与行动系统.md` | 行动 | 只有收到 `CounterEntryOpened` 授权后才能执行 counter | ADR-0007, ADR-0012 | Covered |
| TR-fx-001 | `战斗反馈系统.md` | 反馈 | 只消费事件，绝不修改战斗事实 | ADR-0002, ADR-0009 | Covered |
| TR-fx-002 | `战斗反馈系统.md` | 反馈 | 发起 hit stop 请求，由回合管理器控制 `CombatClockMs` | ADR-0004, ADR-0009 | Covered |
| TR-fx-003 | `战斗反馈系统.md` | 反馈 | VFX 和震动可按性能等级降级，但不得改变结果 | ADR-0009 | Covered |
| TR-ui-001 | `战斗 UI.md` | 战斗 UI | 从事件和快照渲染状态，不直接读取可变玩法对象 | ADR-0002, ADR-0010 | Covered |
| TR-ui-002 | `战斗 UI.md` | 战斗 UI | 通过输入系统注册触区，counter 优先级高于 parry | ADR-0006, ADR-0010 | Covered |
| TR-ui-003 | `战斗 UI.md` | 战斗 UI | 不显示精确弹反倒计时；时机可读性来自敌人与反馈 cue | ADR-0010 | Covered |

## Known Gaps

None.

## Superseded Requirements

None.

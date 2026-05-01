# Sprint 001: Core 核心系统

> **Status**: In Progress
> **Duration**: 1 周
> **Goal**: 实现 Core 层全部 Story — 回合管理器、角色数据模型、伤害与生命系统
> **Milestone**: Production Sprint 1 — 战斗核心逻辑可编码

---

## Sprint Scope

### Core 层 Stories

| # | Epic | Story | Type | Priority |
|---|---|---|---|---|
| 1 | turn-manager-combat-clock | story-001 输入阶段状态机 | Logic | P0 |
| 2 | turn-manager-combat-clock | story-002 BattleInputPhaseChanged 事件 | Integration | P0 |
| 3 | turn-manager-combat-clock | story-003 CombatClock 推进与只读暴露 | Logic | P0 |
| 4 | turn-manager-combat-clock | story-004 同 tick drain 编排 | Integration | P0 |
| 5 | character-schema-snapshot | story-001 权威角色 Schema | Logic | P0 |
| 6 | character-schema-snapshot | story-002 快照不可变与版本 | Logic | P0 |
| 7 | character-schema-snapshot | story-003 UI 只读投影 | UI | P1 |
| 8 | damage-hp-ownership | story-001 权威 DamagePipeline | Logic | P0 |
| 9 | damage-hp-ownership | story-002 护盾吸收与 HP 扣减 | Logic | P0 |
| 10 | damage-hp-ownership | story-003 伤害事件 | Integration | P0 |

---

## 实现顺序（依赖安全）

```
阶段 1: turn-manager story-001 + character story-001
         （输入阶段状态机 + 角色 schema，无交叉依赖）
    ↓
阶段 2: turn-manager story-002 + character story-002
         （阶段变更事件 + 快照版本，依赖阶段 1）
    ↓
阶段 3: turn-manager story-003 (验证 Sprint 000 已实现) + damage story-001
         （CombatClock 已存在 + 伤害管道，依赖 char story-001+002）
    ↓
阶段 4: turn-manager story-004 + damage story-002
         （同 tick drain 编排 + 护盾/HP 顺序）
    ↓
阶段 5: damage story-003
         （伤害事件 DTO + 总线发布）
    ↓
阶段 6: character story-003
         （UI 只读投影 — 可能需人工验证）
```

---

## Next Sprint

Sprint 002: Feature 层（parry-system + action-system + enemy-ai-attack-patterns）

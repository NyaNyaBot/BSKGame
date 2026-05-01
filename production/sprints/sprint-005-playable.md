# Sprint 005 — Playable Prototype

> **Status**: Complete
> **Start**: 2026-05-02
> **Layer**: Playable (可交互原型)
> **Prerequisite**: Sprint 004 Integration 已完成

## Goal

让战斗循环真正可交互：玩家点击/触摸 → 攻击敌人 → 敌人自动回击 → 回合推进 → 战斗结束判定。

## Completed

| # | Story | Status |
|---|-------|--------|
| 1 | BattleOrchestrator 敌人自动攻击 — ExecuteEnemyAttack + EnemyAttackPlanner | Done |
| 2 | BattleFlowController 玩家输入桥接 — 触摸/鼠标 → SubmitPlayerAction | Done |
| 3 | BattleFlowController 完善 — 敌人回合延迟 + 战斗结束日志 | Done |

## Battle Loop

```
PlayerCommand → 玩家点击 → SubmitPlayerAction(player, enemy)
  → DamageService.ApplyDamage → DamageApplied event
  → Phase → EnemyAction

EnemyAction → 0.6s 延迟 → AdvanceToResolution
  → ExecuteEnemyAttack (EnemyAttackPlanner → DamageService)
  → Phase → Resolution → TurnEnd
  → ShouldEndBattle? → BattleEnd / PlayerCommand
```

## 需要后续完善

- 角色 3D 模型加载与血条世界空间绑定
- 攻击动画 + 受击反馈（HitStop/粒子）
- ATK/DEF 字段添加到 DRProperty 数据表
- 弹反交互（ParryResolver + 触区）
